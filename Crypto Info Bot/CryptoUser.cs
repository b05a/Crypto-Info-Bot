using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Crypto_Info_Bot
{
    public class Crypto
    {
        //public int Id { get; set; }
        [Key]
        public string Name { get; set; }
        public double Price { get; set; }

        public List<CryptoUser> users { get; set; } = new();
        public Crypto(string name, double price)
        {
            Name = name;
            Price = price;
        }
    }
    public class CryptoUser
    {

        // Имя пользователя (номер чата)
        [Key]
        public long Name { get; set; }

        // Процент изменения при котором происходит оповещение
        public double prcntChange=0;
        
        // Значение предидущего оповещения
        public Dictionary<string,double> banValue =new Dictionary<string,double>();
        
        // Список крипты
        //public List<string> listCryptoUser = new List<string>();
        public List<Crypto> listCrypto { get; set; } = new();

        // Переменная указывающая на то будет ли добаваться крипта
        public bool changeCryptoListListenetAdd=false;

        // Переменная указывающая на то будет ли удаляться крипта
        public bool changeCryptoListListenetDel = false;
        
        // Переменная указвающая на то включены или нет уведомления
        public bool notification = false;

        
        public void changeBanValue(string name, double value)
        {
            foreach (var item in banValue)
            {
                if (banValue.ContainsKey(name))
                {
                    banValue[name] = value;
                }
            }
        }

        // Метод добавления крипты
        public string addCrypto(Crypto crypto, double price)
        {
            //if (listCryptoUser.Count >= 49) return "Crypto not added as the list is limited to 50";
            if (listCrypto.Count >= 49) return "Crypto not added as the list is limited to 50";
            // Перебираем текущий список
            //foreach (string item in listCryptoUser)
            //{
            //    // Если крипта уже есть то отменяем добавление
            //    if (item.Equals(crypto)){ return "Сrypto already exists"; }
                
            //}
            foreach (Crypto item in listCrypto)
            {
                // Если крипта уже есть то отменяем добавление
                if (item.Equals(crypto)) { return "Сrypto already exists"; }

            }
            // Если крипты нет то добавляем ее в список
            //listCryptoUser.Add(crypto);
            

            listCrypto.Add(crypto);
            banValue.Add(crypto.Name, price);
            return "Сryptо added";
        }

        // Метод удаление крипты
        public bool delCrypto(string crypto)
        {
            //// Перебираем текущий список
            //foreach (string item in listCryptoUser)
            //{
            //    // Если крипта есть в списке то удаляем ее
            //    if (item.Equals(crypto)) { 
            //        listCryptoUser.Remove(item);
            //        banValue.Remove(crypto);
            //        return true; }
            //}

            // Перебираем текущий список
            foreach (Crypto item in listCrypto)
            {
                // Если крипта есть в списке то удаляем ее
                if (item.Name.Equals(crypto))
                {
                    listCrypto.Remove(item);
                    banValue.Remove(crypto);
                    return true;
                }
            }
            // если нет то возвращаем
            return false;
        }

        // Конструктор создание экземпляра класса
        public CryptoUser(long Name)
        {
            this.Name = Name;
        }

        // Метод для получения информации о пользователе
        public void Info()
        {
            // Пишем в консоль имя, периодичность запроса, процент изменения, предидущее значение
            Console.WriteLine($"{Name} {prcntChange}  {notification} INFO");
            //// Перебираем список крипты
            //foreach (string item in listCryptoUser)
            //{
            //    // Пишем название крипты в консоль
            //    Console.WriteLine(item);
            //}

            foreach (Crypto item in listCrypto)
            {
                // Пишем название крипты в консоль
                Console.WriteLine(item.Name);
            }


            foreach (KeyValuePair<string,double> item in banValue)
            {
                Console.WriteLine("Ban value "+item.Key+" "+item.Value);
            };
            Console.WriteLine();




        }
        public string InfoToChat()
        {
            var list = "";
            //foreach (string item in listCryptoUser)
            //{
            //    list += item + " ";

            //}

            foreach (Crypto item in listCrypto)
            {
                list += item.Name + " ";
            }

            return $"List of crypts for notifications: {list}\n"+
                $"Сhange percentage for notification: {prcntChange}%\n" +
                $"Notifications: {notification}";
        }
    }

    // Класс списка пользователей
    public class CryptoUsersList
    {

        // Список пользователей
        public Dictionary<long, CryptoUser> listUsers = new Dictionary<long, CryptoUser>();

        // Метод добавляющий пользователя
        public void AddUser(long name)
        {
            foreach (var item in listUsers)
            {
                if (item.Key.Equals(name)) return;
            }
            listUsers.Add(name, new CryptoUser(name));
        }

        // Метод удаляющий пользователя
        public void DelUser(long name)
        {
            // Перебираем список пользователей
            foreach (var item in listUsers)
            {
                // если в списках пользователь есть то удаляем
                if (item.Key.Equals(name)) listUsers.Remove(name);
            }
            return ;
        }

        // Метод перечисляющий пользователей
        public void Info()
        {
            // Перебираем список пользователей
            foreach (var item in listUsers)
            {
                // пишем в консоль информацию о пользователе
                item.Value.Info();
            }
            return;
        }
    }
}
