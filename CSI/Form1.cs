using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace CSI
{
    public partial class Form1 : Form
    {
        FileStream fs;
        StreamWriter sw;
        StreamReader sr;
        const string generallink = "generalsettings.txt";
        const string loglink = "C:/Users/Tadas/AppData/Roaming/MetaQuotes/Terminal/Common/Files/CSI_config/Log files/logs.txt";

        public MQL_bridge metatrader;
        public Currency currencyList;
        Timer timeris1, timeris2;
        // dizaino kintamieji
        int[,] langeliai; //0 jei tuscia, (simbolio numeris)+1 jei uzimta
        List<Panel> paneles = new List<Panel>();
        Panel selectedpanel; // lentele, kuria vartotojas nesioja
        const int pwidth = 300; // lenteliu plotis
        const int pheight = 100; // paneliu aukstis
        const int mheight = 25; // lenteliu titlebar aukstis
        FormWindowState wndst;
        int scrwidth=0, scrheight=0;
        ComboBox combolentelem;
        Button addsymbol, openSettings;
        Panel meniuDesinesVirsuje, desine, apacia, grafikui, gmeniu, gresize;
        const int gmheight = 30; // grafiko meniu aukstis
        bool grafikasIsVisible;
        int grsy;
        const int dwidth = 300; // desines puses plotis
        const int lheight = 50; // simboliu pasirinkimo lenteles aukstis
        DataGridView divlenta;
        System.Windows.Forms.DataVisualization.Charting.Chart grafikas;
        int gheight = 400; // grafiko aukstis
        const int apheight = 50; // erroru lenteles aukstis
        int yselected = -1; // kursoriaus lokacija plonos linijos atzvilgiu
        Label alerttext;
        public static Font fontas1 = new Font("Arial", 13, FontStyle.Bold);
        int grafikoTaimfreimas = 0, grafikoIlgis = 200;
        List<int> grafikoValiutos = new List<int>(); //grafike rodomu valiutu numeriu sarasas
        bool show_probability = false;
        double alertLimit = 1;
        Color[] spalvos = new Color[] { Color.Red, Color.DarkOrange, Color.Yellow, Color.Green, Color.BlueViolet, Color.Chocolate, Color.Cyan, Color.White };
        public static Color tamsiaiMelyna = Color.FromArgb(255, 31, 45, 62);
        public static Color sviesiaiMelyna = Color.FromArgb(255, 42, 60, 83);
        Form forma2;
        public enum Taimai
        {
            M1 = 60,
            M5 = 300,
            M15 = 900,
            M30 = 1800,
            H1 = 3600,
            H4 = 14400,
            D1 = 86400,
            W1 = 604800
        }
        public struct macdRow
        {
            public string simbol;
            public Taimai taimfreimas;
            public int minutes;
            public bool bullish;
        }
        public Form1()
        {
            InitializeComponent();
            // perskaitom generalsettings.txt, gaunam programos parametrus
            if (File.Exists(generallink))
            {
                fs = new FileStream(generallink, FileMode.Open);
                sr = new StreamReader(fs);
                sr.ReadLine();
                wndst = (FormWindowState)Convert.ToInt32(sr.ReadLine());
                scrwidth = Convert.ToInt32(sr.ReadLine());
                scrheight = Convert.ToInt32(sr.ReadLine());
                gheight = Convert.ToInt32(sr.ReadLine());
                grafikasIsVisible = bool.Parse(sr.ReadLine());
                int paneliuPlotis, paneliuAukstis;
                string eilute = sr.ReadLine();
                paneliuPlotis = Int32.Parse(eilute.Substring(0, eilute.IndexOf(" ")));
                paneliuAukstis = Int32.Parse(eilute.Substring(eilute.IndexOf(" ") + 1));
                langeliai = new int[paneliuPlotis, paneliuAukstis];
                for (int y = 0; y < paneliuAukstis; y++)
                {
                    eilute = sr.ReadLine();
                    for (int x=0; x<paneliuPlotis; x++)
                    {
                        int verte = Int32.Parse(eilute.Remove(eilute.IndexOf(";")));
                        if (verte != 0)
                        {
                            langeliai[x, y] = verte;
                        }
                        eilute = eilute.Substring(eilute.IndexOf(";") + 1);
                    }
                }
                fs.Close();
                sr.Close();
                this.WindowState = wndst;
                if (wndst == FormWindowState.Normal)
                {
                    this.Size = new Size(scrwidth, scrheight);
                }
            }
            else
            {
                this.Size = new Size(2 * pwidth + dwidth + (this.Width - this.ClientRectangle.Width), 2 * pheight + grafikui.Height + (this.Height - this.ClientRectangle.Height));
            }
            //dizainas
            #region 
            this.BackColor = tamsiaiMelyna;
            this.MinimumSize = new Size(2*pwidth + dwidth + (this.Width-this.ClientRectangle.Width), 0);
            this.MinimumSize = new Size(2 * pwidth + dwidth + (this.Width - this.ClientRectangle.Width), 2 * pheight + gmheight + (this.Height - this.ClientRectangle.Height));

            //desine programos puse
            #region
            desine = new Panel();
            desine.Width = dwidth;
            desine.BackColor = Color.White;
            desine.Location = new Point(this.ClientRectangle.Width - dwidth, 0);
            desine.Height = this.ClientRectangle.Height;

            combolentelem = new ComboBox();
            combolentelem.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            combolentelem.AutoCompleteSource = AutoCompleteSource.ListItems;
            combolentelem.TextUpdate += new EventHandler(combobox_TextChanged);
            combolentelem.Location = new Point(100, 12);
            combolentelem.Width = 80;
            combolentelem.Font = fontas1;
            //combolentelem.BackColor = Color.FromArgb(255, 63, 75, 100);
            Label l = new Label();
            l.Text = "Symbol:";
            l.Location = new Point(10, 14);
            l.ForeColor = Color.White;
            l.Font = fontas1;
            addsymbol = new Button();
            addsymbol.Location = new Point(200, 5);
            addsymbol.MouseClick += new MouseEventHandler(addLentele_ButtonClick);
            addsymbol.Text = "+";
            addsymbol.BackColor = Color.White;
            addsymbol.Font = fontas1;
            addsymbol.Width = 40;
            addsymbol.Height = 40;
            openSettings = new Button();
            openSettings.Location = new Point(250, 5);
            openSettings.MouseClick += (s, e) => { openSettings_Click(); };
            openSettings.Text = "S";
            openSettings.BackColor = Color.White;
            openSettings.Font = fontas1;
            openSettings.Width = 40;
            openSettings.Height = 40;
            meniuDesinesVirsuje = new Panel();
            meniuDesinesVirsuje.Width = dwidth;
            meniuDesinesVirsuje.Height = lheight;
            meniuDesinesVirsuje.BackColor = Color.FromArgb(255, 16, 23, 32);
            meniuDesinesVirsuje.Location = new Point(0, 0);

            divlenta = new DataGridView();
            divlenta.Location = new Point(0, lheight);
            divlenta.Height = this.ClientRectangle.Height - lheight - apheight;
            divlenta.Width = dwidth;
            divlenta.Height = this.ClientRectangle.Height - lheight - apheight;
            divlenta.Columns.Add("Pair", "Pair");
            divlenta.Columns.Add("Timeframe", "Timeframe");
            divlenta.Columns.Add("Time", "Time");
            for(int i=0; i<3; i++)
            {
                divlenta.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            divlenta.RowHeadersVisible = false;
            divlenta.ScrollBars = ScrollBars.Vertical;
            divlenta.AllowUserToResizeColumns = false;
            divlenta.AllowUserToResizeRows = false;
            divlenta.AllowUserToOrderColumns = false;
            divlenta.AllowDrop = false;
            divlenta.SelectionChanged += new EventHandler(divlentaSlelected);
            //divlenta.Columns[0].

            apacia = new Panel();
            apacia.Location = new Point(0, desine.Height - apheight);
            apacia.Width = dwidth;
            apacia.Height = apheight;
            apacia.Anchor = AnchorStyles.Bottom;
            alerttext = new Label();
            alerttext.Font = fontas1;
            alerttext.Dock = DockStyle.Fill;
            alerttext.BackColor = Color.Gray;
            alerttext.ForeColor = Color.White;
            alerttext.Click += (s, e) => { openSettings_Click(); };
            apacia.Controls.Add(alerttext);

            meniuDesinesVirsuje.Controls.Add(combolentelem);
            meniuDesinesVirsuje.Controls.Add(l);
            meniuDesinesVirsuje.Controls.Add(addsymbol);
            meniuDesinesVirsuje.Controls.Add(openSettings);
            desine.Controls.Add(meniuDesinesVirsuje);
            desine.Controls.Add(divlenta);
            desine.Controls.Add(apacia);
            #endregion

            // apatinio grafiko organizavimas
            #region
            grafikui = new Panel();
            grafikui.Location = new Point(0, this.ClientRectangle.Height - gheight + gmheight);
            grsy = grafikui.Location.Y;
            grafikui.Width = this.ClientRectangle.Width - dwidth;
            grafikui.Height = gheight;
            grafikui.Anchor = (AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right);
            grafikui.BackColor = Color.White;
            gmeniu = new Panel();
            gmeniu.Height = gmheight;
            gmeniu.Width = this.ClientRectangle.Width - dwidth;
            gmeniu.Location = new Point(0, 0);
            gmeniu.BackColor = Color.DarkGray;
            gmeniu.Anchor = (AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right);
            gmeniu.MouseDoubleClick += new MouseEventHandler(gmeniu_doubleclick);
            gresize = new Panel(); //labai siaura panele grafiko meniu virsuje, patamsuoja slenkant
            gresize.Location = new Point(0, 0);
            gresize.Width = this.ClientRectangle.Width - dwidth;
            gresize.BackColor = Color.Transparent;
            gresize.Height = 8;
            gresize.Anchor = (AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right);
            gresize.MouseMove += new MouseEventHandler(gresize_MouseMove);
            gresize.MouseDown += new MouseEventHandler(gresize_MouseDown);
            gresize.MouseUp += new MouseEventHandler(gresize_MouseUp);
            gresize.MouseDoubleClick += new MouseEventHandler(gmeniu_doubleclick);
            grafikas = new System.Windows.Forms.DataVisualization.Charting.Chart();
            grafikas.Location = new Point(0, gmheight);
            //grafikas.Dock = DockStyle.Fill;
            grafikas.BackColor = Color.White;
            grafikas.Name = "grafikas";
            ChartArea plotas = new ChartArea();
            grafikas.ChartAreas.Add(plotas);
            gmeniu.Controls.Add(gresize);
            gresize.BringToFront();
            grafikui.Controls.Add(gmeniu);
            grafikui.Controls.Add(grafikas);
            #endregion

            this.Controls.Add(grafikui);
            this.Controls.Add(desine);
            #endregion
            //komunikacijos iniciavimas ir grafiku, dilentos paruosimas
            #region
            metatrader = new MQL_bridge();
            currencyList = new Currency(metatrader);
            for(int i=0; i<2; i++)
            {
                Series ser = new Series();
                ser.ChartArea = grafikas.ChartAreas[0].Name;
                ser.Name = "chart"+i;
                ser.ChartType = SeriesChartType.Line;
                grafikas.Series.Add(ser);
            }
            grafikas.Series[0].Color = Color.Red;
            grafikas.Series[1].Color = Color.DarkOrange;
            /*grafikas.Series[2].Color = Color.Yellow;
            grafikas.Series[3].Color = Color.Green;
            grafikas.Series[4].Color = Color.BlueViolet;
            grafikas.Series[5].Color = Color.Chocolate;
            grafikas.Series[6].Color = Color.Cyan;
            grafikas.Series[7].Color = Color.White;*/

            timeris1 = new Timer();
            timeris1.Interval = 1000;
            timeris1.Enabled = true;
            timeris2 = new Timer();
            timeris2.Interval = 7000;
            if (currencyList.errorCode == 0)
            {
                Form1_setup();
                timeris1.Tick += (s, e) => { refresh_CSI(); };
                timeris2.Tick += (s, e) => { refresh_divelenta(); };
            }
            else
            {
                this.Enabled = false;
                forma2 = new Form2(this);
                forma2.Show();
                timeris1.Tick += checkForMissingFiles;
                timeris1.Interval = 3000;
            }
            timeris1.Start();
            timeris2.Start();
            paneliuNormalizavimas();
            #endregion
        }
        private void panel2_MouseDown(object sender, MouseEventArgs e) // skirtas lenteles paspaudimui
        {
            if(sender.GetType() == typeof(Panel))
            {
                Panel p = sender as Panel; // current panel
                selectedpanel = p.Parent as Panel;
            }
            if(sender.GetType() == typeof(Label))
            {
                Label l = sender as Label; // current panel
                selectedpanel = l.Parent.Parent as Panel;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e) // skirta judinti lenteles
        {
            if(e.Button == MouseButtons.Left && selectedpanel != null)
            {
                int x = (System.Windows.Forms.Cursor.Position.X-this.Location.X) /pwidth;
                x = (x <= langeliai.GetLength(0)-1) ? x : langeliai.GetLength(0)-1;
                int y = (System.Windows.Forms.Cursor.Position.Y - this.Location.Y - SystemInformation.CaptionHeight) / pheight;
                y = (y <= langeliai.GetLength(1)-1) ? y : langeliai.GetLength(1)-1;
                selectedpanel.Location = new Point(x*pwidth,y*pheight);
                selectedpanel.BringToFront();
            }
        }
        private void panelesKryziukas_MouseClick(object sender, MouseEventArgs e) 
        {
            Button btn = sender as Button;
            combolentelem.Items.Add(btn.Parent.Parent.Name);
            int numeris = currencyList.NumberOfSymbol(btn.Parent.Parent.Name)+1;
            for (int x = 0; x < langeliai.GetLength(0); x++)
            {
                for (int y = 0; y < langeliai.GetLength(1); y++)
                {
                    if (langeliai[x, y] == numeris)
                    {
                        langeliai[x, y] = 0;
                    }
                }
            }
            paneles.Remove(btn.Parent.Parent as Panel);
            btn.Parent.Parent.Dispose();
            uzpildytitusciuslangelius();
            SaveProgramSettings();
        } // lenteles kryziuko paspaudimas
        
        private void addLentele_ButtonClick(object sender, EventArgs e)// paspaudus Add mygtuka prideda lentele
        {
            if (currencyList.errorCode == 0)
            {
                if (currencyList.NumberOfSymbol(combolentelem.Text) != -1)
                {
                    pridetilentele(combolentelem.Text);
                }
                refresh_CSI();
                SaveProgramSettings();
            }
        }
        private void pridetilentele(string simbol, int x=-1, int y=-1) // prideda paprasyta lentele
        {
            Panel naujalentele = new Panel();
            Panel lentelesmenu = new Panel();
            Button closebutton = new Button();
            Label pavadinimas = new Label();
            Label[] taimfreimai = new Label[6];
            Label[] reiksmes = new Label[6];

            naujalentele.Size = new Size(pwidth, pheight);
            naujalentele.BackColor = sviesiaiMelyna;
            naujalentele.Location = new Point(0, 0);
            naujalentele.BorderStyle = BorderStyle.FixedSingle;
            naujalentele.Name = simbol;
            naujalentele.MouseMove += new MouseEventHandler(Form1_MouseMove);
            naujalentele.MouseUp += new MouseEventHandler(Form1_MouseUp);
            combolentelem.Items.Remove(simbol);

            lentelesmenu.Size = new Size(pwidth, mheight);
            lentelesmenu.BackColor = tamsiaiMelyna;
            lentelesmenu.MouseDown += new MouseEventHandler(panel2_MouseDown);
            lentelesmenu.MouseMove += new MouseEventHandler(Form1_MouseMove);
            lentelesmenu.MouseUp += new MouseEventHandler(Form1_MouseUp);
            naujalentele.Controls.Add(lentelesmenu);

            closebutton.Size = new Size(mheight, mheight);
            closebutton.BackgroundImage = Image.FromFile("C:/Users/Tadas/source/repos/CSI/inactiveCross.png");
            closebutton.Location = new Point(pwidth - mheight, 0);
            closebutton.BackColor = Color.Red;
            closebutton.ForeColor = Color.White;
            closebutton.MouseClick += new MouseEventHandler(panelesKryziukas_MouseClick);
            closebutton.MouseMove += new MouseEventHandler(Form1_MouseMove);
            closebutton.MouseUp += new MouseEventHandler(Form1_MouseUp);
            lentelesmenu.Controls.Add(closebutton);

            pavadinimas.Text = naujalentele.Name;
            pavadinimas.ForeColor = Color.White;
            pavadinimas.Font = fontas1;
            pavadinimas.Location = new Point(0, 2);
            pavadinimas.MouseMove += new MouseEventHandler(Form1_MouseMove);
            pavadinimas.MouseDown += new MouseEventHandler(panel2_MouseDown);
            pavadinimas.MouseUp += new MouseEventHandler(Form1_MouseUp);
            lentelesmenu.Controls.Add(pavadinimas);

            int pixelsFromLeft1 = 5, pirmaEile = 35, antraEile = 70, pixelsFromLeft2 = 25, percentAdjust = 3;
            for (int i = 0; i < 6; i++)
            {
                taimfreimai[i] = new Label();
                naujalentele.Controls.Add(taimfreimai[i]);
                taimfreimai[i].ForeColor = Color.White;
                taimfreimai[i].Font = fontas1;
                taimfreimai[i].Width = 45;
                //taimfreimai[i].BackColor = Color.Blue;
                taimfreimai[i].DoubleClick += new EventHandler(pavaizduotNaujaGrafika_doublePanelClick);
                taimfreimai[i].MouseClick += new MouseEventHandler(pridetiGrafika_panelRightClick);
                taimfreimai[i].Name = i.ToString(); //pvz.: 0, 1, ... 5
            }
            taimfreimai[4].Width = 35;
            taimfreimai[5].Width = 35;
            for (int i = 0; i < 6; i++)
            {
                reiksmes[i] = new Label();
                naujalentele.Controls.Add(reiksmes[i]);
                reiksmes[i].ForeColor = Color.Red;
                reiksmes[i].Font = new Font("Arial", 10, FontStyle.Bold);
                reiksmes[i].Text = "1.567%";
                pixelsFromLeft2 = pixelsFromLeft1 + taimfreimai[0].Width;
                reiksmes[i].Width = pwidth / 3 - taimfreimai[i].Width;
                //reiksmes[i].BackColor = Color.White;
                reiksmes[i].DoubleClick += new EventHandler(pavaizduotNaujaGrafika_doublePanelClick);
                reiksmes[i].MouseClick += new MouseEventHandler(pridetiGrafika_panelRightClick);
                reiksmes[i].Name = i.ToString()+ i.ToString(); //pvz.: 00, 11, ... 55
            }
            taimfreimai[0].Text = "M1";
            taimfreimai[1].Text = "M5";
            taimfreimai[2].Text = "M30";
            taimfreimai[3].Text = "H4";
            taimfreimai[4].Text = "D1";
            taimfreimai[5].Text = "W1";
            taimfreimai[0].Location = new Point(pixelsFromLeft1, pirmaEile);
            taimfreimai[1].Location = new Point(pixelsFromLeft1, antraEile);
            taimfreimai[2].Location = new Point(pixelsFromLeft1 + pwidth/3, pirmaEile);
            taimfreimai[3].Location = new Point(pixelsFromLeft1 + pwidth / 3, antraEile);
            taimfreimai[4].Location = new Point(pixelsFromLeft1 + 2 * pwidth / 3, pirmaEile);
            taimfreimai[5].Location = new Point(pixelsFromLeft1 + 2 * pwidth / 3, antraEile);
            reiksmes[0].Location = new Point(pixelsFromLeft2, pirmaEile + percentAdjust);
            reiksmes[1].Location = new Point(pixelsFromLeft2, antraEile + percentAdjust);
            reiksmes[2].Location = new Point(pixelsFromLeft2 + pwidth / 3, pirmaEile + percentAdjust);
            reiksmes[3].Location = new Point(pixelsFromLeft2 + pwidth / 3, antraEile + percentAdjust);
            reiksmes[4].Location = new Point(pixelsFromLeft2 + 2 * pwidth / 3 -10, pirmaEile + percentAdjust);
            reiksmes[5].Location = new Point(pixelsFromLeft2 + 2 * pwidth / 3 -10, antraEile + percentAdjust);

            if (x == -1 && y == -1) // jei lenteles lokacija yra nenurodyta
            {
                if (rastiLaisvaVieta(ref x, ref y))
                {
                    naujalentele.Location = new Point(pwidth * x, pheight * y);
                    langeliai[x, y] = currencyList.NumberOfSymbol(naujalentele.Name) + 1;
                }
                else
                {
                    naujalentele.Location = new Point(0, -pheight);
                }
            }
            else
            {
                naujalentele.Location = new Point(pwidth * x, pheight * y);
                langeliai[x, y] = currencyList.NumberOfSymbol(naujalentele.Name) + 1;
            }
            this.Controls.Add(naujalentele);
            paneles.Add(naujalentele);
        }

        private void openSettings_Click()
        {
            forma2 = new Form2(this);
            forma2.Show();
            this.Enabled = false;
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            if(grafikui != null && this.WindowState != FormWindowState.Minimized)
            {
                paneliuNormalizavimas();
            }
            if (grafikui != null && this.WindowState != FormWindowState.Minimized) //langeliai != null reikalingas kad nebutu vykdoma kol langeliai neinicijuoti
            {
                if ((this.ClientRectangle.Width - dwidth) / pwidth != langeliai.GetLength(0) || (this.ClientRectangle.Height - grafikui.Height) / pheight != langeliai.GetLength(1))
                {
                    pakeistilangeliumasyva((this.ClientRectangle.Width - dwidth) / pwidth, (this.ClientRectangle.Height - grafikui.Height) / pheight);
                }
            }
            if (desine != null) // nenorime keisti dydziu tik paleidus programa
            {
                divlenta.Height = this.ClientRectangle.Height - lheight - apheight;
                if (wndst != this.WindowState) // resizeEnd neissaukia Resize event'o
                {
                    wndst = this.WindowState;
                    SaveProgramSettings();
                }
            }
        }
        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            SaveProgramSettings();
        } // iraso i faila svarbius dydzius
        private void combobox_TextChanged(object sender, EventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            string upper = cmb.Text.ToUpper();
            string final = "";
            char[] eilute = new char[upper.Length];
            Array.Copy(upper.ToCharArray(),eilute, upper.Length);
            for (int i= 0; (i< upper.Length && final.Length < 3); i++)
            {
                if(eilute[i] >= 65 && eilute[i] <= 90)
                {
                    final += eilute[i];
                }
            }
            if(cmb.Text != final)
            {
                cmb.Text = final;
                cmb.SelectionStart = final.Length;
            }
        } // simboliu paieska
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if(selectedpanel != null)
            {
                int x = selectedpanel.Location.X/pwidth;
                int y = selectedpanel.Location.Y/pheight;
                langeliai[x, y] = currencyList.NumberOfSymbol(selectedpanel.Name)+1;
                for (int b = 0; b < langeliai.GetLength(1); b++)
                {
                    for (int a = 0; a < langeliai.GetLength(0); a++)
                    {
                        int nr = langeliai[a, b] - 1;
                        if (nr >= 0)
                        {
                            if (currencyList.symbol[nr] == selectedpanel.Name && !(a == x && b == y))
                            {
                                langeliai[a, b] = 0;
                            }
                        }
                    }
                }
                foreach (Panel panele in paneles)
                {
                    if (panele != selectedpanel && panele.Location.X/pwidth == x && panele.Location.Y/pheight == y)
                    {
                        if (rastiLaisvaVieta(ref x,ref y))
                        {
                            panele.Location = new Point(pwidth*x, pheight*y);
                            langeliai[x, y] = currencyList.NumberOfSymbol(panele.Name) + 1;
                        }
                        else 
                        { panele.Location = new Point(0, -pheight); }
                        break;
                    }
                }
                SaveProgramSettings();
            }
            selectedpanel = null;
        } // lenteliu nesiojimui
        private void gresize_MouseMove(object sender, MouseEventArgs e)
        {
            Panel linija = sender as Panel;
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeNS; //kad uzvedus pasikeistu kursorius
            if (e.Button == MouseButtons.Left && yselected != -1)
            {
                if (this.PointToClient(System.Windows.Forms.Cursor.Position).Y - yselected >= 0 && gmheight <= this.ClientRectangle.Height - this.PointToClient(System.Windows.Forms.Cursor.Position).Y + yselected)
                {
                    grafikui.Location = new Point(0, this.PointToClient(System.Windows.Forms.Cursor.Position).Y - yselected);
                    grafikui.Height = this.ClientRectangle.Height - grafikui.Location.Y;
                    gheight = grafikui.Height;
                    grafikas.Height = grafikui.Height - gmheight;
                    grafikas.Width = grafikui.Width;
                }
                if ((this.ClientRectangle.Width - dwidth) / pwidth != langeliai.GetLength(0) || (this.ClientRectangle.Height - grafikui.Height) / pheight != langeliai.GetLength(1))
                {
                    if ((this.ClientRectangle.Height - grafikui.Height) / pheight != 1)
                    {
                        pakeistilangeliumasyva((this.ClientRectangle.Width - dwidth) / pwidth, (this.ClientRectangle.Height - grafikui.Height) / pheight);
                    }
                }
                if(grafikui.Location.Y <= 2 * pheight)
                {
                    grafikui.Location = new Point(0, 2 * pheight + 1);
                    grafikui.Height = this.ClientRectangle.Height - 2 * pheight - 1;
                    gheight = grafikas.Height;
                    gresize_MouseUp(sender, e);
                }
            }
        }
        private void gresize_MouseDown(object sender, MouseEventArgs e)
        {
            Panel linija = sender as Panel;
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeNS;
            if (e.Button == MouseButtons.Left)
            {
                linija.BackColor = Color.FromArgb(96, 96, 96);
                yselected = e.Y;
            }
        }
        private void gresize_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && yselected != -1)
            {
                Panel linija = sender as Panel;
                yselected = -1;
                if(this.ClientRectangle.Height-grafikui.Location.Y < gmheight+30)
                {
                    grafikui.Location = new Point(linija.Location.X, this.ClientRectangle.Height-gmheight);
                }
                else
                {
                    grsy = grafikui.Location.Y;
                }
                gheight = grafikui.Height;
                grafikas.Height = grafikui.Height - gmheight;
                grafikas.Width = grafikui.Width;
                linija.BackColor = Color.Transparent;
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                
            }
        }

        private void gmeniu_doubleclick(object sender, MouseEventArgs e)
        {
            if(this.ClientRectangle.Height - grafikui.Location.Y < gmheight + 30) //padidinamas iki normalaus dydzio
            {
                paneliuNormalizavimas();
                grafikasIsVisible = true;
            }
            else //sumazinamas iki gmeniu dydzio
            {
                grafikui.Location = new Point(0, this.ClientRectangle.Height - gmheight);
                grafikui.Height = gmheight;
                grafikasIsVisible = false;
            }
        }

        private void paneliuNormalizavimas()
        {
            if (grafikasIsVisible == true)
            {
                if (gheight > grafikui.Height)
                {
                    grafikui.Location = new Point(0, 2 * pheight + 1);
                    grafikui.Height = this.ClientRectangle.Height - 2 * pheight - 1;
                    if (grafikui.Height > gheight)
                    {
                        grafikui.Height = gheight;
                        grafikui.Location = new Point(0, this.ClientRectangle.Height - grafikui.Height);
                    }
                }
                else if (grafikui.Height + 2 * pheight >= this.ClientRectangle.Height) //kad mazinant langa grafikas neuzstotu daugiau nei 2 lenteliu is aukscio
                {
                    grafikui.Location = new Point(0, 2 * pheight + 1);
                    grafikui.Height = this.ClientRectangle.Height - 2 * pheight - 1;
                }
                //Debug.WriteLine(grafikui.Height);
                if (grafikui.Height > gmheight)
                {
                    grafikas.Height = grafikui.Height - gmheight;
                }
            }
            else
            {
                grafikui.Location = new Point(0, this.ClientRectangle.Height - gmheight);
                grafikui.Height = pheight;
            }
            grafikas.Width = grafikui.Width;
            desine.Location = new Point(this.ClientRectangle.Width - dwidth, 0);
            desine.Height = this.ClientRectangle.Height;
        }

        bool rastiLaisvaVieta(ref int x, ref int y)
        {
            for(int b = 0; b<langeliai.GetLength(1); b++)
            {
                for(int a = 0; a<langeliai.GetLength(0); a++)
                {
                    if(langeliai[a,b] == 0)
                    {
                        x = a;
                        y = b;
                        return true;
                    }
                }
            }
            return false;
        }

        void pakeistilangeliumasyva(int naujasx, int naujasy) // pakeicia langeliu masyvo dydi
        {
            int[,] nmasyvas = new int[naujasx, naujasy];
            for (int x = 0; x < naujasx; x++)
            {
                for(int y=0; y<naujasy; y++)
                {
                    if(x < langeliai.GetLength(0) && y < langeliai.GetLength(1))
                    {
                        nmasyvas[x, y] = langeliai[x,y];
                    }
                }
            }
            langeliai = nmasyvas.Clone() as int[,];
            foreach (Panel p in paneles)
            {
                if (p.Location.X/pwidth >= naujasx || p.Location.Y/pheight >= naujasy)
                {
                    p.Location = new Point(0, -pheight);
                }
            }
            uzpildytitusciuslangelius();
            SaveProgramSettings();
        }

        void uzpildytitusciuslangelius() // uzpildo tusciais langeliais jei tokiu yra
        {
            foreach(Panel p in paneles)
            {
                int x = p.Location.X/pwidth;
                int y = p.Location.Y/pheight;
                if(x == 0 && y == -1)
                {
                    if(rastiLaisvaVieta(ref x,ref y))
                    {
                        p.Location = new Point(pwidth*x, pheight*y);
                        langeliai[x, y] = currencyList.NumberOfSymbol(p.Name)+1;
                    }
                    else
                    { break; }
                }
            }
        }

        private void divlentaSlelected(object sender, EventArgs e)
        {
            divlenta.ClearSelection();
        }

        public void eroras<T>(T zinute)
        {
            alerttext.Text = zinute.ToString();
            alerttext.ForeColor = Color.Red;
            if (File.Exists(loglink))
            {
                fs = new FileStream(loglink, FileMode.Append);
            }
            else
            {
                
                fs = new FileStream(loglink, FileMode.Create);
            }
            sw = new StreamWriter(fs);
            sw.WriteLine("Time: "+DateTime.Now+" Error: "+zinute.ToString());
            sw.Close();
            fs.Close();
        }

        private string changeTimeFormat(int minutes)
        {
            if(minutes < 10)
            {
                return minutes + "M ago";
            }
            else if (minutes < 30)
            {
                return (minutes - minutes % 5) + "M ago";
            }
            else if(minutes < 60)
            {
                return (minutes - minutes % 10) + "M ago";
            }
            else if (minutes < 60*24)
            {
                return minutes/60 + "H ago";
            }
            else if (minutes < 60 * 24 * 7)
            {
                return minutes / 60 / 24 + "D ago";
            }
            else
            {
                return minutes / 60 / 24 / 7 + "W ago";
            }
        }

        private void refresh_CSI() //sita dalis rabotaet) iskvieciama timerio kiekviena perioda
        {
            if (metatrader.refreshCharts() == true)
            {
                currencyList.refresh();
                int TikrasisIlgis = grafikoIlgis;
                double[] adjustConstant = new double[grafikoValiutos.Count];
                for (int x = 0; x < grafikoValiutos.Count; x++)
                {
                    grafikas.Series[2 * x].Points.Clear();
                    grafikas.Series[2 * x + 1].Points.Clear();
                    if (TikrasisIlgis > currencyList.CS_charts[grafikoTaimfreimas, grafikoValiutos[x]].CS_value.Length)
                    {
                        TikrasisIlgis = currencyList.CS_charts[grafikoTaimfreimas, grafikoValiutos[x]].CS_value.Length; //ilgis, kuri parodys grafikas, nebux didesnis nei tikrasisIlgis
                    }
                }
                for (int x = 0; x < grafikoValiutos.Count; x++)
                {
                    adjustConstant[x] = -currencyList.CS_charts[grafikoTaimfreimas, grafikoValiutos[x]].CS_value[TikrasisIlgis - 1];
                }
                //CS_charts vertes skaiciuojamos nuo kaires
                // vertes sudedamos i grafika
                for (int i = 0; i < TikrasisIlgis; i++)
                {
                    for (int x = 0; x < grafikoValiutos.Count; x++)
                    {
                        grafikas.Series[2 * x].Points.AddXY(i, 100 * (currencyList.CS_charts[grafikoTaimfreimas, grafikoValiutos[x]].CS_value[TikrasisIlgis - i - 1] + adjustConstant[x]));
                        grafikas.Series[2 * x + 1].Points.AddXY(i, 100 * (currencyList.CS_charts[grafikoTaimfreimas, grafikoValiutos[x]].Ema.value[TikrasisIlgis - i - 1] + adjustConstant[x]));
                    }
                }
                //atnaujina lenteles (paneles)
                foreach (Panel p in paneles)
                {
                    int simbolis = currencyList.NumberOfSymbol(p.Name);
                    double middle = 0;
                    for (int i = 0; i < 6; i++)
                    {
                        Label skaicius = p.Controls.Find(i.ToString() + i.ToString(), true)[0] as Label;
                        //double value = currencyList.CS_charts[i, simbolis].getPercentage();
                        double value;
                        if (show_probability)
                        { value = currencyList.CS_charts[i, simbolis].getPercentage(); }
                        else
                        { value = currencyList.CS_charts[i, simbolis].getRatio(); }
                        string skaiciukas = value.ToString();
                        if (value < middle)
                        {
                            skaicius.ForeColor = Color.FromArgb(195, 0, 0);
                            if ((value < -alertLimit && !show_probability) || (value < -100 * tradingMath.Erf(alertLimit) && show_probability))
                            {
                                skaicius.ForeColor = Color.FromArgb(255, 110, 0);//(255, 120, 40);
                            }
                        }
                        else
                        {
                            skaicius.ForeColor = Color.FromArgb(0, 175, 10);
                            if ((value > alertLimit && !show_probability) || (value > 100 * tradingMath.Erf(alertLimit) && show_probability))
                            {
                                skaicius.ForeColor = Color.FromArgb(20, 255, 0);
                            }
                        }
                        if (value > 0)
                        { skaiciukas = "+" + skaiciukas; }
                        if (skaiciukas.Length > 5)
                        {
                            skaiciukas = skaiciukas.Remove(5);
                        }
                        if (show_probability)
                        { skaiciukas += "%"; }
                        skaicius.Text = skaiciukas;
                    }
                }
            }
            else
            {
                Debug.WriteLine("neatnaujino chartu");
            }
        }

        private void refresh_divelenta()
        {
            List<macdRow> macdEilutes = new List<macdRow>();
            for (int x = 0; x < MQL_bridge.laikai.Length; x++)
            {
                for (int y = 0; y < metatrader.simboliai.Count; y++)
                {
                    Chart chartas = metatrader.getChart(MQL_bridge.laikai[x], metatrader.simboliai[y]);
                    chartas.macd = new Chart.MacdDivergence(chartas);
                    int macdResult = chartas.macd.DoubleDivergence();
                    if (macdResult != 0)
                    {
                        macdRow row = new macdRow();
                        row.simbol = metatrader.simboliai[y];
                        row.taimfreimas = (Taimai)MQL_bridge.laikai[x];
                        if (macdResult == 1)
                        {
                            row.minutes = (int)(DateTime.Now - chartas.time[chartas.macd.VcandleAgoFormed]).TotalMinutes;
                            row.bullish = false;
                            macdEilutes.Add(row);
                        }
                        else if (macdResult == -1)
                        {
                            row.minutes = (int)(DateTime.Now - chartas.time[chartas.macd.AcandleAgoFormed]).TotalMinutes;
                            row.bullish = true;
                            macdEilutes.Add(row);
                        }
                    }
                }
            }
            //macdEilutes = macdEilutes.GroupBy(x => x.minutes).ToList();
            macdEilutes = macdEilutes.OrderBy(x => x.minutes).ToList();

            int eiluciuLimitas = 40;
            if (macdEilutes.Count > eiluciuLimitas)
            {
                macdEilutes.RemoveRange(eiluciuLimitas, macdEilutes.Count - eiluciuLimitas);
                Debug.WriteLine("nukirpo divlenta");
            }
            divlenta.Rows.Clear();
            for (int i = 0; i < macdEilutes.Count - 1; i++)
            {
                string[] row = new string[3];
                row[0] = macdEilutes[i].simbol;
                row[1] = macdEilutes[i].taimfreimas.ToString();
                row[2] = changeTimeFormat(macdEilutes[i].minutes);
            }
            for (int i = 0; i < macdEilutes.Count - 1; i++)
            {
                string[] row = new string[3];
                row[0] = macdEilutes[i].simbol;
                row[1] = macdEilutes[i].taimfreimas.ToString();
                row[2] = changeTimeFormat(macdEilutes[i].minutes);
                divlenta.Rows.Add(row);
            }
            // nudazo eilutes reikiama spalva
            for (int i = 0; i < divlenta.Rows.Count - 1; i++)
            {
                if (macdEilutes[i].bullish == true)
                {
                    divlenta.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(180, 255, 180);
                }
                else
                {
                    divlenta.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 180, 180);
                }
            }
        }

        private void pavaizduotNaujaGrafika_doublePanelClick(object sender, EventArgs e)
        {
            Label objektas = sender as Label;
            //sukuriamas svarus naujas grafikas
            grafikoValiutos = new List<int>();
            grafikoValiutos.Add(currencyList.NumberOfSymbol(objektas.Parent.Name));
            grafikas.Series.Clear();
            for (int i = 0; i < 2; i++)
            {
                Series ser = new Series();
                ser.ChartArea = grafikas.ChartAreas[0].Name;
                ser.ChartType = SeriesChartType.Line;
                grafikas.Series.Add(ser);
            }
            /*grafikas.Series[0].Legend = objektas.Parent.Name;
            grafikas.Series[1].Legend = objektas.Parent.Name+" Ema";*/
            grafikoTaimfreimas = Int32.Parse(objektas.Name);
            if(grafikoTaimfreimas%11 == 0)
            { grafikoTaimfreimas = grafikoTaimfreimas / 11; }
            //Console.WriteLine(taimfreimas);
            refresh_CSI();
        }

        private void pridetiGrafika_panelRightClick(object sender, MouseEventArgs e)
        {
            Label objektas = sender as Label;
            if (e.Button == MouseButtons.Right)
            {
                if (!grafikoValiutos.Contains(currencyList.NumberOfSymbol(objektas.Parent.Name)))
                {
                    grafikoValiutos.Add(currencyList.NumberOfSymbol(objektas.Parent.Name));
                    for (int i = 0; i < 2; i++)
                    {
                        Series ser = new Series();
                        ser.ChartArea = grafikas.ChartAreas[0].Name;
                        ser.ChartType = SeriesChartType.Line;
                        grafikas.Series.Add(ser);
                    }
                }
                /*grafikas.Series[0].Legend = objektas.Parent.Name;
                grafikas.Series[1].Legend = objektas.Parent.Name+" Ema";*/
                grafikoTaimfreimas = Int32.Parse(objektas.Name);
                if (grafikoTaimfreimas % 11 == 0)
                { grafikoTaimfreimas = grafikoTaimfreimas / 11; }
                refresh_CSI();
            }
        }

        private void SaveProgramSettings()
        {
            fs = File.Create(generallink);
            sw = new StreamWriter(fs);
            sw.WriteLine("4");
            sw.WriteLine((int)wndst);
            sw.WriteLine(this.Size.Width);
            sw.WriteLine(this.Size.Height);
            sw.WriteLine(gheight);
            sw.WriteLine(grafikasIsVisible);

            // suraso paneliu valiuta ir ju vieta
            sw.WriteLine(langeliai.GetLength(0)+" "+langeliai.GetLength(1));
            for (int b = 0; b < langeliai.GetLength(1); b++)
            {
                string eilute = "";
                for (int a = 0; a < langeliai.GetLength(0); a++)
                {
                    eilute += langeliai[a, b].ToString()+";";
                }
                sw.WriteLine(eilute);
            }
            sw.Close();
            fs.Close();
        }

        private void Form1_setup()
        {
            if (currencyList.errorCode == 0)
            {
                combolentelem.Items.AddRange(currencyList.symbol);
                if (langeliai == null)
                {
                    int plotis = (this.Width - dwidth) / pwidth;
                    int aukstis = (this.ClientRectangle.Height - grafikui.Height) / pheight;
                    langeliai = new int[plotis, aukstis];
                }
                else
                {
                    for (int y = 0; y < langeliai.GetLength(1); y++)
                    {
                        for (int x = 0; x < langeliai.GetLength(0); x++)
                        {
                            if (langeliai[x, y] != 0)
                            {
                                pridetilentele(currencyList.symbol[langeliai[x, y] - 1], x, y);
                                combolentelem.Items.Remove(currencyList.symbol[langeliai[x, y] - 1]);
                            }
                        }
                    }
                }
            }
        }
        private void checkForMissingFiles(object sender, EventArgs e)
        {
            currencyList = new Currency(metatrader);
            if(currencyList.errorCode == 0)
            {
                this.Enabled = true;
                Form1_setup();
                timeris1.Tick -= checkForMissingFiles;
                timeris1.Tick += (s, ev) => { refresh_CSI(); };
                timeris1.Interval = 1000;
                timeris2.Tick += (s, ev) => { refresh_divelenta(); };
                Form1_setup();
            }
            else
            {
                //eroras("Not all files were found");
            }
        }

    }
}
