//+------------------------------------------------------------------+
//|                                                    C#_handle.mq4 |
//|                        Copyright 2020, MetaQuotes Software Corp. |
//|                                             https://www.mql5.com |
//+------------------------------------------------------------------+
#property strict
//+------------------------------------------------------------------+
//| Expert initialization function                                   |
//+------------------------------------------------------------------+
string inputfoldername = "C#_output";
string outputfoldername = "C#_input";
string pairslink = "CSI_config\\CSI_symbols\\mainSymbols.txt";
datetime pairslink_ModDate = 0;
datetime waiting_ModDate = 0;
string pairs[];
datetime modDate[][6]; // paskutine data irasyta i duomenu faila
ENUM_TIMEFRAMES chart_timeframes[] = {PERIOD_M1,PERIOD_M5,PERIOD_M30,PERIOD_H4,PERIOD_D1, PERIOD_W1};
int maxLength = 2000;
datetime laikas; //reikalingas, kad outputas tikrintu tik kas minute
string emptyChar = "-";
int NumberOfFilled=-1, numberOfPending=-1;

int OnInit()
  {
//--- create timer
   EventSetMillisecondTimer(1000);
    //checkIfSymbolListChanged();
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
//--- destroy timer
   EventKillTimer();
  }
//+------------------------------------------------------------------+
//| Expert tick function                                             |
//+------------------------------------------------------------------+
void OnTick()
{
//---
   //Print((int)TimeCurrent()-(int)laikas);
   if (IsTesting())
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
   checkOrders();
   //istrinmas senas 'active' failas
   string activename = "";
   bool successfulDelete = true;
   while(FileFindFirst((outputfoldername+"\\*.active"), activename, FILE_COMMON) != INVALID_HANDLE && successfulDelete)
   {
      successfulDelete = FileDelete(outputfoldername+"\\"+activename, FILE_COMMON);
   }
   //kuriamas 'active' failas, kuris rodo kad mql'as yra aktyvus ir rodo kainas
   activename = outputfoldername+"\\metatrader.active";
   int active = FileOpen(activename, FILE_WRITE|FILE_COMMON);
   if(active != INVALID_HANDLE)
   {
      
      FileWrite(active, ArraySize(pairs));
      for(int i=0; i<ArraySize(pairs); i++)
      {
         SymbolSelect(pairs[i],true);
         FileWrite(active, (pairs[i]+" "+DoubleToString(iClose(pairs[i],PERIOD_M1,0), _Digits)));
      }
      FileClose(active);
   }
}
//+------------------------------------------------------------------+
void output(int symbolNumber, int timeNumber, int ilgis) //iraso nauja simbolio charta jei senas nesutampa
{
   //istrina senus failus
   ENUM_TIMEFRAMES taimf = chart_timeframes[timeNumber];
   string symbol = pairs[symbolNumber];
   int time = PeriodSeconds(taimf);
   bool reikiaIrasyti = false;
   int handle;
   string filename = outputfoldername+"\\"+time+"\\"+symbol+".csv";
   datetime paskutineFailoModifikavimoData=0;
   /*if(FileIsExist(filename, FILE_COMMON))
   {
      handle = FileOpen(filename, FILE_READ|FILE_COMMON);
      int ilgis = FileReadString(handle);
      string eilute = FileReadString(handle);
      for(int i=0; i<4; i++)
      {
         eilute = StringSubstr(eilute,StringFind(eilute," ")+1);
      }
      paskutineDataFaile = StringToTime(eilute);
      FileClose(handle);
   }*/
   paskutineFailoModifikavimoData = FileGetInteger(filename, FILE_MODIFY_DATE, true);
   //SymbolSelect(symbol, true);        neselectinam nes tai jau padaro rewriteOutput() funkcija
   //jei failo vidus sutampa ar failas neegzistuoja, sukuriamas naujas
   if(!FileIsExist(filename, FILE_COMMON) || iTime(symbol,taimf,0) != modDate[symbolNumber][timeNumber])//(int)iTime(symbol,taimf,0) - (int)paskutineFailoModifikavimoData >= time)
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
            string date = TimeToStr(iTime(symbol,taimf,i), TIME_DATE|TIME_MINUTES);
            StringReplace(date, ".", "-");
            string eilute = DoubleToString(iLow(symbol,taimf,i),_Digits)+","+DoubleToString(iHigh(symbol,taimf,i),_Digits)+","+DoubleToString(iOpen(symbol,taimf,i),_Digits)+","+DoubleToString(iClose(symbol,taimf,i),_Digits)+","+date;
            FileWrite(failas, eilute);
         }
         FileClose(failas);
         modDate[symbolNumber][timeNumber] = iTime(symbol,taimf,0);
      }
   }
}

bool pairslinkRead()//skaito pairslink ir atnaujina poras, jei modifikacijos data nesutampa
{
   int symbolList;
   int modificationDate;
   //skaito antra faila
   modificationDate = FileGetInteger(symbolList, FILE_MODIFY_DATE);
   if(pairslink_ModDate != (datetime)modificationDate)
   {
      symbolList = FileOpen(pairslink, FILE_READ|FILE_COMMON|FILE_TXT/*|FILE_ANSI*/);
      if(symbolList == INVALID_HANDLE)
      {
         return false;
      }
      if(pairslink_ModDate != (datetime)modificationDate)
      {
         int kiekis = (int)FileReadString(symbolList);
         ArrayResize(pairs,kiekis);
         ArrayResize(modDate,kiekis);
         for(int i=0; i<kiekis; i++)
         {
            pairs[i] = FileReadString(symbolList);
         }
         FileClose(symbolList);
      }
      FileClose(symbolList);
      //gali buti nereikalinga
      /*for(int i=0; i<ArraySize(pairs); i++)
      {
         for(int tmf=0; tmf<ArraySize(chart_timeframes); tmf++)
         {
            output(pairs[i],chart_timeframes[tmf],dataLength);
         }
      }*/
   }
   return true;
}

void rewriteOutput() //pasirupina kad visu simboliu outputas butu irasytas
{
   //netikrinu, nes reiketu isitikinti ar visi failai isirase sekmingai
   if((int)TimeCurrent() - (int)laikas > 60+10)
   {
      
   }
   for(int i=0; i<ArraySize(pairs); i++)
   {
      SymbolSelect(pairs[i], true);
      for(int tmf=0; tmf<ArraySize(chart_timeframes); tmf++)
      {
         output(i, tmf, maxLength);
      }
   }
   laikas = TimeCurrent();
   laikas -= Seconds();
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

void checkOrders()
{
   int modifikavimoData = FileGetInteger(inputfoldername+"\\waitingOrders.csv", FILE_MODIFY_DATE, true);
   if(modifikavimoData != waiting_ModDate)
   {
      int handle = FileOpen(inputfoldername+"\\waitingOrders.csv", FILE_READ|FILE_COMMON|FILE_TXT);
      if(handle != INVALID_HANDLE)
      {
         bool success=true;
         int kiekis = (int)FileReadString(handle);
         for(int i=0; i<kiekis; i++)
         {
            string eilute = FileReadString(handle);
            string stulpeliai[9];
            for(int j=0; j<8; j++)
            {
               int kablelis = StringFind(eilute, ",");
               stulpeliai[j] = StringSubstr(eilute, 0, kablelis);
               eilute = StringSubstr(eilute, kablelis+1, StringLen(eilute)-kablelis-1);
            }
            stulpeliai[8] = eilute;
            int orderioTipas;
            double takeProfit=0;
            double stopLoss=0;
            if(stulpeliai[6] != emptyChar)
            { takeProfit = (double)stulpeliai[6];}
            if(stulpeliai[7] != emptyChar)
            { stopLoss = (double)stulpeliai[7];}
            Alert(stulpeliai[0],emptyChar);
            if(stulpeliai[0] != emptyChar)
            {
               int id = (int)stulpeliai[0];
               OrderSelect(id, SELECT_BY_TICKET);
               OrderModify(id, OrderOpenPrice(), stopLoss, takeProfit, 0, Blue);
               Alert(stopLoss);
            }
            else
            {
               int tipas = 0;
               if(StringFind(stulpeliai[2], "limit") != -1)
               { tipas = 2; }
               else if(StringFind(stulpeliai[2], "stop") != -1)
               { tipas = 4;}
               if(StringFind(stulpeliai[2], "sell") != -1)
               { tipas++;}
               double volume = stulpeliai[3];
               string symbol = stulpeliai[4];
               SymbolSelect(symbol, true);
               double price = stulpeliai[5];
               double takeprofit = stulpeliai[6];
               double stoploss = stulpeliai[7];
               double slippage = _Point*50;
               int magicNumber = stulpeliai[8];
               if(OrderSend(symbol, tipas, volume, price, slippage, stoploss, takeprofit, NULL, magicNumber, 0, clrNONE) == -1)
               {success = false;}
               Alert(tipas+" "+volume+" "+symbol+" "+price+" "+slippage+" "+stoploss+" "+takeprofit);
            }
         }
         FileClose(handle);
         FileDelete(inputfoldername+"\\waitingOrders.csv", FILE_COMMON);
         waiting_ModDate = modifikavimoData;
         numberOfPending = -1;
      }
   }
   //outputas
   int pendingKiekis;
   for(int i=0; i<OrdersTotal(); i++)
   {
      OrderSelect(i, SELECT_BY_POS);
      if(OrderType() >= 2)
      {
         pendingKiekis++;
      }
   }
   if(numberOfPending != pendingKiekis || NumberOfFilled != OrdersTotal()-pendingKiekis)
   {
      int handle1 = FileOpen(outputfoldername+"\\pendingOrders.csv", FILE_WRITE|FILE_COMMON|FILE_CSV);
      int handle2 = FileOpen(outputfoldername+"\\filledOrders.csv", FILE_WRITE|FILE_COMMON|FILE_CSV);
      if(handle1 != INVALID_HANDLE && handle2 != INVALID_HANDLE)
      {
         numberOfPending = pendingKiekis;
         NumberOfFilled = OrdersTotal()-pendingKiekis;
         FileWrite(handle1, pendingKiekis);
         FileWrite(handle2, OrdersTotal()-pendingKiekis);
         for(int i=0; i<OrdersTotal(); i++)
         {
            OrderSelect(i, SELECT_BY_POS);
            int numerioTipas = OrderType();
            string tipas;
            if(numerioTipas%2 == 0)
            { tipas = "buy"; }
            else {tipas = "sell";}
            if(numerioTipas == 2 || numerioTipas == 3)
            { tipas += "limit";}
            if(numerioTipas == 4 || numerioTipas == 5)
            { tipas += "stop";}
            string date = TimeToStr(OrderOpenTime(), TIME_DATE|TIME_MINUTES);
            string takeprofit = OrderTakeProfit();
            if(takeprofit == "0")
            { takeprofit = "-";}
            string stoploss = OrderStopLoss();
            if(stoploss == "0")
            { stoploss = "-";}
            string eilute = OrderTicket()+","+date+","+tipas+","+OrderLots()+","+OrderSymbol()+","+OrderOpenPrice()+","+takeprofit+","+stoploss+","+OrderMagicNumber();
            if(numerioTipas >= 2)
            {
               FileWrite(handle1, eilute);
            }
            else
            {
               FileWrite(handle2, eilute);
            }
         }
         FileClose(handle1);
         FileClose(handle2);
      }
   }
}