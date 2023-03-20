using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms.DataVisualization;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections;
using System.IO;
//using System.Linq;

namespace CSI
{
    public class Form2 : Form
    {
        Form1 forma1;
        const int langoPlotis = 900, langoAukstis = 550;
        const int border = 50;

        //kaires puses elementai
        Panel kaire;
        Panel[] mygtukaiKaireje, skaidresDesineje;
        Label[] mygtukuPavadinimai;
        int activeBtn = 0;
        int sonMygtPlotis = 150, sonMygtAukstis = 80;

        // pirma skaidre
        System.Windows.Forms.DataVisualization.Charting.Chart basketPie;
        Button saveBtn1, changeKof;
        ListBox valiutuLentele;
        TextBox koeficientams;
        Label kofSuma;
        double[] valiutuKoeficientai;
        double koeficientuSuma;

        //antra skaidre
        ListBox simboliai;
        TextBox naujasSimbolis;
        Button addsimbol, deleteSimbol;

        //trecia skaidre
        Label activeData, failai;

        Timer timeris;
        public Form2(Form1 forma)
        {
            forma1 = forma;
            this.BackColor = Form1.sviesiaiMelyna;
            this.FormClosing += new FormClosingEventHandler(langasUzdarytas);
            this.Size = new Size(langoPlotis, langoAukstis);
            this.MinimumSize = new Size(langoPlotis, langoAukstis);
            this.MaximumSize = new Size(langoPlotis, langoAukstis);

            //kaire settings puse, kur yra mygtukai
            #region
            mygtukaiKaireje = new Panel[3];
            skaidresDesineje = new Panel[3];
            mygtukuPavadinimai = new Label[3];
            for(int i=0; i<mygtukaiKaireje.Length; i++)
            {
                mygtukaiKaireje[i] = new Panel();
                mygtukaiKaireje[i].Location = new Point(0, i * sonMygtAukstis);
                mygtukaiKaireje[i].Size = new Size(sonMygtPlotis, sonMygtAukstis);
                mygtukaiKaireje[i].ForeColor = Color.White;
                mygtukaiKaireje[i].BorderStyle = BorderStyle.FixedSingle;
                mygtukaiKaireje[i].MouseClick += new MouseEventHandler(sonBtn_Clicked);
                mygtukuPavadinimai[i] = new Label();
                mygtukuPavadinimai[i].ForeColor = Color.White;
                mygtukuPavadinimai[i].MouseClick += new MouseEventHandler(sonBtn_Clicked);
                mygtukuPavadinimai[i].Height = 40;
                mygtukuPavadinimai[i].Location = new Point(17, (sonMygtAukstis - mygtukuPavadinimai[i].Height) / 2);
                //mygtukuPavadinimai[i].TextAlign = ContentAlignment.
                mygtukaiKaireje[i].Controls.Add(mygtukuPavadinimai[i]);

                skaidresDesineje[i] = new Panel();
                skaidresDesineje[i].Location = new Point(sonMygtPlotis, 0);
                skaidresDesineje[i].Size = new Size(langoPlotis - sonMygtPlotis, langoAukstis);
                skaidresDesineje[i].BackColor = Form1.sviesiaiMelyna;
                skaidresDesineje[i].Visible = false;
                this.Controls.Add(skaidresDesineje[i]);
            }
            mygtukuPavadinimai[0].Text = "Currency\r\nbasket";
            mygtukuPavadinimai[1].Text = "Symbols";
            mygtukuPavadinimai[2].Text = "Status\r\nreport";
            kaire = new Panel();
            kaire.Location = new Point(0, 0);
            kaire.Size = new Size(sonMygtPlotis, langoAukstis);
            kaire.BackColor = Form1.tamsiaiMelyna;
            for (int i = 0; i < mygtukaiKaireje.Length; i++)
            {
                kaire.Controls.Add(mygtukaiKaireje[i]);
            }
            this.Controls.Add(kaire);
            #endregion

            // pirmos skaidres elementai
            #region
            basketPie = new System.Windows.Forms.DataVisualization.Charting.Chart();
            basketPie.Location = new Point(border-11, border-11);
            basketPie.Size = new Size(433, 433);
            basketPie.BackColor = Form1.sviesiaiMelyna;
            basketPie.ChartAreas.Add(new ChartArea());
            saveBtn1 = new Button();
            saveBtn1.Text = "Save";
            saveBtn1.Size = new Size(80, 30);
            saveBtn1.Location = new Point(ClientRectangle.Width-sonMygtPlotis-saveBtn1.Width-border, ClientRectangle.Height-border-saveBtn1.Height);
            saveBtn1.BackColor = Color.White;
            saveBtn1.Font = new Font("Arial", 13, FontStyle.Regular);
            saveBtn1.Click += new EventHandler(valiutuSarasas_Irasyti);
            valiutuLentele = new ListBox();
            valiutuLentele.Size = new Size(175, 260);
            valiutuLentele.Location = new Point(ClientRectangle.Width - sonMygtPlotis - valiutuLentele.Width - border, border);
            valiutuLentele.Font = Form1.fontas1;
            valiutuLentele.SelectedIndexChanged += new EventHandler(valiutuSarasas_Selected);
            Label kofai = new Label();
            kofai.Location = new Point(507, 307);
            kofai.Font = Form1.fontas1;
            kofai.ForeColor = Color.White;
            kofai.Text = "Coefficients:";
            kofai.Width = 175;
            koeficientams = new TextBox();
            koeficientams.Location = new Point(509, 335);
            koeficientams.Width = 85;
            koeficientams.Font = new Font("Arial", 13, FontStyle.Regular);
            koeficientams.KeyUp += new KeyEventHandler(spaudzia_Enter);
            changeKof = new Button();
            changeKof.Height = 30;
            changeKof.Width = 80;
            changeKof.Location = new Point(ClientRectangle.Width - sonMygtPlotis - changeKof.Width - border, 334);
            changeKof.Font = new Font("Arial", 13, FontStyle.Regular);
            changeKof.BackColor = Color.White;
            changeKof.Text = "change";
            changeKof.Click += new EventHandler(valiutuSarasas_Keisti);
            kofSuma = new Label();
            kofSuma.Location = new Point(507, 367);
            kofSuma.ForeColor = Color.White;
            kofSuma.Font = Form1.fontas1;
            skaidresDesineje[0].Controls.Add(basketPie);
            skaidresDesineje[0].Controls.Add(saveBtn1);
            skaidresDesineje[0].Controls.Add(valiutuLentele);
            skaidresDesineje[0].Controls.Add(kofai);
            skaidresDesineje[0].Controls.Add(koeficientams);
            skaidresDesineje[0].Controls.Add(changeKof);
            skaidresDesineje[0].Controls.Add(kofSuma);
            #endregion

            //antros skaidres elementai
            #region
            simboliai = new ListBox();
            simboliai.Size = new Size(270, 308);
            simboliai.Location = new Point(ClientRectangle.Width-sonMygtPlotis-simboliai.Width-border-4, border);
            simboliai.Font = Form1.fontas1;
            naujasSimbolis = new TextBox();
            naujasSimbolis.Location = new Point(410, 366);
            naujasSimbolis.Font = new Font("Arial", 13, FontStyle.Regular);
            naujasSimbolis.Width = 92;
            naujasSimbolis.TextChanged += (s, e) => { naujasSimbolis.Text = naujasSimbolis.Text.ToUpper(); naujasSimbolis.SelectionStart = naujasSimbolis.Text.Length; };
            addsimbol = new Button();
            addsimbol.Size = new Size(80, 30);
            addsimbol.Location = new Point(512, 365);
            addsimbol.Text = "Add";
            addsimbol.Font = new Font("Arial", 13, FontStyle.Regular);
            addsimbol.BackColor = Color.White;
            addsimbol.Click += pridetiSimboli;
            deleteSimbol = new Button();
            deleteSimbol.Size = new Size(80, 30);
            deleteSimbol.Location = new Point(ClientRectangle.Width - sonMygtPlotis - deleteSimbol.Width - border - 4, 365);
            deleteSimbol.Text = "Delete";
            deleteSimbol.Font = new Font("Arial", 13, FontStyle.Regular);
            deleteSimbol.BackColor = Color.White;
            deleteSimbol.Click += atimtiSimboli;
            Label label1 = new Label();
            label1.Text = "List of symbols to take\r\nfrom MetaTrader platform:";
            label1.Font = Form1.fontas1;
            label1.Location = new Point(border, border);
            label1.Size = new Size(250, 60);
            label1.ForeColor = Color.White;
            Label label2 = new Label();
            label2.Text = "Make sure to write\r\nsimbols correctly";
            label2.Font = new Font(Form1.fontas1, FontStyle.Italic);
            label2.Location = new Point(border, 348);
            label2.Size = new Size(250, 60);
            label2.ForeColor = Color.White;
            skaidresDesineje[1].Controls.Add(simboliai);
            skaidresDesineje[1].Controls.Add(naujasSimbolis);
            skaidresDesineje[1].Controls.Add(addsimbol);
            skaidresDesineje[1].Controls.Add(deleteSimbol);
            skaidresDesineje[1].Controls.Add(label1);
            skaidresDesineje[1].Controls.Add(label2);
            #endregion
            //trecios skaidres elementai
            #region
            Label label3 = new Label();
            label3.Text = "Last time metatrader bridge was active:";
            label3.Font = Form1.fontas1;
            label3.ForeColor = Color.White;
            label3.Location = new Point(border, border);
            label3.Size = new Size(230, 60);
            activeData = new Label();
            activeData.Font = new Font("Arial", 13, FontStyle.Regular);
            activeData.ForeColor = Color.White;
            activeData.Location = new Point(350, 60);
            activeData.Size = new Size(150, 60);
            Label label4 = new Label();
            label4.Text = "Necessary files:";
            label4.Font = Form1.fontas1;
            label4.ForeColor = Color.White;
            label4.Location = new Point(border, border+70);
            label4.Size = new Size(150, 60);
            failai = new Label();
            failai.Font = new Font("Arial", 13, FontStyle.Regular);
            failai.ForeColor = Color.White;
            failai.Location = new Point(350, 130);
            failai.Size = new Size(250, 60);
            skaidresDesineje[2].Controls.Add(label3);
            skaidresDesineje[2].Controls.Add(activeData);
            skaidresDesineje[2].Controls.Add(label4);
            skaidresDesineje[2].Controls.Add(failai);
            #endregion
            if (forma1.currencyList.errorCode != 0)
            {
                activeBtn = 2;
                mygtukaiKaireje[0].Enabled = false;
                mygtukaiKaireje[1].Enabled = false;
            }
            sonBtn_SetActive(mygtukaiKaireje[activeBtn]);
            timeris = new Timer();
            timeris.Interval = 1000;
            timeris.Tick += (s, e) => { if (activeBtn == 2) { StatusReport_Open(); } };
            timeris.Start();
        }

        private void langasUzdarytas(object sender, System.EventArgs e)
        {
            forma1.Enabled = true;
            forma1 = new Form1();
        }

        private void sonBtn_Clicked(object sender, MouseEventArgs e)
        {
            sonBtn_SetActive(sender);
        }

        private void sonBtn_SetActive(object sender)
        {
            int mygtukoNumeris;
            Label pavadinimas;
            Panel mygtukas;
            if (sender.GetType() == typeof(Panel))
                mygtukoNumeris = Array.IndexOf(mygtukaiKaireje, sender);
            else
                mygtukoNumeris = Array.IndexOf(mygtukuPavadinimai, sender);
            activeBtn = mygtukoNumeris;
            pavadinimas = mygtukuPavadinimai[mygtukoNumeris];
            mygtukas = mygtukaiKaireje[mygtukoNumeris];
            for(int i=0; i<mygtukaiKaireje.Length; i++)
            {
                if(i != mygtukoNumeris)
                {
                    mygtukaiKaireje[i].BackColor = Form1.tamsiaiMelyna;
                    mygtukuPavadinimai[i].Font = new Font("Arial", 12, FontStyle.Regular);
                    mygtukaiKaireje[i].BorderStyle = BorderStyle.FixedSingle;
                    skaidresDesineje[i].Visible = false;
                }
            }
            mygtukaiKaireje[mygtukoNumeris].BackColor = Form1.sviesiaiMelyna;
            mygtukuPavadinimai[mygtukoNumeris].Font = new Font("Arial", 12, FontStyle.Bold);
            mygtukaiKaireje[mygtukoNumeris].BorderStyle = BorderStyle.None;
            //--------activeBtn = mygtukoNumeris;
            //akyvuojame vaizda desineje
            skaidresDesineje[mygtukoNumeris].Visible = true;
            if (mygtukoNumeris == 0)
                CurrencyBasket_Open();
            else if (mygtukoNumeris == 1)
                Symbols_Open();
            else if (mygtukoNumeris == 2)
                StatusReport_Open();
        }
        //pirma skaidre
        private void CurrencyBasket_Open()
        {
            //pieChart'o iniciavimas
            #region
            Series serija = new Series();
            serija.Name = "basket";
            serija.ChartType = SeriesChartType.Pie;
            serija.ChartArea = basketPie.ChartAreas[0].Name;
            basketPie.Series.Clear();
            basketPie.Series.Add(serija);
            valiutuKoeficientai = new double[forma1.currencyList.Count];
            koeficientuSuma = 0;
            for (int i=0; i<forma1.currencyList.Count; i++)
            {
                valiutuKoeficientai[i] = forma1.currencyList.basketCoeff[i];
                if (forma1.currencyList.basketCoeff[i] != 0)
                {
                    string simbolis = forma1.currencyList.symbol[i];
                    double koeficientas = forma1.currencyList.basketCoeff[i];
                    basketPie.Series[0].Points.AddXY(simbolis, koeficientas);
                    koeficientuSuma += koeficientas;
                }
            }
            kofSuma.Text = "Out of: " + koeficientuSuma;
            #endregion

            //valiutu sudejimas i lentele
            #region
            valiutuLentele.Items.Clear();
            for(int i=0; i<forma1.currencyList.Count; i++)
            {
                valiutuLentele.Items.Add(forma1.currencyList.symbol[i]);
            }
            #endregion
        }
        private void valiutuSarasas_Selected(object sender, EventArgs e)
        {
            //if(valiutuLentele.)
            if (valiutuLentele.SelectedIndex != -1)
            {
                string valiuta = valiutuLentele.SelectedItem.ToString();
                int numeris = forma1.currencyList.NumberOfSymbol(valiuta);
                koeficientams.Text = Convert.ToString(valiutuKoeficientai[numeris]);
            }
        }
        private void valiutuSarasas_Keisti(object sender, EventArgs e)
        {
            int pasirinktosValiutosNumeris = valiutuLentele.SelectedIndex;
            if (valiutuLentele.SelectedIndex != -1)
            {
                string pasirinktaValiuta = valiutuLentele.Items[pasirinktosValiutosNumeris].ToString();
                double kofas;
                if (double.TryParse(koeficientams.Text, out kofas) == true && (kofas > 0 || kofas == 0 && valiutuKoeficientai[pasirinktosValiutosNumeris] != 0))
                {
                    koeficientuSuma = koeficientuSuma + kofas - valiutuKoeficientai[pasirinktosValiutosNumeris];
                    kofSuma.Text = "Out of: " + koeficientuSuma;
                    valiutuKoeficientai[pasirinktosValiutosNumeris] = kofas;
                    if (forma1.currencyList.basketCoeff[pasirinktosValiutosNumeris] == 0)
                    {
                        basketPie.Series[0].Points.AddXY(pasirinktaValiuta, kofas);
                    }
                    else
                    {
                        valiutuKoeficientai[pasirinktosValiutosNumeris] = kofas;
                        basketPie.Series[0].Points.Clear();
                        for (int i = 0; i < forma1.currencyList.Count; i++)
                        {
                            if (valiutuKoeficientai[i] != 0)
                            {
                                string simbolis = forma1.currencyList.symbol[i];
                                double koeficientas = valiutuKoeficientai[i];
                                basketPie.Series[0].Points.AddXY(simbolis, koeficientas);
                                koeficientuSuma += koeficientas;
                            }
                        }
                    }
                }
                else
                {
                    koeficientams.Text = "Invalid";
                }

            }
        }
        private void valiutuSarasas_Irasyti(object sender, EventArgs e)
        {
            FileStream fs = File.Create(Currency.cofflink);
            StreamWriter sw = new StreamWriter(fs);
            int irasytuSkaicius = 0;
            for (int i = 0; i < valiutuKoeficientai.Length; i++)
            {
                if (valiutuKoeficientai[i] != 0)
                {
                    irasytuSkaicius++;
                }
            }
            sw.WriteLine(irasytuSkaicius);
            for (int i = 0; i < valiutuKoeficientai.Length; i++)
            {
                if (valiutuKoeficientai[i] != 0)
                {
                    sw.WriteLine(forma1.currencyList.symbol[i] + " " + valiutuKoeficientai[i]);
                }
            }
            sw.Close();
            fs.Close();
            forma1.currencyList = new Currency(forma1.metatrader);
        }
        private void spaudzia_Enter(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                valiutuSarasas_Keisti(sender, e);
            }
        }
        //antra skaidre
        private void Symbols_Open()
        {
            simboliai.Items.Clear();
            for(int i=0; i<forma1.metatrader.simboliai.Count; i++)
            {
                simboliai.Items.Add(forma1.metatrader.simboliai[i]);
            }
        }
        private void pridetiSimboli(object sender, EventArgs e)
        {
            if(naujasSimbolis.Text != string.Empty)
            {
                FileStream fs = File.OpenWrite(MQL_bridge.Allpairslink);
                StreamWriter sw = new StreamWriter(fs);
                int kiekis = forma1.metatrader.simboliai.Count;
                sw.WriteLine(kiekis+1);
                for (int i=0; i<kiekis; i++)
                {
                    sw.WriteLine(forma1.metatrader.simboliai[i]);
                }
                sw.WriteLine(naujasSimbolis.Text);
                sw.Close();
                fs.Close();
                naujasSimbolis.Text = string.Empty;
            }
        }
        private void atimtiSimboli(object sender, EventArgs e)
        {
            if(simboliai.SelectedIndex != -1)
            {
                simboliai.Items.RemoveAt(simboliai.SelectedIndex);
                FileStream fs = File.OpenWrite(MQL_bridge.Allpairslink);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(simboliai.Items.Count);
                for(int i=0; i<simboliai.Items.Count; i++)
                {
                    sw.WriteLine(simboliai.Items[i]);
                }
                sw.Close();
                fs.Close();
            }
        }
        //trecia skaidre
        private void StatusReport_Open()
        {
            string lastTimeActive =  File.GetLastWriteTime(MQL_bridge.inputlink+MQL_bridge.activeName).ToString();
            activeData.Text = lastTimeActive;
            failai.ForeColor = Color.Red;
            if(File.Exists(MQL_bridge.Allpairslink) == false)
            {
                failai.Text = "mainSymbols.txt was not found";
            }
            else if (File.Exists(Currency.cofflink) == false)
            {
                failai.Text = "coefficientsForCurrencyBasket.txt was not found";
            }
            else
            {
                failai.Text = "were found";
                failai.ForeColor = Color.Green;
            }
        }

    }
}
