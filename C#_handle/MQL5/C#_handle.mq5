//+------------------------------------------------------------------+
//|                                                    C#_handle.mq5 |
//|                                  Copyright 2021, MetaQuotes Ltd. |
//|                                             https://www.mql5.com |
//+------------------------------------------------------------------+
#property copyright "Copyright 2021, MetaQuotes Ltd."
#property link      "https://www.mql5.com"
#property version   "1.00"
//+------------------------------------------------------------------+
//| Expert initialization function                                   |
//+------------------------------------------------------------------+
string inputfoldername = "C#_output";
string outputfoldername = "C#_input";
string pairslink = "CSI_config\\CSI_symbols\\pairsForPanels.txt";
string pairslink2 = "CSI_config\\CSI_symbols\\mainSymbols.txt";
datetime pairslink_ModDate1 = 0;
datetime pairslink_ModDate2 = 0;
string pairs[];
ENUM_TIMEFRAMES chart_timeframes[] = {PERIOD_M1,PERIOD_M5,PERIOD_M30,PERIOD_H4,PERIOD_D1, PERIOD_W1};
int dataLength = 2000;
datetime laikas; //reikalingas, kad outputas tikrintu tik kas minute

int OnInit()
{
   EventSetMillisecondTimer(1000);
   FolderClean(outputfoldername, FILE_COMMON);
   pairslinkRead();
//---
   return(INIT_SUCCEEDED);
}
//+------------------------------------------------------------------+
//| Expert deinitialization function                                 |
//+------------------------------------------------------------------+
void OnDeinit(const int reason)
{
   EventKillTimer();
}
//+------------------------------------------------------------------+
//| Expert tick function                                             |
//+------------------------------------------------------------------+
void OnTick()
{
//---
   if (MQLInfoInteger(MQL_TESTER))
   {
      rewriteOutput();
   }
}
//+------------------------------------------------------------------+
//| Timer function                                                   |
//+------------------------------------------------------------------+
void OnTimer()
{
   //---
   pairslinkRead();
   
   //Jei vyksta ne testas, rewriteOutput() vykdomas onTimer(), jei testas, tai OnTimer neissaukiamas
   rewriteOutput();
   //istrinmas senas 'active' failas
   string activename = "";
   bool successfulDelete = true;
   while(FileFindFirst((outputfoldername+"\\*.active"), activename, FILE_COMMON) != INVALID_HANDLE && successfulDelete)
   {
      successfulDelete = FileDelete(outputfoldername+"\\"+activename, FILE_COMMON);
   }
   //kuriamas 'active' failas, kuris rodo kad mql'as yra aktyvus
   activename = outputfoldername+"\\metatrader.active";
   int active = FileOpen(activename, FILE_WRITE|FILE_COMMON);
   if(active != INVALID_HANDLE)
   {
      FileWrite(active, ArraySize(pairs));
      for(int i=0; i<ArraySize(pairs); i++)
      {
         string symbol = pairs[i];
         SymbolSelect(symbol,true);
         FileWrite(active, (symbol+DoubleToString(iClose(symbol,PERIOD_M1,0), _Digits)));
      }
      FileClose(active);
   }
}
//+------------------------------------------------------------------+
void output(string symbol, ENUM_TIMEFRAMES taimf, int ilgis) //iraso nauja simbolio charta jei senas nesutampa
{
   //istrina senus failus
   int time = PeriodSeconds(taimf);
   bool reikiaIrasyti = false;
   int handle;
   string filename = outputfoldername+"\\"+time+"\\"+symbol+".order";
   datetime paskutineFailoModifikavimoData=0;
   paskutineFailoModifikavimoData = FileGetInteger(filename, FILE_MODIFY_DATE, true);
   //SymbolSelect(symbol, true);        neselectinam nes tai jau padaro rewriteOutput() funkcija
   if(symbol == "XAUUSD")
   {
      symbol = "GOLD";
   }
   //jei failo vidus sutampa ar failas neegzistuoja, sukuriamas naujas
   if(!FileIsExist(filename, FILE_COMMON) || (int)iTime(symbol,taimf,0) - (int)paskutineFailoModifikavimoData >= time)
   {
      if(FileIsExist(filename, FILE_COMMON))
      {
         FileDelete(outputfoldername+"\\"+time+"\\"+filename, FILE_COMMON);
      }
      //sudaro faila
      int failas = FileOpen(filename,FILE_WRITE|FILE_COMMON);
      if(failas != INVALID_HANDLE)
      {
         int rt = iBars(symbol,taimf);
         if(ilgis >= rt)
         {
            ilgis = rt-1;
         }
         FileWrite(failas, ilgis);
         for(int i=0; i<=ilgis; i++)
         {
            string eilute = DoubleToString(iLow(symbol,taimf,i),_Digits)+" "+DoubleToString(iHigh(symbol,taimf,i),_Digits)+" "+DoubleToString(iOpen(symbol,taimf,i),_Digits)+" "+DoubleToString(iClose(symbol,taimf,i),_Digits)+" "+iTime(symbol,taimf,i);
            FileWrite(failas, eilute);
         }
         FileClose(failas);
      }
   }
}

bool pairslinkRead()//skaito pairslink ir atnaujina poras, jei modifikacijos data nesutampa
{
   string pairs1[];
   string pairs2[];
   int symbolList;
   int modificationDate1;
   int modificationDate2;
   //skaito pirma faila
   modificationDate1 = FileGetInteger(pairslink, FILE_MODIFY_DATE, true);
   if(pairslink_ModDate1 != (datetime)modificationDate1)
   {
      symbolList = FileOpen(pairslink, FILE_READ|FILE_COMMON|FILE_TXT|FILE_ANSI);
      if(symbolList == INVALID_HANDLE)
      {
         return false;
      }
      int kiekis = (int)FileReadString(symbolList);
      ArrayResize(pairs1,kiekis);
      for(int i=0; i<kiekis; i++)
      {
         pairs1[i] = FileReadString(symbolList);
      }
      FileClose(symbolList);
   }
   //skaito antra faila
   symbolList = FileOpen(pairslink2, FILE_READ|FILE_COMMON|FILE_TXT|FILE_ANSI);
   if(symbolList == INVALID_HANDLE)
   {
      return false;
   }
   modificationDate2 = FileGetInteger(symbolList, FILE_MODIFY_DATE);
   if(pairslink_ModDate2 != (datetime)modificationDate2)
   {
      int kiekis = (int)FileReadString(symbolList);
      ArrayResize(pairs2,kiekis);
      for(int i=0; i<kiekis; i++)
      {
         pairs2[i] = FileReadString(symbolList);
      }
      FileClose(symbolList);
   }
   FileClose(symbolList);
   //jei nors vienas simboliu failas buvo pakeistas, pairs[] masyvas pakeiciamas, o visi output'o failai yra perasomi
   if(pairslink_ModDate1 != (datetime)modificationDate1 || pairslink_ModDate2 != (datetime)modificationDate2)
   {
      //abudu pairs1 ir pairs2 masyvai sujungiami i viena pairs[]
      //---------------------------------------------------------
      //is pradziu nustato bendro pairs[] masyvo dydi
      pairslink_ModDate1 = (datetime)modificationDate1;
      pairslink_ModDate2 = (datetime)modificationDate2;
      int masyvoDydis = ArraySize(pairs1) + ArraySize(pairs2);
      for(int x=0; x<ArraySize(pairs1); x++)
      {
         for(int y=0; y<ArraySize(pairs2); y++)
         {
            if(pairs1[x] == pairs2[y])
            {
               masyvoDydis--;
               break;
            }
         }
      }
      //tada sukuria nauja pairs[] masyva
      ArrayResize(pairs,masyvoDydis);
      ArrayCopy(pairs, pairs1);
      int vieta = ArraySize(pairs1); //vieta, nuo kurios prades irasineti pairs[] masyva
      for(int y=0; y<ArraySize(pairs2); y++)
      {
         for(int x=0; x<ArraySize(pairs1); x++)
         {
            if(pairs1[x] == pairs2[y])
            {
               break;
            }
            if(x+1 == ArraySize(pairs1))
            {
               pairs[vieta] = pairs2[y];
               vieta++;
            }
         }
      }
      //perasomi visi output'o failai
      for(int i=0; i<ArraySize(pairs); i++)
      {
         for(int tmf=0; tmf<ArraySize(chart_timeframes); tmf++)
         {
            output(pairs[i],chart_timeframes[tmf],dataLength);
         }
      }
   }
   return true;
}

void rewriteOutput() //pasirupina kad visu simboliu outputas butu irasytas
{
   if((int)TimeCurrent() - (int)laikas > 60+10)
   {
      
   }
   for(int i=0; i<ArraySize(pairs); i++)
   {
      if(pairs[i] != "XAUUSD")
         SymbolSelect(pairs[i], true);
      else
         SymbolSelect("GOLD", true);
      for(int tmf=0; tmf<ArraySize(chart_timeframes); tmf++)
      {
         output(pairs[i], chart_timeframes[tmf], dataLength);
      }
   }
   laikas = TimeCurrent();
   laikas -= TimeCurrent()%60;
   //Print(TimeCurrent());
}

int TaimfreimoNumeris(ENUM_TIMEFRAMES taim)
{
   for(int i=0; i<ArraySize(chart_timeframes); i++)
   {
      if(chart_timeframes[i] == taim)
      {
         return i;
      }
   }
   return 0;
}
