using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
//using System.Linq;

namespace CSI
{

    public class Chart
    {
        //const string inputlink = "C:/Users/Tadas/AppData/Roaming/MetaQuotes/Terminal/Common/Files/C#_input/";

        private int dydis;
        public string symbol { get; private set; }
        public int timeframe { get; private set; }
        public List<double> low;
        public List<double> high;
        public List<double> open;
        public List<double> close;
        public List<DateTime> time;
        private DateTime lastTimeChartEdited = new DateTime(1970, 1, 1);
        public MacdDivergence macd;
        private FileStream failas;
        private StreamReader sr;
        private const int maxdydis = 2000;

        //privatus kintamiejei padaryti prieinami publicly
        public int Count
        {
            get { return dydis; }
        }
        public Chart(int askedTimeframe, string askedSymbol)
        {
            symbol = askedSymbol;
            timeframe = askedTimeframe;
            low = new List<double>();
            high = new List<double>();
            open = new List<double>();
            close = new List<double>();
            time = new List<DateTime>();
            renewChart();
        }
        public bool checkIfRenewChart() // jei sukurtas naujas order failas, perasomi duomenys su renewChart()
        {
            FileInfo info = new FileInfo($"{MQL_bridge.inputlink}{timeframe}/{symbol}.order");
            if (lastTimeChartEdited != info.LastWriteTime)
            {
                return renewChart();
            }
            return true;
        }
        private bool renewChart() // nieko nebetikrina ir atnaujina visa charta
        {
            if (File.Exists($"{MQL_bridge.inputlink}{timeframe}/{symbol}.csv"))
            {
                try //jei netycia kazkas kitas yra atidares faila
                {
                    failas = File.OpenRead($"{MQL_bridge.inputlink}{timeframe}/{symbol}.csv");
                    sr = new StreamReader(failas);
                    if (Int32.TryParse(sr.ReadLine(), out dydis) && dydis > 0)
                    {
                        string pirmaEilute = sr.ReadLine();
                        string laikinas = pirmaEilute;
                        for (int i = 0; i < 4; i++)
                        {
                            laikinas = laikinas.Substring(laikinas.IndexOf(" ") + 1);
                        }
                        DateTime senasLaikas = new DateTime(1, 1, 1);
                        if (time.Count != 0)
                        {
                            senasLaikas = time[0];
                        }
                        //temporary list'ai
                        List<double> tlow = new List<double>();
                        List<double> thigh = new List<double>();
                        List<double> topen = new List<double>();
                        List<double> tclose = new List<double>();
                        List<DateTime> ttime = new List<DateTime>();
                        if (Convert.ToDateTime(laikinas) != senasLaikas) //kitu atveju irasineti nereikia
                        {
                            tlow.Add(Convert.ToDouble(pirmaEilute.Substring(0, pirmaEilute.IndexOf(","))));
                            pirmaEilute = pirmaEilute.Remove(0, pirmaEilute.IndexOf(",") + 1);
                            thigh.Add(Convert.ToDouble(pirmaEilute.Substring(0, pirmaEilute.IndexOf(","))));
                            pirmaEilute = pirmaEilute.Remove(0, pirmaEilute.IndexOf(",") + 1);
                            topen.Add(Convert.ToDouble(pirmaEilute.Substring(0, pirmaEilute.IndexOf(","))));
                            pirmaEilute = pirmaEilute.Remove(0, pirmaEilute.IndexOf(",") + 1);
                            tclose.Add(Convert.ToDouble(pirmaEilute.Substring(0, pirmaEilute.IndexOf(","))));
                            pirmaEilute = pirmaEilute.Remove(0, pirmaEilute.IndexOf(",") + 1);
                            ttime.Add(Convert.ToDateTime(pirmaEilute));
                            for (int j = 1; j < dydis; j++)
                            {
                                string eilute = sr.ReadLine();
                                double tempLow = Convert.ToDouble(eilute.Substring(0, eilute.IndexOf(",")));
                                eilute = eilute.Remove(0, eilute.IndexOf(",") + 1);
                                double tempHigh = Convert.ToDouble(eilute.Substring(0, eilute.IndexOf(",")));
                                eilute = eilute.Remove(0, eilute.IndexOf(",") + 1);
                                double tempOpen = Convert.ToDouble(eilute.Substring(0, eilute.IndexOf(",")));
                                eilute = eilute.Remove(0, eilute.IndexOf(",") + 1);
                                double tempClose = Convert.ToDouble(eilute.Substring(0, eilute.IndexOf(",")));
                                eilute = eilute.Remove(0, eilute.IndexOf(",") + 1);
                                DateTime tempTime = Convert.ToDateTime(eilute);
                                if (senasLaikas == tempTime)
                                {
                                    low[0] = tempLow;
                                    high[0] = tempHigh;
                                    open[0] = tempOpen;
                                    close[0] = tempClose;
                                    break;
                                }
                                tlow.Add(tempLow);
                                thigh.Add(tempHigh);
                                topen.Add(tempOpen);
                                tclose.Add(tempClose);
                                ttime.Add(tempTime);
                            }
                            if (ttime.Count == dydis || time.Count == 0)
                            {
                                low = new List<double>(tlow);
                                high = new List<double>(thigh);
                                open = new List<double>(topen);
                                close = new List<double>(tclose);
                                time = new List<DateTime>(ttime);
                                
                            }
                            else
                            {
                                low.InsertRange(0, tlow);
                                high.InsertRange(0, thigh);
                                open.InsertRange(0, topen);
                                close.InsertRange(0, tclose);
                                time.InsertRange(0, ttime);
                                if (time.Count > maxdydis)
                                {
                                    low.RemoveRange(maxdydis, low.Count - maxdydis);
                                    high.RemoveRange(maxdydis, high.Count - maxdydis);
                                    open.RemoveRange(maxdydis, open.Count - maxdydis);
                                    close.RemoveRange(maxdydis, close.Count - maxdydis);
                                    time.RemoveRange(maxdydis, time.Count - maxdydis);
                                }
                            }
                        }
                        FileInfo info = new FileInfo($"{MQL_bridge.inputlink}{timeframe}/{symbol}.order");
                        lastTimeChartEdited = info.LastWriteTime;
                    }
                    sr.Close();
                    failas.Close();
                }
                catch(IOException e) 
                {
                    Debug.WriteLine($"{symbol} nepavyko irasyt duomenu");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"{symbol} nepavyko irasyt duomenu");
                return false;
            }
            return true;
        }

        //indikatoriai
        #region
        public class MacdDivergence
        {
            private tradingMath.Ema_indicator Ema1;
            private tradingMath.Ema_indicator Ema2;
            private Chart chart;
            private static double Akoeficientas = 0.8; //kokia dali nuo extremumo turi nukristi verte, kad extremumas uzsiskaitytu
            private static double Bkoeficientas = 2; //maksimali verte kuria gali pasiekti antra macd divergence banga
            public struct taskai
            {
                public int candleNumber;
                public double value;
            }
            private List<taskai> virsunes;
            private List<taskai> apacios;
            private double[] macd;
            public int VcandleAgoFormed = -1;
            public int AcandleAgoFormed = -1;
            public MacdDivergence(Chart chartas)
            {
                chart = chartas;
                Ema1 = new tradingMath.Ema_indicator(12, chart.close.ToArray());
                Ema2 = new tradingMath.Ema_indicator(26, chart.close.ToArray());
                virsunes = new List<taskai>();
                apacios = new List<taskai>();
                int PaskAukscTaskas = 0;
                int PaskZemTaskas = 0;
                int ilgis = 200;
                if(ilgis > chartas.Count)
                {
                    ilgis = chartas.Count;
                }
                macd = new double[ilgis];
                for (int i = 0; i < ilgis; i++)
                {
                    macd[i] = Ema1.value[i] - Ema2.value[i];
                }
                for (int i = 0; i < ilgis; i++)
                {
                    macd[i] = Ema1.value[i] - Ema2.value[i];
                    double aukscSkirtumas = Ema1.value[Math.Abs(PaskAukscTaskas)] - Ema2.value[Math.Abs(PaskAukscTaskas)];
                    double zemSkirtumas = Ema1.value[Math.Abs(PaskZemTaskas)] - Ema2.value[Math.Abs(PaskZemTaskas)];
                    //ieskomos virsunes
                    #region
                    double didesnis = Math.Abs(macd[i]);
                    if (Math.Abs(aukscSkirtumas) > didesnis)
                    { didesnis = Math.Abs(aukscSkirtumas); }

                    if(i == 0)
                    {
                        taskai taskas;
                        taskas.candleNumber = PaskAukscTaskas;
                        taskas.value = aukscSkirtumas;
                        virsunes.Add(taskas);
                        PaskAukscTaskas = -1;
                    }
                    else if(PaskAukscTaskas > 0) // jei vis dar kylam iki auksciausio vertes ir nenukritom zemiau nei per koeficienta
                    {
                        if (macd[i] > aukscSkirtumas)
                        {
                            PaskAukscTaskas = i;
                        }
                        else if ((aukscSkirtumas - macd[i]) / didesnis > 1 - Akoeficientas)
                        {
                            taskai taskas;
                            taskas.candleNumber = PaskAukscTaskas;
                            taskas.value = aukscSkirtumas;
                            virsunes.Add(taskas);
                            PaskAukscTaskas = -i;
                            if (virsunes.Count == 2)
                            {
                                for (int j = taskas.candleNumber; j >= 0; j--)
                                {
                                    didesnis = Math.Abs(taskas.value);
                                    if ((aukscSkirtumas - macd[i]) / didesnis > 1 - Akoeficientas)
                                    { 
                                        VcandleAgoFormed = j;
                                        break; 
                                    }
                                }
                            }
                        }
                    }
                    else // jei leidziames nuo auksciausios vertes 'i' candle numeris yra neigiamas
                    {
                        if (macd[i] < aukscSkirtumas)
                        {
                            PaskAukscTaskas = -i;
                        }
                        else if ((macd[i] - aukscSkirtumas) / didesnis > 1 - Akoeficientas) // cia reikia piesti schema, taip lengvai nesuprasi
                        {
                            PaskAukscTaskas = i;
                        }
                    }
                    #endregion
                    //ieskomos apacios
                    #region
                    didesnis = Math.Abs(macd[i]);
                    if (Math.Abs(zemSkirtumas) > didesnis)
                    { didesnis = Math.Abs(zemSkirtumas); }

                    if (i == 0)
                    {
                        taskai taskas;
                        taskas.candleNumber = PaskZemTaskas;
                        taskas.value = zemSkirtumas;
                        apacios.Add(taskas);
                        PaskZemTaskas = -1;
                    }
                    else if (PaskZemTaskas >= 0) // vertes yra neigiamos
                    {
                        if (macd[i] < zemSkirtumas)
                        {
                            PaskZemTaskas = i;
                        }
                        else if ((macd[i] - zemSkirtumas) / didesnis > 1 - Akoeficientas)
                        {
                            taskai taskas;
                            taskas.candleNumber = PaskZemTaskas;
                            taskas.value = zemSkirtumas;
                            apacios.Add(taskas);
                            PaskZemTaskas = -i;
                            if (apacios.Count == 2)
                            {
                                //Debug.WriteLine("::;:"+ zemSkirtumas);
                                for (int j = taskas.candleNumber; j >= 0; j--)
                                {
                                    didesnis = Math.Abs(zemSkirtumas);
                                    if ((macd[i] - zemSkirtumas) / didesnis > 1 - Akoeficientas)
                                    { AcandleAgoFormed = j; break; }
                                }
                            }
                        }
                    }
                    else // jei kylam nuo zemiausios vertes 'i' candle numeris yra neigiamas
                    {
                        if (macd[i] > zemSkirtumas)
                        {
                            PaskZemTaskas = -i;
                        }
                        else if ((zemSkirtumas - macd[i]) / didesnis > 1 - Akoeficientas) // cia reikia piesti schema, taip lengvai nesuprasi
                        {
                            PaskZemTaskas = i;
                        }
                    }
                    #endregion
                }
            }

            public int DoubleDivergence()
            {
                double tarpTaskas1 = chart.low[chart.lowestCandle(virsunes[2].candleNumber, virsunes[1].candleNumber)];
                double tarpTaskas2 = chart.high[chart.highestCandle(apacios[2].candleNumber, apacios[1].candleNumber)];
                if (virsunes[0].value < virsunes[1].value && virsunes[1].value < virsunes[2].value && chart.close[0] > tarpTaskas1) //bearish
                {
                    if (chart.high[virsunes[1].candleNumber] > chart.high[virsunes[2].candleNumber] && Bkoeficientas * (chart.high[apacios[2].candleNumber] - tarpTaskas1) > (chart.high[apacios[1].candleNumber] - tarpTaskas1))
                    {
                        return 1;
                    }
                }
                else if (apacios[0].value > apacios[1].value && apacios[1].value > apacios[2].value && chart.close[0] < tarpTaskas2) //bullish
                {
                    if (chart.low[apacios[1].candleNumber] < chart.low[apacios[2].candleNumber] && Bkoeficientas * (tarpTaskas2 - chart.low[apacios[2].candleNumber]) > (tarpTaskas2 - chart.low[apacios[1].candleNumber]))
                    {
                        return -1;
                    }
                }
                return 0;
            }
        }
        private int highestCandle(int to, int from=0) // find highest candle
        {
            double highest = this.high[from];
            int candle = from;
            for(int i=from; i<to; i++)
            {
                if(highest < this.high[i])
                {
                    highest = this.high[i];
                    candle = i;
                }
            }
            return candle;
        }
        private int lowestCandle(int to, int from = 0) // find highest candle
        {
            double lowest = this.low[from];
            int candle = from;
            for (int i = from; i <= to; i++)
            {
                if (lowest > this.low[i])
                {
                    lowest = this.low[i];
                    candle = i;
                }
            }
            return candle;
        }
        #endregion
    }
}
