using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

using TelegramBot.Resources; // Folder "Resources"

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
                while (Additionforpenis == 0) Additionforpenis = rand.Next(-5, 11);

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
                        text += $", твой писюн укоротили на { Math.Abs(Additionforpenis)}см. 🔪\n" +
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
                    }
                }
            }
            else
            {
                text += $", ты сегодня уже играл/a 😈\n" +
                    $"Продолжай играть через { timeForNextGame } 😴";
            }

            return text;
        }
        public static string Top(ref List<Username> usernames)
        {
            string text = "Топ самых больших писюнов:\n";

            List<Username> temp = new List<Username>();
            for (int i = 0; i < usernames.Count; i++)
            {
                temp.Add(usernames[i]);
            }
            int tmp;

            for (int i = 0; i < temp.Count; i++)
            {
                if (!temp[i].inGame) temp.RemoveAt(i);
            }

            for (int i = 0; i < temp.Count - 1; i++) // сортируем топ
            {
                for (int j = 0; j < temp.Count; j++)
                {
                    if (temp[j + 1].PenisLength > temp[j].PenisLength)
                    {
                        tmp = temp[j].PenisLength;
                        temp[j].PenisLength = temp[j + 1].PenisLength;
                        temp[j + 1].PenisLength = tmp;
                    }
                }
            }

            for (int i = 0; i < temp.Count; i++) // форматируем топ
            {
                text += $"\t{i + 1}. {temp[i].FirstName} {temp[i].LastName} - {temp[i].PenisLength} см.\n";
            }

            if (temp.Count == 0) text = "Список пуст, кажеться у кого-то есть все шансы стать первым 😎"; // Если нет игроков в топе, пишем о том что топ пуст

            return text;
        }
    }
}
