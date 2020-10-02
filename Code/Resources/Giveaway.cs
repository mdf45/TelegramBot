using System;
using System.Collections.Generic;
using System.Timers;
using Telegram.Bot.Types;

namespace TelegramBot.Resources
{
    class Giveaway : IDisposable
    {
        public Giveaway(long chatId)
        {
            ChatId = chatId;
            maxGifts = (int)Math.Ceiling(maxPlayers / 5.0);
        }
        public string GiveawayEventAndText() // Начало игры
        {
            timeForGame = DateTime.Now.AddMinutes(10);

            //
            players = new List<Username>();
            playerInGame = 0;
            IsGiveaway = true;
            //

            SetATimer();

            string text = $"Начался розыгрыш дополнительных игр! 🎰\n" +
                $"Максимум победителей: {maxGifts}\n" +
                $"Для участия введите {Command}\n" +
                $"Игроков:\n" +
                $"{FillPlayerBar(ref playerInGame, ref maxPlayers)}\n" +
                $"Время регистрации: {(int) Math.Round((timeForGame - DateTime.Now).TotalMinutes, MidpointRounding.ToEven)} мин.\n" +
                $"Если до начала розыгрыша набёрётся {maxPlayers} чел. то розыгрыш начнётся автоматически!";

            return text;
        }

        public async void StartGiveaway()
        {
            MessagesList.Add(await Program.bot.SendTextMessageAsync(ChatId, GiveawayEventAndText()).ConfigureAwait(false));
        }

        private static string FillPlayerBar(ref int pl, ref int max)
        {
            string text = "",
                fillSymbol = "🌕",
                emptySymbol = "🌑";

            for (int i = 0; i < max; i++)
            {
                if (i < pl) text += fillSymbol;
                else text += emptySymbol;
            }

            return text;
        }
        private void SetATimer()
        {
            aTimer = new Timer(1000);
            aTimer.Elapsed += WaitingForGame;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private async void WaitingForGame(object source, ElapsedEventArgs e)
        {
            if (playerInGame == maxPlayers && aTimer.Enabled && !IsMaxPlayersEnd)
            {
                IsMaxPlayersEnd = true;
                timeForGame = e.SignalTime.AddSeconds(4);
            }
            else if ((int)Math.Round((timeForGame - e.SignalTime).TotalSeconds, MidpointRounding.ToEven) <= 0 && aTimer.Enabled)
            {
                timeForGame = DateTime.Now.AddMinutes(10);

                await Program.bot.SendTextMessageAsync(ChatId, EndGame()).ConfigureAwait(true);

                DelMsgList(MessagesList);

                aTimer.Stop();
                aTimer.Dispose();
                aTimer.Enabled = false;

                MessagesList = new List<Message>();
            }
            else if ((int)Math.Round((timeForGame - e.SignalTime).TotalSeconds, MidpointRounding.ToEven) <= 3 &&
                (int)Math.Round((timeForGame - e.SignalTime).TotalSeconds, MidpointRounding.ToEven) > 0)
            {
                MessagesList.Add(await Program.bot.SendTextMessageAsync(ChatId, $"{(int)Math.Round((timeForGame - e.SignalTime).TotalSeconds, MidpointRounding.ToEven)}...").ConfigureAwait(true)); // 3... 2... 1...
            }
            else if ((int) Math.Round((timeForGame - e.SignalTime).TotalSeconds, MidpointRounding.ToEven) == 60 + 1)
            {
                MessagesList.Add(await Program.bot.SendTextMessageAsync(ChatId, $"До начала розыгрыша {(int)Math.Round((timeForGame - e.SignalTime).TotalMinutes, MidpointRounding.ToEven)} мин...").ConfigureAwait(true)); // Уведомление о том что осталась 1 минута
            }
        }
        public string EndGame()
        {
            string text, winnersText = "";

            if (playerInGame > 0)
            {
                gifts = (int)Math.Ceiling(playerInGame / 5.0);

                int winner;

                List<int> winners = new List<int>(gifts);

                for (int i = 0; i < gifts; ++i)
                {
                    winners.Add(-1);

                    winner = rand.Next(0, playerInGame);

                    while (winners.Contains(winner)) winner = rand.Next(0, playerInGame);

                    winners[i] = winner;
                }

                double x = 1, y = playerInGame;

                for (int i = 0; i < gifts; i++)
                {
                    winnersText += $"@{players[winners[i]].Username_} выйграл(а) +{gifts - i} 🎮\n";
                }

                text = $"Поздравляем!\n" +
                        $"Прямо на ваших глазах:\n" +
                        $"{winnersText}" + 
                        $"Шанс выйгрыша: {(int)(x / y * 100)}%\n" +
                        $"Ожидайте следующий розыгрыш!\n";

                for (int i = 0; i < gifts; i++)
                {
                    var user = Program.usernames.Find(x => x.Id == players[winners[i]].Id);

                    user.Attempts += gifts - i;
                }
            }
            else
            {
                text = "Ну и ладно, раз никому не надо - себе оставлю ☝\n" +
                        "Ожидаем следующий розыгрыш\n";
            }

            //
            IsGiveaway = false;
            IsMaxPlayersEnd = false;
            playerInGame = 0;

            foreach (var item in players)
                item.InGiveaway = false;

            //

            return text;
        }

        public string NewPlayer(ref Username us, ref Message msg) // Регистрация новых игроков
        {
            string text = $"@{msg.From.Username}";

            if (IsGiveaway)
            {
                long userId = msg.From.Id;
                long chatId = msg.Chat.Id;

                bool AlreadyInGame = false;

                if (players.Find(x => x.ChatId == chatId && x.TgId == userId) != null)
                    AlreadyInGame = players.Find(x => x.ChatId == chatId && x.TgId == userId).InGiveaway;

                if (AlreadyInGame) return $"{text}, вы уже зарегистрированы ❌\n" +
                        $"Игроков:\n" +
                        $"{FillPlayerBar(ref playerInGame, ref maxPlayers)}\n" +
                        $"Время до начала розыгрыша: {(timeForGame - DateTime.Now).Minutes} мин. {(timeForGame - DateTime.Now).Seconds} сек.";
                else
                {
                    if (playerInGame < maxPlayers)
                    {
                        players.Add(us);
                        players[players.Count - 1].InGiveaway = true;

                        playerInGame = players.FindAll(x => x.InGiveaway && x.ChatId == chatId).Count;

                        return $"{text}, вы успешно зарегистрировались! ✔\n" +
                            $"Игроков:\n" +
                            $"{FillPlayerBar(ref playerInGame, ref maxPlayers)}\n" +
                            $"Время до начала розыгрыша: {(timeForGame - DateTime.Now).Minutes} мин. {(timeForGame - DateTime.Now).Seconds} сек.";
                    }
                    else
                    {
                        return $"{text}, в игре уже максимальное количество игроков! ❌\n" +
                            $"Время до начала розыгрыша: {(timeForGame - DateTime.Now).Minutes} мин. {(timeForGame - DateTime.Now).Seconds} сек.";
                    }
                }
            }
            else
            {
                return $"{text}, сейчас нет активного розыгрыша❗\n" +
                    $"Розыгрыш проводится каждый день в {(new TimeSpan(24,0,0).Add(new TimeSpan(Program.UTC * -1 + 3,0,0))).Hours} по МСК!";
            }
        }

        private async static void DelMsg(Message msg)
        {
            if (msg != null) await Program.bot.DeleteMessageAsync(msg.Chat.Id, msg.MessageId).ConfigureAwait(true);
        }

        private async static void DelMsgList(List<Message> msg)
        {
            for (int i = 0; i < msg.Count; i++)
            {
                if (msg[i] != null) await Program.bot.DeleteMessageAsync(msg[i].Chat.Id, msg[i].MessageId).ConfigureAwait(true);
            }
        }

        public long ChatId { get; set; }
        public bool IsGiveaway { get; set; }

        private bool IsMaxPlayersEnd;

        public static string Command { get; } = "/giveaway";

        private int playerInGame,
            maxPlayers = 15,
            gifts;

        private readonly int maxGifts;

        public Timer aTimer;
        public List<Message> MessagesList = new List<Message>();

        private DateTime timeForGame;
        private List<Username> players = new List<Username>();

        private readonly Random rand = new Random();

        public void Dispose()
        {
            aTimer.Dispose();

            throw new NotImplementedException();
        }
    }
}
