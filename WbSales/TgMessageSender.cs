using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WPC = WbSales.JsonToSharpClasses;

namespace WbSales
{
    public class TgMessageSender
    {
        private static readonly ITelegramBotClient bot = new TelegramBotClient("1528966775:AAHu7tqcWVTh6bLxtwKA7Y-__ie4P6KYk5M");
        private static readonly long _myId = 860507683;
        private static readonly CancellationTokenSource _cts = new CancellationTokenSource();
        public Product Product { get; set; }
        public string ImgUrl { get; set; }
        //public object HandlePollingErrorAsync { get; private set; }
        private static ConcurrentDictionary<long, object> _startedChats = new();
        private static ConcurrentDictionary<long, WaitingChatInfo> _waitingForProductChats = new();

        private enum WaitingForProductChatStatus
        {
            MustSendVenderCode,
            MustSendSize,
        }

        class WaitingChatInfo
        {
            public WaitingForProductChatStatus Status;
            public List<WaitingProduct> WaitingProductList = new List<WaitingProduct>();
        }

        class WaitingProduct
        {
            public string Size { get; set; }
            public string VenderCode { get; set; }
            public bool IsStarted = false;

            public override string ToString()
            {
                return $"Артикул {VenderCode}, размер {Size}";
            }
        }

        public TgMessageSender(Product product, string imgUrl)
        {
            if (product != null)
                this.Product = product;
            if (!string.IsNullOrEmpty(imgUrl))
                this.ImgUrl = imgUrl;
        }

        static class MessageType
        {
            public const string Start = "/start";
            public const string Stop = "/stop";
            public const string WaitProductByVendorCode = "Хочу этот товар";
        }

        public static void StartListening()
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };
            //var updateReceiver = new QueuedUpdateReceiver(bot, receiverOptions);
            //var cts = new CancellationTokenSource();
            bot.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: _cts.Token
            );

            var me = bot.GetMeAsync();
        }

        private static Task HandlePollingErrorAsync(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            Console.WriteLine(arg2.ToString());
            return Task.CompletedTask;
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update == null)
            {
                return;
            }

            string messageText = string.Empty;
            long chatId = -1;
            #region Check message type
            if (update.Type == UpdateType.CallbackQuery)
            {
                messageText = update.CallbackQuery.Data;
                chatId = update.CallbackQuery.Message.Chat.Id;
            }
            else if (update.Type == UpdateType.Message)
            {
                messageText = update.Message.Text;
                chatId = update.Message.Chat.Id;
            }
            #endregion

            if (messageText != MessageType.Start && !IsBoStarted(chatId))
            {
                return;
            }

            if (!string.IsNullOrEmpty(messageText) && chatId > 0)
            {
                Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
                switch (messageText)
                {
                    case MessageType.Start:
                        {
                            if (_startedChats.TryGetValue(chatId, out _))
                            {
                                await bot.SendTextMessageAsync(
                                                    chatId: chatId,
                                                    text: "Бот уже запущен",
                                                    parseMode: ParseMode.Html,
                                                    cancellationToken: _cts.Token);
                                return;
                            }
                            List<InlineKeyboardButton> row1 = new List<InlineKeyboardButton>
                            {
                                InlineKeyboardButton.WithCallbackData("Кроссовки"),
                                InlineKeyboardButton.WithCallbackData("Кофты"),
                                InlineKeyboardButton.WithCallbackData("Брюки")
                            };
                            List<InlineKeyboardButton> row2 = new List<InlineKeyboardButton>
                            {
                                InlineKeyboardButton.WithCallbackData("Хочу этот товар")
                            };
                            List<List<InlineKeyboardButton>> rows = new List<List<InlineKeyboardButton>> { row1, row2 };
                            InlineKeyboardMarkup markup = new(rows);

                            await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: "Бот успешно запущен\nВыберите раздел",
                                                parseMode: ParseMode.Html,
                                                null,
                                                true,
                                                false,
                                                null,
                                                true,
                                                markup,
                                                _cts.Token);

                            _startedChats[chatId] = null;
                        }
                        return;
                    case MessageType.Stop:
                        {
                            if (!_startedChats.TryGetValue(chatId, out _))
                            {
                                await bot.SendTextMessageAsync(
                                                    chatId: chatId,
                                                    text: "Бот уже остановлен",
                                                    parseMode: ParseMode.Html,
                                                    cancellationToken: _cts.Token);
                                return;
                            }
                            ReplyKeyboardRemove replyKeyboardRemove = new();

                            Message message2 = await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: "Бот успешно остановлен",
                                                parseMode: ParseMode.Html,
                                                null,
                                                true,
                                                false,
                                                null,
                                                true,
                                                replyKeyboardRemove,
                                                _cts.Token);

                            _startedChats.TryRemove(chatId, out _);
                        }
                        return;
                    case MessageType.WaitProductByVendorCode:
                        {
                            await bot.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: "Отправьте артикул товара",
                                                parseMode: ParseMode.Html,
                                                cancellationToken: _cts.Token);

                            //if(_waitingForProductChats.TryGetValue(chatId, out WaitingChatInfo waitingChatInfo))
                            //{

                            //}
                            _waitingForProductChats[chatId] = new WaitingChatInfo { Status = WaitingForProductChatStatus.MustSendVenderCode };
                        }
                        return;
                }

                if (_waitingForProductChats.TryGetValue(chatId, out WaitingChatInfo info))
                {
                    if (info.Status == WaitingForProductChatStatus.MustSendVenderCode)
                    {
                        if (string.IsNullOrEmpty(messageText))
                        {
                            return;
                        }

                        messageText = messageText.Trim();
                        WPC.ProductByVendorCode productByVendorCode = ProductByVendorCodeHandler.TestVendorCode(messageText);
                        if (productByVendorCode == null)
                        {
                            await bot.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: "Ошибка",
                                                parseMode: ParseMode.Html,
                                                cancellationToken: _cts.Token);
                            return;
                        }
                        var sizes = productByVendorCode.data?.products.FirstOrDefault(x => x.id.ToString() == messageText.Trim())?.sizes?.ToList();
                        if (sizes?.Count > 0)
                        {
                            List<List<InlineKeyboardButton>> buttons = new List<List<InlineKeyboardButton>>();

                            sizes.ForEach(x => buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData($"{x.name} {x.origName}")}));

                            InlineKeyboardMarkup markup = new(buttons);

                            await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: "Выберите ожидаемый размер",
                                                parseMode: ParseMode.Html,
                                                null,
                                                true,
                                                false,
                                                null,
                                                true,
                                                markup,
                                                _cts.Token);
                            info.WaitingProductList.Add(new WaitingProduct { VenderCode = messageText });
                            info.Status = WaitingForProductChatStatus.MustSendSize;
                            return;
                        }
                    }
                    if (info.Status == WaitingForProductChatStatus.MustSendSize)
                    {
                        foreach (var waitingProduct in info.WaitingProductList.Where(x => !x.IsStarted))
                        {
                            waitingProduct.Size = messageText;
                            _ = Task.Run(() => WaitingForProductByVenderCode(chatId, waitingProduct));
                        }

                        //await bot.SendPhotoAsync(
                        //   chatId: chatId,
                        //   photo: imgUrl,
                        //   caption: $"<b>{product.name} {product.brand} с размером {waitingProduct.Size} появился в наличии</b>\n<i>Ссылка</i>: <a href=\"https://www.wildberries.ru/catalog/{waitingProduct.VenderCode}/detail.aspx?targetUrl=EX\">Посмотреть</a>\n\n<b>{date}</b>",
                        //   parseMode: ParseMode.Html,
                        //   cancellationToken: _cts.Token).Result;

                        await bot.SendTextMessageAsync(
                                            chatId: chatId,
                                            text: "Вам будет выслано сообщение, как только товар появится в наличии",
                                            parseMode: ParseMode.Html,
                                            cancellationToken: _cts.Token);
                    }
                }
            }
        }

        private static bool IsBoStarted(long chatId)
        {
            if (!_startedChats.TryGetValue(chatId, out _))
            {
                bot.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Бот не запущен",
                                    parseMode: ParseMode.Html,
                                    cancellationToken: _cts.Token);
                return false;
            }
            return true;
        }

        private static void WaitingForProductByVenderCode(long chatId, WaitingProduct waitingProduct)
        {
            try
            {
                bool productNotFound = true;
                while (productNotFound)
                {
                    Console.WriteLine($"Запрос на наличие продукта \"{waitingProduct}\" для {chatId}");
                    WPC.ProductByVendorCode productByVendorCode = ProductByVendorCodeHandler.TestVendorCode(waitingProduct.VenderCode);

                    if (productByVendorCode == null)
                    {
                        bot.SendTextMessageAsync(
                                            chatId: chatId,
                                            text: "Ошибка",
                                            parseMode: ParseMode.Html,
                                            cancellationToken: _cts.Token);
                        return;
                    }

                    WPC.Product product = productByVendorCode
                        .data?
                        .products?
                        .FirstOrDefault(
                            x => x.id.ToString() == waitingProduct.VenderCode.Trim());

                    string size = waitingProduct.Size.Substring(0, 2);

                    if (product?.sizes?.Any(x => x.name == size && x.stocks?.Count > 0) ?? false)
                    {
                        
                        string imgUrl = Program.GetProductImgById(new Product { id = product.id });
                        string date = string.Format($"Актуально для {DateTime.Now:yyyy MM dd HH:mm}");
                        Message msg = bot.SendPhotoAsync(
                           chatId: chatId,
                           photo: imgUrl,
                           caption: $"<b>{product.name} {product.brand} с размером {waitingProduct.Size} появился в наличии</b>\n<i>Ссылка</i>: <a href=\"https://www.wildberries.ru/catalog/{waitingProduct.VenderCode}/detail.aspx?targetUrl=EX\">Посмотреть</a>\n\n<b>{date}</b>",
                           parseMode: ParseMode.Html,
                           cancellationToken: _cts.Token).Result;

                        productNotFound = false;
                    }

                    if (productNotFound)
                    {
                        Console.WriteLine($"Продукт \"{product?.name} {product?.brand}\" с размером {waitingProduct.Size} нету в наличии. Дата: {DateTime.Now:yyyy MM dd HH:mm}");
                        TimeSpan sleepTime = TimeSpan.FromMinutes(10);
                        Console.WriteLine($"Ожидание: {sleepTime}");
                        Thread.Sleep(sleepTime);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void SendMsg()
        {
            try
            {
                Console.Write($"Отправка {Product.id} {Product.name} {Product.brand} {Product.SalePrice}:");

                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = { }, // receive all update types
                };

                var chat = new Chat
                {
                    Id = _myId,
                    FirstName = "al-hurr",
                    Username = "alhurr996",
                    Type = ChatType.Private
                };

                string date = string.Format("Актуально для {0:dd/MM/yyyy H:mm:ss zzz}", DateTime.Now);
                Message msg = bot.SendPhotoAsync(
                   chatId: chat,
                   photo: this.ImgUrl,
                   caption: $"<b>Название: {Product.name} {Product.brand}</b>\n<b>Цена со скидкой: {Product.SalePrice}</b>\n<b>Скидка: {Product.sale} %</b>\n<b>Старая цена: {Product.priceU / 100}</b>\n<b>Id: {Product.id}</b>\n<i>Ссылка</i>: <a href=\"https://www.wildberries.ru/catalog/{Product.id}/detail.aspx?targetUrl=GP\">Посмотреть</a>\n\n<b>{date}</b>",
                   parseMode: ParseMode.Html,
                   cancellationToken: _cts.Token).Result;

                Console.WriteLine($" success");
            }
            catch
            {
                throw;
            }
        }

        public static bool SendTextMsg()
        {
            try
            {
                var chat = new Chat
                {
                    Id = _myId,
                    FirstName = "al-hurr",
                    Username = "alhurr996",
                    Type = ChatType.Private
                };

                Message message = bot.SendTextMessageAsync(
                                    chatId: chat,
                                    text: "<b>🚀🚀🚀🚀🚀🚀</b>\n<b>Отправка товаров</b>\n<b>🚀🚀🚀🚀🚀🚀</b>",
                                    parseMode: ParseMode.Html,
                                    cancellationToken: _cts.Token).Result;

                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}
