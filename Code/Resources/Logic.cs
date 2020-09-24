using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace TelegramBot.Resources
{
    class Logic
    {
        public static string Dick(ref Username us, ref Message msg)
        {
            string text = $"@{ msg.From.Username }";

            string timeForNextGame = $"{(DateTime.UtcNow.AddHours(3).AddDays(1).Date - DateTime.UtcNow.AddHours(3)).Hours} ч." +
                $" {(DateTime.UtcNow.AddHours(3).AddDays(1).Date - DateTime.UtcNow.AddHours(3)).Minutes} мин.";

            if (us.canPlay())
            {
                us.lastGame = DateTime.UtcNow.AddHours(3);

                var rand = new Random();

                int Additionforpenis = 0;
                while (Additionforpenis == 0) Additionforpenis = rand.Next(-6, 11);

                us.PenisLength += Additionforpenis;

                if (Additionforpenis > 0)
                {
                    text += $", твой писюн вырос на { Additionforpenis } см. 😮\n" +
                    $"Теперь его длина: { us.PenisLength } см. 😳\n" +
                    $"Продолжай играть через {timeForNextGame } 😏";

                    us.inGame = true;
                }
                else
                {
                    if (us.PenisLength > 0 && us.inGame)
                    {
                        text += $", твой писюн укоротили на { Math.Abs(Additionforpenis)} см. 🔪\n" +
                        $"Теперь его длина: { us.PenisLength } см. 😳\n" +
                        $"Продолжай играть через { timeForNextGame } 😏";
                    }
                    else if (us.PenisLength <= 0 && us.inGame)
                    {
                        text += $", твой писюн покидает наш мир. 😧\n" +
                        $"Теперь ты без писюна. 😔\n" +
                        $"Продолжай играть через { timeForNextGame } 😢";

                        // new game

                        us.PenisLength = 0;
                        us.inGame = false;
                    }
                    else if (!us.inGame)
                    {
                        text += $", неудачная попытка. 😕\n" +
                        $"Ты без писюна. 😔\n" +
                        $"Продолжай играть через { timeForNextGame } 😇";

                        us.PenisLength = 0;
                    }
                }
            }
            else
            {
                if (!us.isStels)
                {
                    text += $", ты сегодня уже играл/a 😈\n" +
                    $"Продолжай играть через { timeForNextGame } 😴";
                }
                else
                {
                    text += ",";
                }
            }

            if (us.isStels)
            {
                us.isStels = false;

                text += "\nВы вышли из стелс режима ✖";
            }

            return text;
        }

        public static string Undick(ref Username us,ref Message msg)
        {
            string text;
            if (us.isStels)
            {
                text = $"@{ msg.From.Username }, вы уже находитесь в стелс режиме ✔\n" +
                    $"Для того чтобы из него выйти напишите /dick";
            }
            else
            {
                text = $"@{ msg.From.Username }, стелс режим активирован 👻";
                us.isStels = true;
            }

            return text;
        }
        public static string Top(ref List<Username> usernames, ref Message msg)
        {
            string text = $"@{ msg.From.Username },\n" +
                "Топ самых больших писюнов:\n";

            List<Username> temp = new List<Username>(); Username tmp;

            foreach (var user in usernames)
            {
                if (user.inGame && msg.Chat.Id == user.chatId && !user.isStels) temp.Add(user);
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
                    if (temp[j + 1].PenisLength > temp[j].PenisLength)
                    {
                        tmp = temp[j];
                        temp[j] = temp[j + 1];
                        temp[j + 1] = tmp;
                    }
                }
            }

            for (int i = 0; i < temp.Count; i++) // форматируем топ
            {
                text += $"\t{i + 1}. {temp[i].FirstName} {temp[i].LastName} - {temp[i].PenisLength} см.\n";
            }

            return text;
        }

        public static string HelpMenu(ref Message msg)
        {
            string text = $"@{ msg.From.Username },\n" +
                $"Бот разработан: © Резинкин Иван - @KLG_Ivan_Rezinkin\n" +
                $"Список команд: \n" +
                "/dick - Играть в игру 😄\n" +
                "/undick - Спрятать себя из топа 🌚\n" +
                "/giveaway - Участвовать в розыгрыше 🎲\n" +
                "/top - Полюбоваться самыми большими писюнами 🙈\n" +
                "/help или /command - Вызвать это самое меню\n" +
                "/support или /supp - Прямое обращение к разработчику (Пожелания по дальнейшему развитию, багрепорты или просто похвастаться своим писюном 😏)\n";

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

        public static void AddLength(ref List<Username> usernames,ref Username us, ref Message msg)
        {
            string[] items = StripStartTags(msg.Text, "/add ").Split(" ");

            if (items.Length == 3)
            {
                try
                {
                    long userId = long.Parse(items[0], System.Globalization.CultureInfo.CurrentCulture);
                    long chatId = long.Parse(items[1], System.Globalization.CultureInfo.CurrentCulture);
                    int addition = int.Parse(items[2], System.Globalization.CultureInfo.CurrentCulture);

                    us = usernames.Find(x => x.tgId == userId && x.chatId == chatId);
                    if (us != null) us.PenisLength += addition;

                    if (us.PenisLength > 0 && !us.inGame) us.inGame = true;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    throw;
                }
            }
        }

        public static void addg(ref Message msg)
        {
            string[] items = StripStartTags(msg.Text, "/gadd ").Split(" ");

            if (items.Length == 1)
            {
                try
                {
                    long chatId = long.Parse(items[0], System.Globalization.CultureInfo.CurrentCulture);

                    if (Program.giveaways.Find(x => x.chatId == chatId) == null)
                    {
                        Program.giveaways.Add(new Giveaway(chatId));
                        Program.giveaways.Find(x => x.chatId == chatId).StartGiveaway();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    throw;
                }
            }
        }

        public static void delg(ref Message msg)
        {
            string[] items = StripStartTags(msg.Text, "/gdel ").Split(" ");

            if (items.Length == 1)
            {
                try
                {
                    long chatId = long.Parse(items[0], System.Globalization.CultureInfo.CurrentCulture);

                    if (Program.giveaways.Find(x => x.chatId == chatId) != null)
                    {
                        Program.giveaways.Remove(Program.giveaways.Find(x => x.chatId == chatId));
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

            long chatId = long.Parse(items[0], System.Globalization.CultureInfo.CurrentCulture);

            if (items.Length == 1)
            {
                try
                {
                    var giveaway = Program.giveaways.Find(x => x.chatId == chatId);

                    if (giveaway != null)
                        if (!giveaway.isGiveaway)
                            giveaway.StartGiveAway();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    throw;
                }
            }
        }
        
        public static string adminInfo()
        {
            string text = $"/add [id] [chatId] [value] - добавляет размер писюну\n" +
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
    }
}
