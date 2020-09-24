using System;
using Telegram.Bot.Types;

namespace TelegramBot.Resources
{
    class Username
    {
        public Username()
        {
            id = staticID++;

            // GAME

            PenisLength = 0;
            inGame = false;
        }
        public Username(Message msg) : this()
        {
            tgId = msg.From.Id;
            username = msg.From.Username;
            FirstName = msg.From.FirstName;
            LastName = msg.From.LastName;
            chatId = msg.Chat.Id;
        }

        static uint staticID = 1;

        public long chatId { get; set; }
        public long tgId { get; set; }
        public uint id { get; set; }
        public string username { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        // GAME

        public bool inGiveaway { get; set; }
        public bool isStels { get; set; }
        public bool inGame { get; set; }
        public int PenisLength { get; set; }

        public DateTime lastGame { get; set; }

        //

        public bool canPlay()
        {
            return DateTime.UtcNow.AddHours(3).Day != lastGame.Day;
        }
    }
}
