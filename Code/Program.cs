using System;
using System.Collections.Generic;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Newtonsoft.Json;
using System.IO;

using TelegramBot.Resources; // Folder "Resources"

namespace TelegramBot
{
    class Program
    {
        private const string BOT_API = null; // UR API

        static public readonly TelegramBotClient bot = new TelegramBotClient(BOT_API);

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
            await System.IO.File.WriteAllTextAsync(FILE_NAME, JsonConvert.SerializeObject(usernames)).ConfigureAwait(true); // Обновляем данные
        }

        public const string EXPLORE_NAME = "Datas";

        public const string FILE_NAME = EXPLORE_NAME + "/Data.json";
        public const string GIVEAWAYS_FILE_NAME = EXPLORE_NAME + "/Giveaways.json";

        public const long ADMIN_ID = 0; // UR ADMIN ID
        public const long SUPPORT_CHAT_ID = 0; // UR SUPPORT_CHAT_ID

        public const short UTC = 6;

        static void Main()
        {
            bot.OnMessage += Bot_OnMessage;

            var me = bot.GetMeAsync().Result;
            Console.Title = me.Username;

            // load Data

            DirectoryInfo dirInfo = new DirectoryInfo(EXPLORE_NAME);

            if (!dirInfo.Exists) dirInfo.Create();

            if (System.IO.File.Exists(FILE_NAME))
                usernames = JsonConvert.DeserializeObject<List<Username>>(System.IO.File.ReadAllText(FILE_NAME));

            if (System.IO.File.Exists(GIVEAWAYS_FILE_NAME))
                giveaways = JsonConvert.DeserializeObject<List<Giveaway>>(System.IO.File.ReadAllText(GIVEAWAYS_FILE_NAME));

            //

            StartTimer(30000);
            Logic.StartTimer(1000);

            bot.StartReceiving();
            Console.ReadLine();
            bot.StopReceiving();
        }

        private static async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            Message msg = e.Message;
            Username us;

            if (msg == null) return;
            if (usernames.Find(x => x.TgId == msg.From.Id && x.ChatId == msg.Chat.Id) == null) usernames.Add(us = new Username(msg));// add only if username not exist
            else us = usernames.Find(x => x.TgId == msg.From.Id && x.ChatId == msg.Chat.Id);  // ищем существующий объект пользователя

            if (msg.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                string logText = $"Id: {msg.From.Id}, ChatId: {msg.Chat.Id}, ChatTitle: {msg.Chat.Title}, Username: {msg.From.Username}, Name: {msg.From.FirstName}, " +
                    $"LastName: {msg.From.LastName}, Writed: \"{msg.Text}\", at {DateTime.UtcNow.AddHours(3)}\n"; // Содержание лога

                System.IO.File.AppendAllText("Datas/msg.log", logText); // Записываем логи

                if (msg.Text.Contains("/play", StringComparison.Ordinal) && msg.Text[0] == '/')
                {
                    await bot.SendTextMessageAsync(msg.Chat.Id, Logic.Play(ref us, ref msg)).ConfigureAwait(true);

                    await System.IO.File.WriteAllTextAsync(FILE_NAME, JsonConvert.SerializeObject(usernames)).ConfigureAwait(true); // Обновляем данные
                }
                else if (msg.Text.Contains("/hide", StringComparison.Ordinal) && msg.Text[0] == '/')
                {
                    await bot.SendTextMessageAsync(msg.Chat.Id, Logic.Hide(ref us, ref msg)).ConfigureAwait(true);

                    await System.IO.File.WriteAllTextAsync(FILE_NAME, JsonConvert.SerializeObject(usernames)).ConfigureAwait(true); // Обновляем данные
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
                    Logic.AddBalance(ref usernames, ref us, ref msg);

                    await System.IO.File.WriteAllTextAsync(FILE_NAME, JsonConvert.SerializeObject(usernames)).ConfigureAwait(true); // Обновляем данные
                }
                else if (msg.Text.Contains("/att", StringComparison.Ordinal) && msg.Text[0] == '/' && msg.Chat.Id == ADMIN_ID) //
                {
                    Logic.AddAttempts(ref usernames, ref us, ref msg);

                    await System.IO.File.WriteAllTextAsync(FILE_NAME, JsonConvert.SerializeObject(usernames)).ConfigureAwait(true); // Обновляем данные
                }
                else if (msg.Text.Contains("/konkurs", StringComparison.Ordinal) && msg.Text[0] == '/' && msg.Chat.Id == ADMIN_ID) //
                {
                    Logic.StartGiveaway(ref msg);
                }
                else if (msg.Text.Contains("/gadd", StringComparison.Ordinal) && msg.Text[0] == '/' && msg.Chat.Id == ADMIN_ID) //
                {
                    Logic.Gadd(ref msg);

                    await bot.SendTextMessageAsync(msg.Chat.Id, "Группа успешно добавлена").ConfigureAwait(true);

                    await System.IO.File.WriteAllTextAsync(GIVEAWAYS_FILE_NAME, JsonConvert.SerializeObject(giveaways)).ConfigureAwait(true); // Обновляем данные giveaways
                }
                else if (msg.Text.Contains("/gdel", StringComparison.Ordinal) && msg.Text[0] == '/' && msg.Chat.Id == ADMIN_ID) //
                {
                    Logic.Gdel(ref msg);

                    await bot.SendTextMessageAsync(msg.Chat.Id, "Группа успешно удаленна").ConfigureAwait(true);

                    await System.IO.File.WriteAllTextAsync(GIVEAWAYS_FILE_NAME, JsonConvert.SerializeObject(giveaways)).ConfigureAwait(true); // Обновляем данные giveaways
                }
                else if (msg.Text.Contains("/adminhelp", StringComparison.Ordinal) && msg.Text[0] == '/' && msg.Chat.Id == ADMIN_ID) //
                {
                    await bot.SendTextMessageAsync(msg.Chat.Id, Logic.AdminInfo()).ConfigureAwait(true);
                }
                else if (msg.Text.Contains(Giveaway.Command, StringComparison.Ordinal))
                {
                    for (int i = 0; i < giveaways.Count; i++)
                    {
                        if (msg.Chat.Id == giveaways[i].ChatId)
                        {
                            giveaways[i].MessagesList.Add(await bot.SendTextMessageAsync(giveaways[i].ChatId, giveaways[i].NewPlayer(ref us, ref msg)).ConfigureAwait(true));
                        }
                    }
                }
            }
        }
    }
}
