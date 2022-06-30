using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace WbSales
{
    public class TgMessageSender
    {
        private ITelegramBotClient bot = new TelegramBotClient("1528966775:AAHu7tqcWVTh6bLxtwKA7Y-__ie4P6KYk5M");
        private readonly long _myId = 860507683;

        public Product Product { get; set; }
        public string ImgUrl { get; set; }

        public TgMessageSender(Product product, string imgUrl)
        {
            if (product != null)
                this.Product = product;
            if (!string.IsNullOrEmpty(imgUrl))
                this.ImgUrl = imgUrl;
        }

        public void SendMsg()
        {
            try
            {
                Console.Write($"Отправка {Product.id} {Product.name} {Product.brand} {Product.SalePrice()}:");
                var cts = new CancellationTokenSource(); 
                var cancellationToken = cts.Token;
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
                   caption: $"<b>Название: {Product.name} {Product.brand}</b>\n<b>Цена со скидкой: {Product.SalePrice()}</b>\n<b>Скидка: {Product.sale} %</b>\n<b>Старая цена: {Product.priceU / 100}</b>\n<b>Id: {Product.id}</b>\n<i>Ссылка</i>: <a href=\"https://www.wildberries.ru/catalog/{Product.id}/detail.aspx?targetUrl=GP\">Посмотреть</a>\n\n<b>{date}</b>",
                   parseMode: ParseMode.Html,
                   cancellationToken: cancellationToken).Result;


                Console.WriteLine($" success");

                //SaveProductInXml(Product);
            }
            catch
            {
                throw;
            }
        }
    }
}
