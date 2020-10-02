using System;
using Telegram.Bot.Types;

namespace TelegramBot.Resources
{
    class Username
    {
        public Username()
        {
            Id = staticID++;

            // GAME

            Balance = 0;
            Attempts = 1;
            InGame = false;
        }
        public Username(Message msg) : this()
        {
            TgId = msg.From.Id;
            Username_ = msg.From.Username;
            FirstName = msg.From.FirstName;
            LastName = msg.From.LastName;
            ChatId = msg.Chat.Id;
        }

        static uint staticID = 1;

        public long ChatId { get; set; }
        public long TgId { get; set; }
        public uint Id { get; set; }
        public string Username_ { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        // GAME

        public bool InGiveaway { get; set; }
        public bool IsStels { get; set; }

        public bool Subscribed { get; set; }
        public bool InGame { get; set; }
        public int Balance { get; set; }

        public int Attempts { get; set; }

        public DateTime SubscriptionTime;

        //

        public bool CanPlay()
        {
            return Attempts > 0;
        }
    }
}
