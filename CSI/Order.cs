using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI
{
    public class Order
    {
        public Type type;
        public string simbol;
        public double volume;
        public double price = -1;
        public double takeProfit = -1;
        public double stopLoss = -1;
        public Status status;
        public int ticket = -1;
        public DateTime time;
        public int magicNumber;
        public enum Type
        {
            buy, sell
        }
        public enum Status
        {
            notsent,
            pending,
            filled
        }
    }
    public class marketOrder : Order
    {
        public marketOrder(Type tipas, double kiekis, string simbolis)
        {
            simbol = simbolis;
            type = tipas;
            volume = kiekis;
            status = Status.pending;
        }
    }
    public class limitOrder : Order
    {
        public limitOrder(string simbolis, Type tipas, double kiekis, double kaina)
        {
            simbol = simbolis;
            type = tipas;
            volume = kiekis;
            status = Status.pending;
            price = kaina;
        }
    }
    public class stopOrder : Order
    {
        public stopOrder(string simbolis, Type tipas, double kiekis, double kaina)
        {
            simbol = simbolis;
            type = tipas;
            volume = kiekis;
            status = Status.pending;
            price = kaina;
        }
    }
}
