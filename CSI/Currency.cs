using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace CSI
{
    public class Currency //skaiciuoja valiutu pokyti valiutu rinkinio atzvilgiu
    {
        public const string cofflink = "C:/Users/Tadas/AppData/Roaming/MetaQuotes/Terminal/Common/Files/CSI_config/CSI_symbols/coefficientsForCurrencyBasket.txt";

        FileStream fs;
        StreamWriter sw;
        StreamReader sr;
        public double[] basketCoeff; //coefficient for basket calculations
        public string[] symbol; // etc EUR, USD or GBP
        public string[] pairs; // etc EURUSD or GBPUSD
        //public string[] valiutuPoros;
        public int[] correlationCoeff; //XUSD => 1, USDX => -1 or 1, USD => 0
        public CS_chart[,] CS_charts; //pirmas narys nurodo taimfreima, antras - valiutos numeri
        private MQL_bridge metatrader;
        public double sumOfCoefficients = 0;
        static int nlength = 300;
        private int numberOfPairs = 0; //valiutu poros reikalingos skaiciavimui
        public int errorCode { get; set; } = 0;
        //public TimeSpan forexActiveFrom = new TimeSpan(0, 0, 0), forexActiveTo = new TimeSpan(12, 0, 0); //nuo iki laikai kada forex is available
        DateTime[,] generalTime;

        //public static int[] laikai = new int[] { 60, 300, 1800, 14400, 86400, 604800};
        public class CS_chart
        {
            public double[] CS_value;
            public tradingMath.Ema_indicator Ema;
            //public double standartineDeviacija = 0;
            public double[] skirtumas;
            public CS_chart()
            {
                CS_value = new double[nlength];
            }
            public void CalculateEma()
            {
                Ema = new tradingMath.Ema_indicator(50, this.CS_value);
                skirtumas = new double[CS_value.Length];
                for(int i=0; i<CS_value.Length; i++)
                {
                    skirtumas[i] = this.CS_value[i] - this.Ema.value[i];
                }
            }
            public double getPercentage()
            {
                return 100 * tradingMath.Erf(getRatio()) / 2;
            }
            public double getRatio()
            {
                double standardDeviationOfEma = tradingMath.standardDeviation(skirtumas);
                if (standardDeviationOfEma != 0)
                {
                    return ((this.CS_value[0] - this.Ema.value[0]) / standardDeviationOfEma);
                }
                else
                {
                    return (0);
                }
            }
        }

        //cia mes skaitom pairs lista ir coeficientus, kuriuos sustatome i reikiamas vietas
        public Currency(MQL_bridge platform)
        {
            metatrader = platform;
            metatrader.renewSymbolList();
            //susideliojame symbol lista
            if (metatrader.errorCode == 0)
            {
                List<string> sarasas = new List<string>();
                numberOfPairs = 0;
                for(int i=0; i<metatrader.simboliai.Count; i++)
                {
                    string simbolis = metatrader.simboliai[i];
                    if(simbolis.IndexOf("USD") == 0 || simbolis.IndexOf("USD") == 3)
                    {
                        numberOfPairs++;
                        sarasas.Add(simbolis);
                    }
                }
                basketCoeff = new double[numberOfPairs + 1];
                symbol = new string[numberOfPairs + 1];
                pairs = new string[numberOfPairs];
                correlationCoeff = new int[numberOfPairs + 1];
                CS_charts = new CS_chart[MQL_bridge.laikai.Length, numberOfPairs + 1];

                symbol[0] = "USD";
                basketCoeff[0] = 0;
                correlationCoeff[0] = 0;
                for (int i = 1; i < numberOfPairs + 1; i++)
                {
                    string eilute = sarasas[i - 1];
                    pairs[i - 1] = eilute;
                    if (eilute.IndexOf("USD") > 0)
                    {
                        correlationCoeff[i] = -1;
                        symbol[i] = eilute.Substring(0, eilute.IndexOf("USD"));
                        basketCoeff[i] = 0;
                    }
                    else
                    {
                        correlationCoeff[i] = 1;
                        symbol[i] = eilute.Substring(3);
                        basketCoeff[i] = 0;
                    }
                }
            }
            else
            {
                //forma1.eroras("Pairs.txt has not been found! I need this shit to function properly");
                errorCode = 1;
            }
            //skaitome koeficientu faila
            if (File.Exists(cofflink) && errorCode == 0)
            {
                fs = File.OpenRead(cofflink);
                sr = new StreamReader(fs);
                int kiekis = Convert.ToInt32(sr.ReadLine());
                for (int i = 0; i < kiekis; i++)
                {
                    string eilute = sr.ReadLine();
                    string simbolis = eilute.Substring(0, eilute.IndexOf(" "));
                    eilute = eilute.Remove(0, eilute.IndexOf(" ") + 1);
                    for (int j = 0; j < numberOfPairs+1; j++) //suranda tinkama valiuta kuriai duoti koeficienta
                    {
                        if (simbolis == symbol[j])
                        {
                            basketCoeff[j] = Convert.ToDouble(eilute);
                            sumOfCoefficients += basketCoeff[j];
                            break;
                        }
                    }
                }
                sr.Close();
                fs.Close();
            }
            else if(errorCode == 0)
            {
                //forma1.eroras("coefficients.txt has not been found");
                errorCode = 2;
            }
        }

        //grazina valiutu skaiciu reikalinga programai
        public int Count
        {
            get { return numberOfPairs+1; }
        }
        public int NumberOfSymbol(string simbolis)
        {
            return Array.IndexOf(symbol, simbolis);
        } // grazina simbolio numeri  pagal pavadinima

        //be salygu atnaujina CS_chart
        public void refresh()
        {
            generateTimeArray();
            for(int i=0; i < Count; i++)
            {
                for(int j=0; j< MQL_bridge.laikai.Length; j++)
                {
                    CS_charts[j, i] = new CS_chart();
                }
            }
            for (int taimas = 0; taimas < MQL_bridge.laikai.Length; taimas++)
            {
                Chart[] chartai = new Chart[Count-1];
                for(int j=0; j<Count-1; j++)
                {
                    chartai[j] = metatrader.getChart(MQL_bridge.laikai[taimas], pairs[j]);
                }
                for (int i = 0; i < nlength; i++) // i equals to number of candles from the beggining
                {
                    double[] vidurkiai = new double[Count];
                    double[] CurrChange = new double[Count];
                    for (int y = 0; y < Count; y++) // y equals to the number of currency from whitch the average is calculated
                    {
                        CurrChange[y] = 1;
                        if (y > 0)
                        {
                            double closeValue = -1000;
                            if (chartai[y - 1].time[chartai[y - 1].time.Count-1] <= generalTime[taimas, i])
                            {
                                for (int j = i; j >= 0; j--)
                                {
                                    if (chartai[y - 1].time[j] - generalTime[taimas, i] <= TimeSpan.FromSeconds(MQL_bridge.laikai[taimas]/2))
                                    {
                                        closeValue = chartai[y - 1].close[j];
                                        break;
                                    }
                                    if(j == 0)
                                    {
                                        closeValue = chartai[y - 1].close[0];
                                    }
                                }
                                CurrChange[y] = Math.Pow((closeValue / chartai[y - 1].close[nlength - 1]), -this.correlationCoeff[y]); ; // for example ((EURUSD(i)/EURUSD(nlenght))^1 for %CHF average from EUR
                            }
                        }
                    }
                    for (int x = 0; x < Count; x++) // x equals to the number of currency being calculated
                    {
                        /*
                        (XY)
                                USD     EUR     GBP     CHF       valiuta kurios CS ieskom    (0) means no need to calculate
                        USD   ( 0 0)  ( 0 1)  ( 0 2)  ( 0 3)
                        EUR   ( 1 0)  ( 1 1)  ( 1 2)  ( 1 3)
                        GBP   ( 2 0)  ( 2 1)  ( 2 2)  ( 2 3)
                        CHF   ( 3 0)  ( 3 1)  ( 3 2)  ( 3 3)
                        valiuta nuo kurios skaiciuojame
                        */
                        double first = CurrChange[x];
                        for (int y = 0; y < Count; y++) // y equals to the number of currency from whitch the average is calculated
                        {
                            if (basketCoeff[y] != 0)
                            {
                                if (x != y) //jei x==y realiai vistiek first/second gaunas lygu 1
                                {
                                    vidurkiai[x] += basketCoeff[y] * first / CurrChange[y];
                                }
                                else
                                {
                                    vidurkiai[x] += basketCoeff[y];
                                }
                            }
                        }
                        vidurkiai[x] = vidurkiai[x] / sumOfCoefficients - 1;
                        vidurkiai[x] = Math.Round(vidurkiai[x], 5);
                    }
                    //----------- si dalis redaguojama, priklausys ko reikia grafikui
                    for (int j = 0; j < Count; j++)
                    {
                        CS_charts[taimas, j].CS_value[i] += vidurkiai[j];
                    }
                }
                for (int y = 0; y < Count; y++)
                {
                    CS_charts[taimas, y].CalculateEma();
                }
            }
        }

        private void generateTimeArray()
        {
            generalTime = new DateTime[MQL_bridge.laikai.Length, nlength];
            for(int i=0; i< MQL_bridge.laikai.Length; i++)
            {
                DateTime laikas = DateTime.Now;
                if ((laikas.DayOfWeek == DayOfWeek.Saturday || laikas.DayOfWeek == DayOfWeek.Sunday) && MQL_bridge.laikai[i] <= 86400)
                {
                    /*pradinis -= TimeSpan.FromMilliseconds(pradinis.Second);
                    pradinis -= TimeSpan.FromMinutes(pradinis.Minute);
                    pradinis -= TimeSpan.FromHours(pradinis.Hour);*/
                    laikas = new DateTime(laikas.Year, laikas.Month, laikas.Day);
                    if (laikas.DayOfWeek == DayOfWeek.Sunday)
                    { laikas -= TimeSpan.FromDays(1); }
                    laikas -= TimeSpan.FromSeconds(MQL_bridge.laikai[i]);
                }
                else if(MQL_bridge.laikai[i] <= 86400)
                {
                    laikas -= TimeSpan.FromSeconds((laikas.Second + 60*laikas.Minute + 60*60*laikas.Hour)% MQL_bridge.laikai[i]);
                }
                else if(MQL_bridge.laikai[i] == 604800)
                {
                    laikas = new DateTime(laikas.Year, laikas.Month, laikas.Day);
                    laikas -= TimeSpan.FromDays(1);
                    while (laikas.DayOfWeek != DayOfWeek.Sunday)
                    {
                        laikas -= TimeSpan.FromDays(1);
                    }
                }
                generalTime[i, 0] = laikas;
                for(int j=1; j<nlength; j++)
                {
                    laikas = generalTime[i, j-1] - TimeSpan.FromSeconds(MQL_bridge.laikai[i]);
                    if (laikas.DayOfWeek == DayOfWeek.Sunday && MQL_bridge.laikai[i] <= 86400)
                    {
                        laikas -= TimeSpan.FromDays(2);
                    }
                    generalTime[i, j] = laikas;
                }
            }
        }
        #region
        /*private bool forexIsActive(DateTime pradinis)
        {
            int minutes = pradinis.Minute + 60 * pradinis.Hour;
            int from = forexActiveFrom.TotalMinutes;
            if(forexActiveFrom > forexActiveTo)
            {
                if(minutes > forexActiveFrom.TotalMinutes || minutes < fore)
            }
            return true;
        }*/        
        #endregion
    }
}
