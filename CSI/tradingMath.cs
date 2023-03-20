using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI
{
    public static class tradingMath
    {
        public class Ema_indicator
        {
            public double[] value;
            public Ema_indicator(int days, double[] duom, double smoothing = 2)
            {
                int ilgis = duom.Length;
                value = new double[ilgis];
                for (int i = ilgis - 1; i >= 0; i--)
                {
                    double verte = duom[i];
                    if (i != ilgis - 1)
                    {

                        value[i] = smoothing / (days + 1) * (verte - value[i + 1]) + value[i + 1];
                    }
                    else // [igis-1] yra pirmoji charto verte ir jai skaiciuoti nereikia
                    {
                        value[ilgis - 1] = verte;
                    }
                }
            } //vertes isdeliotos nuo desines
        }
        public static double Erf(double x) //erf funkcija reiksmiu sritis Ex={-1;1}
        {
            // constants
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            // Save the sign of x
            int sign = 1;
            if (x < 0)
                sign = -1;
            x = Math.Abs(x);

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return sign * y;
        }
        public static double standardDeviation(double[] duomenys)
        {
            double sumOfX2 = 0;
            for (int i = 0; i < duomenys.Length; i++)
            {
                sumOfX2 += Math.Pow(duomenys[i], 2);
            }
            double rezultatas = Math.Sqrt(sumOfX2 / (duomenys.Length - 1));
            //standartineDeviacija = rezultatas;
            return rezultatas;
        }
        public static double average(double[] duomenys)
        {
            double sum = 0;
            for(int i=0; i<duomenys.Length; i++)
            {
                sum += Math.Abs(duomenys[i]);
            }
            return sum / duomenys.Length;
        }
    }
}
