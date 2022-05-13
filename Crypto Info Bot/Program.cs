// See https://aka.ms/new-console-template for more information
using Crypto_Info_Bot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.EntityFrameworkCore;
using (ApplicationContext db = new ApplicationContext())

{

    var firstStart = true;

    // Переменные для выхода из цикла программы
    var e = "";
    var d = "";

    // Переменная запроса в которую записывается JSON крипты
    string Response;

    // Список пользователей
    CryptoUsersList cryptoUsersList = new();

    
    // Список крипты (класс)
    Dictionary<string,Crypto> cryproListMain = new();


    // Создаем экземпляр класса для серилизации запросов цен
    HttpCyptoResponse listRequest = new();
    HttpCryptoName t;

    // Список с криптой пользователей для запроса
    List<string> listCryptoUsers = new();
    //listCryptoUsers.Add("BTC");
    //listCryptoUsers.Add("LTC");
    //listCryptoUsers.Add("ETH");

    // Строка списка с криптой для http запроса
    string listCryptoRequest = "";

    // Создаем телеграмм бота
    var botClient = new TelegramBotClient("1164009224:AAEnk7ZM3ckaBZGLLC33X7ngQzjbLisPqEI");

    // Создаем веб-клиент
    System.Net.WebClient we = new System.Net.WebClient();



    using var cts = new CancellationTokenSource();

    // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
    var receiverOptions = new ReceiverOptions
    {
        AllowedUpdates = { } // receive all update types
    };


    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
    {
        new KeyboardButton[] { "Add Crypto", "Setting Crypto", "Delete Crypto" },
        new KeyboardButton[] { "F.A.Q.", "Info Crypto price", "On/Off Notification" },
    })
    {
        ResizeKeyboard = true
    };

    ReplyKeyboardMarkup replySetting = new(new[]
    {
        new KeyboardButton[] { "Info My Crypto setting", "Set value change" },
    })
    {
        ResizeKeyboard = true
    };

    ReplyKeyboardMarkup replySetTimerPeriod = new(new[]
    {
        new KeyboardButton[] { "1 min", "5 min", "10 min", " 30 min" },
        new KeyboardButton[] { "1 hour", "2 hour", "3 hour", "4 hour" },
        new KeyboardButton[] { "6 hour", "9 hour", "12 hour", "24 hour" },
    })
    {
        ResizeKeyboard = true
    };

    ReplyKeyboardMarkup replySetValueChange = new(new[]
    {
        new KeyboardButton[] { "0.1%", "0.2%", "0.3%", "0.4%" },
        new KeyboardButton[] { "0.5%", "0.7%", "1%", "1.2%" },
        new KeyboardButton[] { "1.5%", "1.7%", "2%", "2.5%" },
        new KeyboardButton[] { "3%", "4%", "5%", "10%" },
    })
    {
        ResizeKeyboard = true
    };


    botClient.StartReceiving(
        HandleUpdateAsync,
        HandleErrorAsync,
        receiverOptions,
        cancellationToken: cts.Token);

    var me = await botClient.GetMeAsync();




    Console.WriteLine($"Start listening for @{me.Username}");

    // Запускаем функцию запросов
    massageToChatAndRequest();

    while (e != "Exit")
    {
        e = Console.ReadLine();
        d = e;
    }

    // Send cancellation request to stop bot
    cts.Cancel();

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Type != UpdateType.Message)
            return;
        // Only process text messages
        if (update.Message!.Type != MessageType.Text)
            return;

        var chatId = update.Message.Chat.Id;
        var messageText = update.Message.Text;

        // проверяем создан ли экземпляр класса пользователья
        try
        {
            if (cryptoUsersList.listUsers[chatId].Name==chatId);
        }
        catch
        {
            // если не создан, то создаем
            cryptoUsersList.AddUser(chatId);

            await botClient.SendTextMessageAsync(
                      chatId: chatId,
                      text: "How to use the bot",
                      replyMarkup: replyKeyboardMarkup,
                      cancellationToken: cancellationToken);
            try
            {
                db.Users.Add(cryptoUsersList.listUsers[chatId]);
                db.SaveChanges();

            }
            catch
            {

            }
            
            return;

        }



        // проверяем нужно ли добавить крипту
        if (cryptoUsersList.listUsers[chatId].changeCryptoListListenetAdd)
        {
            cryptoUsersList.listUsers[chatId].changeCryptoListListenetAdd = false;

            try
            {
                // Запрос на сайт
                Response = we.DownloadString($"https://min-api.cryptocompare.com/data/pricemulti?fsyms={messageText}&tsyms=USD");

                // Преобразуем строку в объект JSON
                JObject jObject = JObject.Parse(Response);
                if (jObject.SelectToken(messageText) == null) {
                    await botClient.SendTextMessageAsync(
                      chatId: chatId,
                      text: "This crypt is not in the list of available",
                      replyMarkup: replyKeyboardMarkup,
                      cancellationToken: cancellationToken);
                    return;
                }
                Console.WriteLine(jObject.SelectToken(messageText));
                Console.WriteLine();

                var d =JsonConvert.DeserializeObject<HttpCryptoName>(jObject.SelectToken(messageText).ToString());

                try
                {
                    if (cryproListMain[messageText].Name == messageText) ;
                }
                catch
                {
                    cryproListMain.Add(messageText, new Crypto(messageText, d.USD));
                }

                // Если получается конвертировать, значит запрос правильный и добавляем в список крипты пользователя
                var r=cryptoUsersList.listUsers[chatId].addCrypto(cryproListMain[messageText],d.USD);
                                

                await botClient.SendTextMessageAsync(
                      chatId: chatId,
                      text: r,
                      replyMarkup: replyKeyboardMarkup,
                      cancellationToken: cancellationToken);
                
                return;
            }
            catch
            {
                await botClient.SendTextMessageAsync(
                      chatId: chatId,
                      text: "Try again or try later",
                      replyMarkup: replyKeyboardMarkup,
                      cancellationToken: cancellationToken);
                return;
            }
        }

        // проверяем нужно ли удалить крипту
        if (cryptoUsersList.listUsers[chatId].changeCryptoListListenetDel)
        {
            cryptoUsersList.listUsers[chatId].changeCryptoListListenetDel = false;
            if (cryptoUsersList.listUsers[chatId].delCrypto(messageText))
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Crypto deleted",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);

                foreach(KeyValuePair<long, CryptoUser> i in cryptoUsersList.listUsers)
                {
                    if (i.Key == chatId) continue;
                    foreach(Crypto e in i.Value.listCrypto)
                    {
                        if (e.Name == messageText) return;
                    }
                }

                return;
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Crypto not found",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                return;
            }

            
        }



        Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

        switch (messageText)
        {
            case "F.A.Q.":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Info",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                ;
                cryptoUsersList.Info();
                return;
            case "Info My Crypto setting":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: cryptoUsersList.listUsers[chatId].InfoToChat(),
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                ;
                cryptoUsersList.Info();
                return;
            case "Info Crypto price":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: messageInfoCryptoPrice(chatId),
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                ;
                cryptoUsersList.Info();
                return;
            case "On/Off Notification":
                if (cryptoUsersList.listUsers[chatId].notification)
                {
                    cryptoUsersList.listUsers[chatId].notification = false;
                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Notification off",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                }
                else
                {
                    cryptoUsersList.listUsers[chatId].notification = true;
                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Notification on",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                }
                cryptoUsersList.Info();
                return;
            case "0.1%":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Change the percentage at which the notification will be 0.1%",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                cryptoUsersList.listUsers[chatId].prcntChange = 0.1;
                cryptoUsersList.Info();
                return;
            case "0.2%":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Change the percentage at which the notification will be 0.2%",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                cryptoUsersList.listUsers[chatId].prcntChange = 0.2;
                cryptoUsersList.Info();
                return;
            case "0.3%":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Change the percentage at which the notification will be 0.3%",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                cryptoUsersList.listUsers[chatId].prcntChange = 0.3;
                cryptoUsersList.Info();
                return;
            case "0.4%":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Change the percentage at which the notification will be 0.4%",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                cryptoUsersList.listUsers[chatId].prcntChange = 0.4;
                cryptoUsersList.Info();
                return;
            case "0.5%":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Change the percentage at which the notification will be 0.5%",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                cryptoUsersList.listUsers[chatId].prcntChange = 0.5;
                cryptoUsersList.Info();
                return;
            case "0.7%":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Change the percentage at which the notification will be 0.7%",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                cryptoUsersList.listUsers[chatId].prcntChange = 0.7;
                cryptoUsersList.Info();
                return;
            case "1%":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Change the percentage at which the notification will be 1%",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                cryptoUsersList.listUsers[chatId].prcntChange = 1;
                cryptoUsersList.Info();
                return;
            case "1.2%":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Change the percentage at which the notification will be 1.2%",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                cryptoUsersList.listUsers[chatId].prcntChange = 1.2;
                cryptoUsersList.Info();
                return;
            case "1.5%":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Change the percentage at which the notification will be 1.5%",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                cryptoUsersList.listUsers[chatId].prcntChange = 1.5;
                cryptoUsersList.Info();
                return;
            case "1.7%":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Change the percentage at which the notification will be 1.7%",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                cryptoUsersList.listUsers[chatId].prcntChange = 1.7;
                cryptoUsersList.Info();
                return;
            case "2%":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Change the percentage at which the notification will be 2%",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                cryptoUsersList.listUsers[chatId].prcntChange = 2;
                cryptoUsersList.Info();
                return;
            case "2.5%":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Change the percentage at which the notification will be 2.5%",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                cryptoUsersList.listUsers[chatId].prcntChange = 2.5;
                cryptoUsersList.Info();
                return;
            case "3%":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Change the percentage at which the notification will be 3%",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                cryptoUsersList.listUsers[chatId].prcntChange = 3;
                cryptoUsersList.Info();
                return;
            case "4%":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Change the percentage at which the notification will be 4%",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                cryptoUsersList.listUsers[chatId].prcntChange = 4;
                cryptoUsersList.Info();
                return;
            case "5%":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Change the percentage at which the notification will be 5%",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                cryptoUsersList.listUsers[chatId].prcntChange = 5;
                cryptoUsersList.Info();
                return;
            case "10%":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Change the percentage at which the notification will be 10%",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                cryptoUsersList.listUsers[chatId].prcntChange = 10;
                cryptoUsersList.Info();
                return;
            case "Add Crypto":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Write the crypto one by one in the format: BTC",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                cryptoUsersList.listUsers[chatId].changeCryptoListListenetAdd = true;
                return;
            case "Setting Crypto":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Setting Crypto",
                    replyMarkup: replySetting,
                    cancellationToken: cancellationToken);
                return;
            case "Set value change":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Percent change for alert",
                    replyMarkup: replySetValueChange,
                    cancellationToken: cancellationToken);
                return;
            case "Delete Crypto":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Write a crypt for which you want to remove notifications in the format: BTC",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                cryptoUsersList.listUsers[chatId].changeCryptoListListenetDel = true;
                return;

        }



        // Echo received message text
        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Use the menu",
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
    }

    Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }





    // Функция отправляющая стоимость крипты
    string messageInfoCryptoPrice(long nameChat)
    {
        string list = "Crypto price\n";
        try
        {
            foreach (Crypto crypto in cryptoUsersList.listUsers[nameChat].listCrypto)
            {
                list += $"{crypto.Name} {listRequest.list[crypto.Name]}\n";
            }
        }
        catch
        {
            return "Try again or try later";
        }
        
        Console.WriteLine(list);
        return list;
        
    }








    // Функция циклов запросов
    async void massageToChatAndRequest()
    {

        // начало цикла
        while (d != "Выход")
        {
            if (firstStart)
            {
                firstStart = false;
                foreach (var i in db.Users)
                {
                    try
                    {
                        await botClient.SendTextMessageAsync(i.Name, $"The bot has been restarted for technical reasons, the alert settings have been removed, you need to set them again.");
                    }
                    catch
                    {
                        db.Users.Remove(i);
                        db.SaveChanges();
                    }

                }
            }

            listCryptoUsers.Clear();

            foreach (KeyValuePair<long, CryptoUser> crypto in cryptoUsersList.listUsers)
            {
                foreach (Crypto nameCrypto in crypto.Value.listCrypto)
                {
                    foreach (string name in listCryptoUsers)
                    {
                        if (name == nameCrypto.Name) continue;
                    }
                    listCryptoUsers.Add(nameCrypto.Name);
                }
            }
            Console.WriteLine();


            // Пишем время в консоли
            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss"));

            // Пишем крипту для запроса
            foreach (KeyValuePair<string,Crypto> i in cryproListMain)
            {
                Console.WriteLine($"Added {i.Key}");
            }
            Console.WriteLine();
            
            
            // Если список крипты пуст, то пишем в консоли, что он пуст
            if (listCryptoUsers.Count == 0) 
            { 
                Console.WriteLine("List crypto request is empty"); Thread.Sleep(5000); 
                Console.WriteLine();
                continue; 
            }            

            // Очищаем список цен крипты
            listRequest.list.Clear();
            // Очищаем список запроса крипты
            listCryptoRequest = "";
            // Заполняем список зкпроса крипты
            foreach (var crypto in listCryptoUsers)
            {
                listCryptoRequest += $"{crypto},";
            }

            // Пишем строку запроса крипты
            Console.WriteLine(listCryptoRequest);

            try
            {
                // Запрос на сайт
                Response = we.DownloadString($"https://min-api.cryptocompare.com/data/pricemulti?fsyms={listCryptoRequest}&tsyms=USD");
            }
            catch
            { Thread.Sleep(2000); continue; }

            // Преобразуем строку в объект JSON
            JObject jObject = JObject.Parse(Response);
            // Перебираем объекты JSON
            foreach (KeyValuePair<string, JToken?> nameCrypto in jObject)
            {
                // десерилисзуем строки с криптой
                t = JsonConvert.DeserializeObject<HttpCryptoName>(nameCrypto.Value.ToString());
                // добавляем крипту и ее цену в список
                listRequest.AddCrypto(nameCrypto.Key, t.USD);
            }

            // пишем в консоль список запрашиваемой крипты и ее цену
            listRequest.Info();

            // перебираем пользователей
            foreach(KeyValuePair<long, CryptoUser> User in cryptoUsersList.listUsers)
            {
                // если у пользователя отключены оповещения или процент изменения то пропускаем его
                if (!User.Value.notification || User.Value.prcntChange == 0) continue;
                // перебираем список предидущих оповещений пользователя
                foreach(KeyValuePair<string, double> item in User.Value.banValue)
                {
                    // вычисляем разницу предидущего оповещения с текущей ценой
                    var a = Math.Abs(((listRequest.list[item.Key]/item.Value)-1)*100);

                    Console.WriteLine(a);

                    // если разница больше процента изменения то продолжаем
                    if (a > User.Value.prcntChange) 
                    { 
                        // если цена увеличилась, то пишем что увеличилась
                        if(listRequest.list[item.Key]> item.Value)
                        {
                            try
                            {
                                await botClient.SendTextMessageAsync(User.Key, $"{item.Key} up to {listRequest.list[item.Key]}");
                            }
                            catch
                            {
                                // если оповещение не дошло, то удаляем пользователя, тк он скорее всего отключил бота
                                cryptoUsersList.DelUser(User.Key);
                            }                            
                        }
                        else // если уменьшилась, то пишем что уменьшилась
                        {
                            try
                            {
                                await botClient.SendTextMessageAsync(User.Key, $"{item.Key} down to {listRequest.list[item.Key]}");
                            }
                            catch
                            {
                                // если оповещение не дошло, то удаляем пользователя, тк он скорее всего отключил бота
                                cryptoUsersList.DelUser(User.Key);
                            }
                            
                        }
                        // делаем текущую цену ценой оповещения
                        User.Value.changeBanValue(item.Key, listRequest.list[item.Key]);
                    }
                }
            }



            //var mes = await botClient.SendTextMessageAsync(771071943, $"Start");

            // Задержка в миллисекундах
            Thread.Sleep(20000);
        }
    }
}
