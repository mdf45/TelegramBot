using System;
using System.Timers;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace TelegramBot.Resources
{
    class Logic
    {
        public static string Play(ref Username us, ref Message msg)
        {
            CheckForSubscription(us);

            string text = $"@{ msg.From.Username }";

            string timeForNextGame = $"{(DateTime.UtcNow.AddHours(Program.UTC).AddDays(1).Date - DateTime.UtcNow.AddHours(Program.UTC)).Hours} ч." +
                $" {(DateTime.UtcNow.AddHours(Program.UTC).AddDays(1).Date - DateTime.UtcNow.AddHours(Program.UTC)).Minutes} мин.";

            if (us.CanPlay())
            {
                us.Attempts -= 1;

                var rand = new Random();

                int AdditionForBalance = 0;
                while (AdditionForBalance == 0) AdditionForBalance = rand.Next(-6000, 10000 + 1);

                us.Balance += AdditionForBalance;

                if (AdditionForBalance > 0)
                {
                    text += $", ты получаешь прибыль 💪 в { AdditionForBalance } 💵\n";

                    if (!us.IsStels)
                    {
                        text += $"Теперь твой баланс: { us.Balance } 💰\n";
                    }

                    us.InGame = true;
                }
                else
                {
                    if (us.Balance > 0 && us.InGame)
                    {
                        text += $", тебя ограбили 💩 на { Math.Abs(AdditionForBalance)} 💵\n";

                        if (!us.IsStels)
                        {
                            text += $"Теперь твой баланс: { us.Balance } 💰\n";
                        }
                    }
                    else if (us.Balance <= 0 && us.InGame)
                    {
                        text += $", тебя ограбили до ниточки. 🔫\n" +
                        $"Теперь ты банкрот. ⚠\n";

                        // new game

                        us.Balance = 0;
                        us.InGame = false;
                    }
                    else if (!us.InGame)
                    {
                        text += $", ты облажался. 👎\n" +
                        $"Ты без деняг.\n";

                        us.Balance = 0;
                    }
                }
                if (us.Attempts > 0) text += $"🎮: x{us.Attempts}\n";
                else text += $"Продолжай играть через {timeForNextGame } ⏳\n";
            }
            else
            {
                text += $", у вас нет доступных игр 🚷\n" +
                $"Продолжай играть через { timeForNextGame } ⏳\n";
            }

            return text;
        }

        public static string Hide(ref Username us,ref Message msg)
        {
            string text;

            CheckForSubscription(us);

            if (us.IsStels)
            {
                us.IsStels = false;
                text = $"@{ msg.From.Username }, вы вышли из стелс режима 🔓\n";
            }
            else
            {
                if (us.Balance > COST_FOR_HIDES && !us.Subscribed)
                {
                    us.Balance -= COST_FOR_HIDES;
                    us.SubscriptionTime = DateTime.Now.AddDays(1);
                    us.Subscribed = true;
                    us.IsStels = true;
                    text = $"@{ msg.From.Username }, стелс режим активирован 🔒\n" +
                        $"Вы потратили: {COST_FOR_HIDES} $\n";
                }
                else if (us.Subscribed)
                {
                    us.IsStels = true;
                    text = $"@{ msg.From.Username }, стелс режим активирован 🔒\n" +
                        $"До конца стелса: {(us.SubscriptionTime - DateTime.Now).Hours} ч. {(us.SubscriptionTime - DateTime.Now).Minutes} мин.";
                }
                else
                {
                    text = $"@{ msg.From.Username }, недостаточно средств ⛔\n" +
                        $"Нехватает {COST_FOR_HIDES - us.Balance} $\n";
                }
            }

            return text;
        }
        public static string Top(ref List<Username> usernames, ref Message msg)
        {
            string text = $"@{ msg.From.Username },\n" +
                "Топ самых богатых и уважаемых:\n";

            List<Username> temp = new List<Username>(); Username tmp;

            foreach (var user in usernames)
            {
                if (user.InGame && msg.Chat.Id == user.ChatId && !user.IsStels) temp.Add(user);
            }

            if (temp.Count == 0) 
            {
                text = $"@{ msg.From.Username }, " +
                     $"cписок пуст, кажется у кого-то есть все шансы стать первым 😎"; // Если нет игроков в топе, пишем о том что топ пуст

                return text; 
            }

            for (int i = 0; i < temp.Count - 1; ++i) // сортируем топ
            {
                for (int j = 0; j < temp.Count - i - 1; ++j)
                {
                    if (temp[j + 1].Balance > temp[j].Balance)
                    {
                        tmp = temp[j];
                        temp[j] = temp[j + 1];
                        temp[j + 1] = tmp;
                    }
                }
            }

            for (int i = 0; i < temp.Count; i++) // форматируем топ
            {
                text += $"\t{i + 1}. {temp[i].FirstName} {temp[i].LastName} - {temp[i].Balance} $.\n";
            }

            return text;
        }

        public static string HelpMenu(ref Message msg)
        {
            string text = $"@{ msg.From.Username },\n" +
                $"Бот разработан: © Резинкин Иван - @KLG_Ivan_Rezinkin\n" +
                $"Список команд: \n" +
                $"/play - Играть в игру 💸\n" +
                $"/hide - Спрятать себя из топа (Цена: {COST_FOR_HIDES} $/День)\n" +
                $"/giveaway - Участвовать в розыгрыше 🎲 (Розыгрыш проводится каждую полночь по МСК)\n" +
                $"/top - Посмотреть на самых богатых 💰\n" +
                $"/help или /command - Вызвать это самое меню\n" +
                $"/support или /supp - Прямое обращение к разработчику (Пожелания по дальнейшему развитию, багрепорты или просто похвастаться своим балансом 😏)\n";

            return text;
        }

        public static string Support(ref Message msg)
        {
            string text = $"@{msg.From.Username}, ваше ✉ успешно отправлено!";

            return text;
        }

        public static string SupportForAdmin(ref Message msg)
        {
            string text = $"ChatId: {msg.Chat.Id}, ChatTitle: {msg.Chat.Title}, UserId: {msg.From.Id}, Username: {msg.From.Username}, FirstName: {msg.From.FirstName}, " +
                $"LastName: {msg.From.LastName}\n" +
                $"Writed: \n{StripStartTags(msg.Text, "/supp")}";

            return text;
        }

        public static void AddBalance(ref List<Username> usernames,ref Username us, ref Message msg)
        {
            string[] items = StripStartTags(msg.Text, "/add ").Split(" ");

            if (items.Length == 3)
            {
                try
                {
                    long userId = long.Parse(items[0], System.Globalization.CultureInfo.CurrentCulture);
                    long chatId = long.Parse(items[1], System.Globalization.CultureInfo.CurrentCulture);
                    int addition = int.Parse(items[2], System.Globalization.CultureInfo.CurrentCulture);

                    us = usernames.Find(x => x.TgId == userId && x.ChatId == chatId);
                    if (us != null) us.Balance += addition;

                    if (us.Balance > 0 && !us.InGame) us.InGame = true;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    throw;
                }
            }
        }

        public static void AddAttempts(ref List<Username> usernames, ref Username us, ref Message msg)
        {
            string[] items = StripStartTags(msg.Text, "/att ").Split(" ");

            if (items.Length == 3)
            {
                try
                {
                    long userId = long.Parse(items[0], System.Globalization.CultureInfo.CurrentCulture);
                    long chatId = long.Parse(items[1], System.Globalization.CultureInfo.CurrentCulture);
                    int addition = int.Parse(items[2], System.Globalization.CultureInfo.CurrentCulture);

                    us = usernames.Find(x => x.TgId == userId && x.ChatId == chatId);
                    if (us != null) us.Attempts += addition;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    throw;
                }
            }
        }

        public static void Gadd(ref Message msg)
        {
            string[] items = StripStartTags(msg.Text, "/gadd ").Split(" ");

            if (items.Length == 1)
            {
                try
                {
                    long chatId = long.Parse(items[0], System.Globalization.CultureInfo.CurrentCulture);

                    if (Program.giveaways.Find(x => x.ChatId == chatId) == null)
                    {
                        Program.giveaways.Add(new Giveaway(chatId));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    throw;
                }
            }
        }

        public static void Gdel(ref Message msg)
        {
            string[] items = StripStartTags(msg.Text, "/gdel ").Split(" ");

            if (items.Length == 1)
            {
                try
                {
                    long chatId = long.Parse(items[0], System.Globalization.CultureInfo.CurrentCulture);

                    Giveaway g = Program.giveaways.Find(x => x.ChatId == chatId);

                    if (g != null)
                    {
                        Program.giveaways.Remove(g);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    throw;
                }
            }
        }

        public static void StartGiveaway(ref Message msg)
        {
            string[] items = StripStartTags(msg.Text, "/konkurs ").Split(" ");

            if (items.Length == 1)
            {
                try
                {
                    long chatId = long.Parse(items[0], System.Globalization.CultureInfo.CurrentCulture);

                    var giveaway = Program.giveaways.Find(x => x.ChatId == chatId);

                    if (giveaway != null)
                        if (!giveaway.IsGiveaway)
                            giveaway.StartGiveaway();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    throw;
                }
            }
        }
        
        public static string AdminInfo()
        {
            string text = $"/add [id] [chatId] [value] - добавляет баланс [value] игроку [id]\n" +
                $"/att [id] [chatId] [value] - добавляет доп. игры [value] игроку [id]\n" +
                $"/konkurs [chatId] - запускает конкурс в [chatId] группе\n" +
                $"/gadd [chatId] - добавляет в группу [chatId] конкурс\n" +
                $"/gdel [chatId] - удаляет из группы [chatId] конкурс\n";

            return text;
        }

        // NOT FOR MESSAGE
        static string StripStartTags(string item, string start)
        {
            if (item.Trim().StartsWith(start, StringComparison.Ordinal))
            {
                int lastLocation = item.IndexOf(" ", StringComparison.Ordinal);

                if (lastLocation >= 0)
                {
                    item = item.Substring(lastLocation + 1);

                    item = StripStartTags(item, start);
                }
            }

            return item;
        }

        public static void StartTimer(int interval)
        {
            timerForUploadAttempts = new Timer(interval);
            timerForUploadAttempts.Elapsed += Tick;
            timerForUploadAttempts.AutoReset = true;
            timerForUploadAttempts.Enabled = true;
        }
        private static void Tick(object sender, ElapsedEventArgs e)
        {
            if (e.SignalTime.Minute % 10 == 0 && e.SignalTime.Second == 0) // Каждые 10 минут проверяем пользователей на наличие подписки /hide
            {
                CheckForSubscription();
            }

            if (DateTime.UtcNow.AddHours(Program.UTC).Hour == 0 && !isUpload)
            {
                isUpload = true;

                Upload();
                Giveaway();
            }
            else if (DateTime.UtcNow.AddHours(Program.UTC).Hour == 1 && isUpload) isUpload = false;
        }

        private static void Upload()
        {
            foreach (var us in Program.usernames)
            {
                if (us.Attempts <= 0)
                {
                    us.Attempts = 1;
                }
            }
        }

        private static void Giveaway()
        {
            foreach (var item in Program.giveaways)
            {
                item.StartGiveaway();
            }
        }

        private static void CheckForSubscription()
        {
            foreach (var us in Program.usernames)
            {
                if ((int)Math.Round((us.SubscriptionTime - DateTime.Now).TotalSeconds, MidpointRounding.ToEven) < 0)
                { 
                    us.Subscribed = false;
                    us.IsStels = false;
                }
            }
        }

        private static void CheckForSubscription(Username user)
        {
            if ((int)Math.Round((user.SubscriptionTime - DateTime.Now).TotalSeconds, MidpointRounding.ToEven) < 0)
            {
                user.Subscribed = false;
                user.IsStels = false;
            }
        }

        private static bool isUpload;

        private const int COST_FOR_HIDES = 5000;

        private static Timer timerForUploadAttempts;
    }
}
