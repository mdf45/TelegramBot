using System;
using System.Collections.Generic;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Newtonsoft.Json;

using TelegramBot.Resources; // Folder "Resources"

namespace TelegramBot
{
    class Program
    {
        static public readonly TelegramBotClient bot = new TelegramBotClient("UR BOT API-TOKEN");

        public static List<Username> usernames = new List<Username>();
        public static List<Giveaway> giveaways = new List<Giveaway>();

        private static Timer timerForSaveData;

        static void StartTimer(long interval)
        {
            timerForSaveData = new Timer(interval);
            timerForSaveData.Elapsed += SaveDataTimer;
            timerForSaveData.AutoReset = true;
            timerForSaveData.Enabled = true;
        }

        private static async void SaveDataTimer(object source, ElapsedEventArgs e)
        {
            await System.IO.File.WriteAllTextAsync(fileName, JsonConvert.SerializeObject(usernames)).ConfigureAwait(true); // Обновляем данные
        }
        //

        public const string fileName = "Datas/Data.json";
        public const string GiveawaysFileName = "Datas/Giveaways.json";

        public const long ADMIN_ID = 550948742;
        public const long SUPPORT_CHAT_ID = -446723469;

        /* 
         ChatId's: 
             -1001292003146 Флудилка КЛГ 
             -488031692 Game 
             -406748550 Bot Test
        */

        static void Main()
        {
            bot.OnMessage += Bot_OnMessage;

            var me = bot.GetMeAsync().Result;
            Console.Title = me.Username;

            if (System.IO.File.Exists(fileName))
                usernames = JsonConvert.DeserializeObject<List<Username>>(System.IO.File.ReadAllText(fileName));

            if (System.IO.File.Exists(GiveawaysFileName))
                giveaways = JsonConvert.DeserializeObject<List<Giveaway>>(System.IO.File.ReadAllText(GiveawaysFileName));

            foreach (var giveaway in giveaways) // Заполнение чатов
            {
                giveaway.StartGiveaway();
            }

            StartTimer(30000);

            bot.StartReceiving();
            Console.ReadLine();
            bot.StopReceiving();
        }

        private static async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            Message msg = e.Message;
            Username us;  // создаём объект пользователя

            if (msg == null) return;
            if (usernames.Find(x => x.tgId == msg.From.Id && x.chatId == msg.Chat.Id) == null) usernames.Add(us = new Username(msg));// add only if username not exist
            else us = usernames.Find(x => x.tgId == msg.From.Id && x.chatId == msg.Chat.Id);  // ищем существующий объект пользователя

            if (msg.Type == Telegram.Bot.Types.Enums.MessageType.Text/* && msg.Chat.Id == CHATS_ID[1] || msg.Chat.Id == ADMIN_ID*/)
            {
                string logText = $"Id: {msg.From.Id}, ChatId: {msg.Chat.Id}, ChatTitle: {msg.Chat.Title}, Username: {msg.From.Username}, Name: {msg.From.FirstName}, " +
                    $"LastName: {msg.From.LastName}, Writed: \"{msg.Text}\", at {DateTime.UtcNow.AddHours(3)}\n"; // Содержание лога

                System.IO.File.AppendAllText("Datas/msg.log", logText); // Записываем логи

                if (msg.Text.Contains("/dick", StringComparison.Ordinal) && msg.Text[0] == '/')
                {
                    await bot.SendTextMessageAsync(msg.Chat.Id, Logic.Dick(ref us, ref msg)).ConfigureAwait(true);

                    await System.IO.File.WriteAllTextAsync(fileName, JsonConvert.SerializeObject(usernames)).ConfigureAwait(true); // Обновляем данные
                }
                else if (msg.Text.Contains("/undick", StringComparison.Ordinal) && msg.Text[0] == '/')
                {
                    await bot.SendTextMessageAsync(msg.Chat.Id, Logic.Undick(ref us, ref msg)).ConfigureAwait(true);

                    await System.IO.File.WriteAllTextAsync(fileName, JsonConvert.SerializeObject(usernames)).ConfigureAwait(true); // Обновляем данные
                }
                else if (msg.Text.Contains("/top", StringComparison.Ordinal) && msg.Text[0] == '/')
                {
                    await bot.SendTextMessageAsync(msg.Chat.Id, Logic.Top(ref usernames, ref msg)).ConfigureAwait(true); // выводим топ
                }
                else if (msg.Text.Contains("/help", StringComparison.Ordinal) || msg.Text.Contains("/commands", StringComparison.Ordinal) && msg.Text[0] == '/')
                {
                    await bot.SendTextMessageAsync(msg.Chat.Id, Logic.HelpMenu(ref msg)).ConfigureAwait(true);
                }
                else if (msg.Text.Contains("/supp", StringComparison.Ordinal) && msg.Text[0] == '/')
                {
                    await bot.SendTextMessageAsync(msg.Chat.Id, Logic.Support(ref msg)).ConfigureAwait(true);

                    await bot.SendTextMessageAsync(SUPPORT_CHAT_ID, Logic.SupportForAdmin(ref msg)).ConfigureAwait(true); // Пишем сообщение разработчику
                }
                else if (msg.Text.Contains("/add", StringComparison.Ordinal) && msg.Text[0] == '/' && msg.Chat.Id == ADMIN_ID) //
                {
                    Logic.AddLength(ref usernames, ref us, ref msg);

                    await System.IO.File.WriteAllTextAsync(fileName, JsonConvert.SerializeObject(usernames)).ConfigureAwait(true); // Обновляем данные
                }
                else if (msg.Text.Contains("/konkurs", StringComparison.Ordinal) && msg.Text[0] == '/' && msg.Chat.Id == ADMIN_ID) //
                {
                    Logic.StartGiveaway(ref msg);
                }
                else if (msg.Text.Contains("/gadd", StringComparison.Ordinal) && msg.Text[0] == '/' && msg.Chat.Id == ADMIN_ID) //
                {
                    Logic.addg(ref msg);

                    await bot.SendTextMessageAsync(msg.Chat.Id, "Группа успешно добавлена").ConfigureAwait(true);

                    await System.IO.File.WriteAllTextAsync(GiveawaysFileName, JsonConvert.SerializeObject(giveaways)).ConfigureAwait(true); // Обновляем данные giveaways
                }
                else if (msg.Text.Contains("/gdel", StringComparison.Ordinal) && msg.Text[0] == '/' && msg.Chat.Id == ADMIN_ID) //
                {
                    Logic.delg(ref msg);

                    await bot.SendTextMessageAsync(msg.Chat.Id, "Группа успешно удаленна").ConfigureAwait(true);

                    await System.IO.File.WriteAllTextAsync(GiveawaysFileName, JsonConvert.SerializeObject(giveaways)).ConfigureAwait(true); // Обновляем данные giveaways
                }
                else if (msg.Text.Contains("/adminhelp", StringComparison.Ordinal) && msg.Text[0] == '/' && msg.Chat.Id == ADMIN_ID) //
                {
                    await bot.SendTextMessageAsync(msg.Chat.Id, Logic.adminInfo()).ConfigureAwait(true);
                }
                else if (msg.Text.Contains(Giveaway.command, StringComparison.Ordinal))
                {
                    for (int i = 0; i < giveaways.Count; i++)
                    {
                        if (msg.Chat.Id == giveaways[i].chatId)
                        {
                            await bot.SendTextMessageAsync(giveaways[i].chatId, giveaways[i].newPlayer(ref us, ref msg)).ConfigureAwait(true);
                        }
                    }
                }
            }
        }
    }
}
