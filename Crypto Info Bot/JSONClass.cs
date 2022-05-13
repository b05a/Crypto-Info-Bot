using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto_Info_Bot
{
    class HttpCyptoResponse
    {
        public Dictionary<string, double> list = new Dictionary<string, double>();
        public bool AddCrypto(string Name, double price)
        {
            foreach (var item in list)
            {
                if (item.Key==Name) return true;
            }
            list.Add(Name, price);
            return true;
        }
        public void Info()
        {
            foreach (var item in list) Console.WriteLine($"{item.Key} {item.Value}");
        }
    }
    class HttpCryptoName
    {
        public double USD { get; set; }
    }
}
