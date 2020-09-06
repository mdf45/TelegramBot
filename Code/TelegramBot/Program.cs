using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Newtonsoft.Json;

using TelegramBot.Resources; // Folder "Resources"

namespace TelegramBot
{
    class Program
    {
        static private readonly TelegramBotClient bot = new TelegramBotClient("1312812754:AAG80LUpOomt-Wi51Y21L1Zg0uhUyJHFqLc");

        static List<Username> usernames;

        static void Main()
        {
            bot.OnMessage += Bot_OnMessage;

            var me = bot.GetMeAsync().Result;
            Console.Title = me.Username;

            usernames = new List<Username>();

            bot.StartReceiving();
            Console.ReadLine();
            bot.StopReceiving();
        }

        private static async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            Message msg = e.Message;
            Username us;  // создаём объект пользователя

            if (msg == null) return;
            if (usernames.Find(x => x.username == msg.From.Username) == null) usernames.Add(us = new Username(msg));// add only if username not exist
            else us = usernames.Find(x => x.username == msg.From.Username);  // создаём объект пользователя

            if (msg.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                Console.WriteLine($"id: {msg.From.Id}\tusername: {msg.From.Username}\tName: {msg.From.FirstName}\tLastName: {msg.From.LastName}\n\tWrited: {msg.Text}");

                if (msg.Text == "/dick" || msg.Text == "/dick@ru_ebobot")
                {
                    await bot.SendTextMessageAsync(msg.Chat.Id, Logic.Dick(ref us, ref msg)).ConfigureAwait(true);
                }
                else if (msg.Text == "/top" || msg.Text == "/top@ru_ebobot")
                {
                    await bot.SendTextMessageAsync(msg.Chat.Id, Logic.Top(ref usernames)).ConfigureAwait(true); // выводим топ
                }
            }
        }
    }
}
