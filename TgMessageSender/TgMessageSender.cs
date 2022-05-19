using Telegram.Bot;

namespace TgMessageSender
{
    public class TgMessageSender
    {
        private ITelegramBotClient _bot = new TelegramBotClient("1528966775:AAHu7tqcWVTh6bLxtwKA7Y-__ie4P6KYk5M");

        public Product MyProperty { get; set; }
    }
}
