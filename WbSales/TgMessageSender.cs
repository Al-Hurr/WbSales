using System;
using System.IO;
using System.Threading;
using System.Xml;
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
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };

            var chat = new Chat
            {
                Id = 860507683,
                FirstName = "al-hurr",
                Username = "alhurr996",
                Type = ChatType.Private
            };
            Message msg = bot.SendPhotoAsync(
               chatId: chat,
               photo: this.ImgUrl,
               caption: $"<b>Название: {Product.name} {Product.brand}</b>\n<b>Цена со скидкой: {Product.salePriceU / 100}</b>\n<b>Скидка: {Product.sale} %</b>\n<b>Старая цена: {Product.priceU / 100}</b>\n<b>Id: {Product.id}</b>\n<i>Ссылка</i>: <a href=\"https://www.wildberries.ru/catalog/{Product.id}/detail.aspx?targetUrl=GP\">Посмотреть</a>",
               parseMode: ParseMode.Html,
               cancellationToken: cancellationToken).Result;

            SaveProductInXml(Product);
        }

        private void SaveProductInXml(Product product)
        {
            string path = Program._xmlPath;
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(path);
            XmlElement xRoot = xDoc.DocumentElement;

            //add product
            XmlElement productEl = xDoc.CreateElement(string.Empty, "product", string.Empty);
            xRoot.AppendChild(productEl);
            //add id
            XmlElement id = xDoc.CreateElement(string.Empty, "id", string.Empty);
            XmlText idText = xDoc.CreateTextNode(product.id.ToString());
            id.AppendChild(idText);
            productEl.AppendChild(id);
            //add name
            XmlElement name = xDoc.CreateElement(string.Empty, "name", string.Empty);
            XmlText nameText = xDoc.CreateTextNode(product.name + " " + product.brand);
            name.AppendChild(nameText);
            productEl.AppendChild(name);
            //add price
            XmlElement price = xDoc.CreateElement(string.Empty, "price", string.Empty);
            XmlText priceText = xDoc.CreateTextNode(product.salePriceU.ToString());
            price.AppendChild(priceText);
            productEl.AppendChild(price);
            xDoc.Save(path);
        }
    }
}
