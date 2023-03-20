using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace CSI
{
    public class MQL_bridge
    {
        const string commonlink = "C:/Users/Tadas/AppData/Roaming/MetaQuotes/Terminal/Common/Files/";
        const string outputlink = commonlink+"C#_output/";
        public const string inputlink = commonlink + "C#_input/";
        const string loglink = commonlink + "CSI_config/Log files/logs.txt";
        public const string activeName = "metatrader.active";
        public const string Allpairslink = commonlink + "CSI_config/CSI_symbols/mainSymbols.txt";
        public const string waitinglink = outputlink + "waitingOrders.csv";
        public const string pendinglink = inputlink + "pendingOrders.csv";
        public const string filledlink = inputlink + "filledOrders.csv";
        const int magicNr = 5515135;
        private Chart[,] charts;
        public static int[] laikai = new int[] { 60, 300, 1800, 14400, 86400, 604800 };
        public List<string> simboliai;
        public List<Order> filledOrders;
        public List<Order> pendingOrders;
        private DateTime lastDateOrdersRefreshed = new DateTime(1970, 1, 1);
        public int errorCode { get; set; } = 0;
        public bool active
        {
            get
            {
                if (DateTime.Now - File.GetLastWriteTime(inputlink + activeName) > new TimeSpan(0, 0, 15))
                { return false; }
                else { return true; }
            }
        }

        FileStream fs;
        StreamWriter sw;
        StreamReader sr;

        public MQL_bridge()
        {
            if (File.Exists(loglink))
            { File.Delete(loglink); }
            Directory.Delete(outputlink, true);
            Directory.CreateDirectory(outputlink);
            refreshOrders();
        }

        //inputo funkcijos
        public void renewSymbolList() //be salygu nuskaito simboliu faila ir atnaujina visus chartus
        {
            //nuskaito visus simbolius ir sudeda i Lista 'simboliai'
            #region
            errorCode = 0;
            if (File.Exists(Allpairslink))
            {
                fs = File.OpenRead(Allpairslink);
                sr = new StreamReader(fs);
                int kiekis = Int32.Parse(sr.ReadLine());
                simboliai = new List<string>();
                for (int i = 0; i < kiekis; i++)
                {
                    string eilute = sr.ReadLine();
                    if (!simboliai.Contains(eilute) || true) // reikia sutvarkyti
                    {
                        simboliai.Add(eilute);
                    }
                }
                sr.Close();
                fs.Close();
                charts = new Chart[laikai.Length, simboliai.Count];
                for (int x = 0; x < laikai.Length; x++)
                {
                    for (int y = 0; y < simboliai.Count; y++)
                    {
                        charts[x, y] = new Chart(laikai[x], simboliai[y]);
                    }
                }
            }
            else
            {
                errorCode = 1;
            }
            #endregion;
        }
        public bool refreshCharts() //naudojamas timeriui, kad atnaujinti informacija
        {
            bool successful = true;
            for (int x = 0; x < laikai.Length; x++)
            {
                for (int y = 0; y < simboliai.Count; y++)
                {
                    if(charts[x, y].checkIfRenewChart() == false)
                    {
                        successful = false;
                    }
                }
            }
            renewLastValues();
            return successful;
        }
        private void renewLastValues() //atnaujina paskutines reiksmes is 'active' failo
        {
            try
            {
                fs = File.OpenRead(inputlink + activeName);
                sr = new StreamReader(fs);
                int kiekis = Int32.Parse(sr.ReadLine());
                for (int i = 0; i < kiekis; i++)
                {
                    Chart chartas;
                    double close;

                    string eilute = sr.ReadLine();
                    string simbolis = eilute.Substring(0, eilute.IndexOf(" "));
                    eilute = eilute.Substring(eilute.IndexOf(" ") + 1);
                    close = Double.Parse(eilute);
                    //sudeda sias keturias vertes i visu taimfreimu chartus
                    for (int j = 0; j < laikai.Length; j++)
                    {
                        chartas = getChart(laikai[j], simbolis);
                        chartas.close[0] = close;
                    }
                }
                sr.Close();
                fs.Close();
            }
            catch //jei nepaeina perskaityt sudedam pries
            {

            }
        }
        public Chart getChart(int askedTimeframe, string askedSymbol)
        {
            return charts[Array.IndexOf(laikai, askedTimeframe), simboliai.IndexOf(askedSymbol)];
        }

        //output funkcijos
        public bool sendOrder(Order orderis)
        {
            fs = File.Create(waitinglink);
            if(fs != null)
            {
                orderis.time = DateTime.Now;
                filledOrders.Add(orderis);
                sw = new StreamWriter(fs);
                sw.WriteLine(filledOrders.Count);
                filledOrders.GroupBy(o => o.price);
                foreach (Order o in filledOrders)
                {
                    string tipas="";
                    if (typeof(marketOrder) == o.GetType())
                    { tipas = o.type.ToString(); }
                    else if (typeof(limitOrder) == o.GetType())
                    { tipas = o.type + "limit"; }
                    else if (typeof(stopOrder) == o.GetType())
                    { tipas = o.type + "stop"; }
                    string eilute = $"{o.time},{tipas},{o.volume},{o.simbol}";
                    if(typeof(Order) == o.GetType())
                    { eilute = $"{o.ticket},{eilute}"; }
                    else 
                    { eilute = "-," + eilute; }
                    if (typeof(marketOrder) == o.GetType())
                    { eilute += ",-"; }
                    else
                    { eilute += $",{o.price}"; }
                    if (o.takeProfit == -1)
                    { eilute += ",-"; }
                    else { eilute += $",{o.takeProfit}"; }
                    if (o.stopLoss == -1)
                    { eilute += ",-"; }
                    else 
                    { eilute += $",{o.stopLoss}"; }
                    eilute += $",{magicNr}";

                    sw.WriteLine(eilute);
                }
                sw.Close();
                fs.Close();
                return true;
            }
            return false;
        }

        public void refreshOrders() // refreshina orderius jei yra ka atnaujinti
        {
            DateTime date1 = new FileInfo(filledlink).LastWriteTime;
            DateTime date2 = new FileInfo(pendinglink).LastWriteTime;
            if (lastDateOrdersRefreshed != date1 && lastDateOrdersRefreshed != date2)
            {
                for (int x = 0; x < 2; x++)
                {
                    try
                    {
                        FileStream fs;
                        if (x == 0)
                        {
                            fs = File.OpenRead(filledlink);
                            filledOrders = new List<Order>();
                        }
                        else
                        {
                            fs = File.OpenRead(pendinglink);
                            pendingOrders = new List<Order>();
                        }
                        StreamReader sr = new StreamReader(fs);
                        int kiekis = Convert.ToInt32(sr.ReadLine());
                        for (int i = 0; i < kiekis; i++)
                        {
                            string[] stulpeliai = sr.ReadLine().Split(',');
                            Order orderis = new Order();
                            orderis.ticket = Convert.ToInt32(stulpeliai[0]);
                            orderis.time = Convert.ToDateTime(stulpeliai[1]);
                            if (stulpeliai[2].Contains("buy"))
                            { orderis.type = Order.Type.buy; }
                            else { orderis.type = Order.Type.sell; }
                            orderis.volume = Convert.ToDouble(stulpeliai[3]);
                            orderis.simbol = stulpeliai[4];
                            orderis.price = Convert.ToDouble(stulpeliai[5]);
                            if (stulpeliai[6] == "-")
                            { orderis.takeProfit = -1; }
                            else { orderis.takeProfit = Convert.ToDouble(stulpeliai[6]); }
                            if (stulpeliai[7] == "-")
                            { orderis.stopLoss = -1; }
                            else { orderis.stopLoss = Convert.ToDouble(stulpeliai[7]); }
                            orderis.magicNumber = Convert.ToInt32(stulpeliai[8]);
                            if (x == 0)
                            { filledOrders.Add(orderis); }
                            else
                            { pendingOrders.Add(orderis); }
                        }
                        sr.Close();
                        fs.Close();
                    }
                    catch (IOException e) { Debug.WriteLine(e.Message); }
                }
                lastDateOrdersRefreshed = date1;
                if(date2 > date1)
                { lastDateOrdersRefreshed = date2; }
            }
        }

    }

}
