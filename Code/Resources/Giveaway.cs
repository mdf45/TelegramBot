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
            this.chatId = chatId;
        }
        public string GiveawayEventAndText() // Начало игры
        {
            timeForGame = DateTime.Now.AddMinutes(2).AddSeconds(1);

            //
            players = new List<Username>();
            playerInGame = 0;
            isGiveaway = true;
            gift = 0;
            //

            SetATimer();

            string text = $"Начался розыгрыш писюна! 🎰\n" +
                $"Каждый участник добавляет в банк по {bonusPerPlayer} см!\n" +
                $"Для участия введите {command}\n" +
                $"Игроков:\n" +
                $"{fillPlayerBar(ref playerInGame, ref maxPlayers)}\n" +
                $"Время регистрации: {(timeForGame - DateTime.Now).Minutes} мин. {(timeForGame - DateTime.Now).Seconds} сек.\n" +
                $"Если до начала розыгрыша набёрётся {maxPlayers} чел. то розыгрыш начнётся автоматически!";

            return text;
        }

        private static string fillPlayerBar(ref int pl, ref int max)
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
        private void SetGTimer()
        {
            gTimer = new Timer(1000);
            gTimer.Elapsed += WaitingForGiveaway;
            gTimer.AutoReset = true;
            gTimer.Enabled = true;
        }
        public void StartGiveAway()
        {
            if (isGiveaway) return;

            timeForGiveaway = DateTime.Now.AddHours(1); // Просто обновляем время на время игры, во избежание багов 

            isAdminStart = true;

            SetGTimer();
        }
        public void StartGiveaway(int min = 60 + 1, int max = 120 + 1)
        {
            if (isGiveaway) return;

            timeForGiveaway = DateTime.Now.AddMinutes(rand.Next(min, max + 1));

            SetGTimer();
        }

        private static string NotificationText(DateTime t1, DateTime t2)
        {
            string text = $"Готовтесь, через {(t1 - t2).Minutes} минут будет розыгрыш! 🎰🎰🎰";

            return text;
        }

        private async void WaitingForGiveaway(object source, ElapsedEventArgs e)
        {
            if (e.SignalTime >= timeForGiveaway.AddMinutes(-1) && gTimer.Enabled || isAdminStart)
            {
                if (isAdminStart) isAdminStart = false;

                timeForGiveaway = DateTime.Now.AddHours(10); // Просто обновляем время на время игры, во избежание багов 

                await Program.bot.SendTextMessageAsync(chatId, GiveawayEventAndText()).ConfigureAwait(true);

                gTimer.Stop();
                gTimer.Dispose();
                gTimer.Enabled = false;
            }
            else if ((timeForGiveaway - e.SignalTime).Hours <= 0)
            {
                if ((timeForGiveaway - e.SignalTime).Minutes == NotificationsTimes[indexOfNotification])
                {
                    ++indexOfNotification;

                    if (indexOfNotification == NotificationsTimes.Length) indexOfNotification = 0;

                    await Program.bot.SendTextMessageAsync(chatId, NotificationText(timeForGiveaway, e.SignalTime)).ConfigureAwait(true);
                }
            }
        }

        private async void WaitingForGame(object source, ElapsedEventArgs e)
        {
            if (playerInGame == maxPlayers && aTimer.Enabled && !isMaxPlayersEnd)
            {
                isMaxPlayersEnd = true;
                timeForGame = e.SignalTime.AddSeconds(4);
            }
            if (e.SignalTime >= timeForGame && aTimer.Enabled)
            {
                timeForGame = DateTime.Now.AddMinutes(2).AddSeconds(1);

                await Program.bot.SendTextMessageAsync(chatId, EndGame()).ConfigureAwait(true);

                aTimer.Stop();
                aTimer.Dispose();
                aTimer.Enabled = false;
            }
            if ((timeForGame.Minute - e.SignalTime.Minute) == 0 && (timeForGame.Second - e.SignalTime.Second) <= 3 && (timeForGame.Second - e.SignalTime.Second) > 0)
            {
                await Program.bot.SendTextMessageAsync(chatId, $"{timeForGame.Second - e.SignalTime.Second}...").ConfigureAwait(true); // 3... 2... 1...
            }
            else if ((timeForGame.Minute - e.SignalTime.Minute) == 1 && (timeForGame.Second - e.SignalTime.Second) == 0)
            {
                await Program.bot.SendTextMessageAsync(chatId, $"До начала розыгрыша {(timeForGame.Minute - e.SignalTime.Minute)} мин...").ConfigureAwait(true); // Уведомление о том что осталась 1 минута
            }
        }
        public string EndGame()
        {
            string text;

            if (playerInGame > 0)
            {
                int winner = rand.Next(0, playerInGame);

                double x = 1, y = playerInGame;

                text = $"ОПА НА! 🙈🙉🙊\n" +
                        $"Прямо на ваших глазах @{players[winner].username} выйграл/а {gift} см. с шансом {(int)(x / y * 100)}%\n" +
                        $"Ожидайте следующий розыгрыш!\n";

                Username user = Program.usernames.Find(x => x.id == players[winner].id);

                user.inGame = true;
                user.PenisLength += gift;
            }
            else
            {
                text = "Ну и ладно, раз никому не надо - себе оставлю ☝\n" +
                        "Ожидаем следующий розыгрыш\n";
            }

            //
            isGiveaway = false;
            isMaxPlayersEnd = false;
            indexOfNotification = 0;
            playerInGame = 0;

            foreach (var item in players)
                item.inGiveaway = false;

            StartGiveaway();

            //

            return text;
        }

        public string newPlayer(ref Username us, ref Message msg) // Регистрация новых игроков
        {
            string text = $"@{msg.From.Username}";

            if (isGiveaway)
            {
                long userId = msg.From.Id;
                long chatId = msg.Chat.Id;

                bool AlreadyInGame = false;

                if (players.Find(x => x.chatId == chatId && x.tgId == userId) != null)
                    AlreadyInGame = players.Find(x => x.chatId == chatId && x.tgId == userId).inGiveaway;

                if (AlreadyInGame) return $"{text}, вы уже зарегистрированы ❌\n" +
                        $"Сейчас разыгрывается: {gift} см.\n" +
                        $"Игроков:\n" +
                        $"{fillPlayerBar(ref playerInGame, ref maxPlayers)}\n" +
                        $"Время до начала розыгрыша: {(timeForGame - DateTime.Now).Minutes} мин. {(timeForGame - DateTime.Now).Seconds} сек.";
                else
                {
                    if (playerInGame < maxPlayers)
                    {
                        players.Add(us);
                        gift += bonusPerPlayer;
                        players[players.Count - 1].inGiveaway = true;

                        playerInGame = players.FindAll(x => x.inGiveaway && x.chatId == chatId).Count;

                        return $"{text}, вы успешно зарегистрировались! ✔\n" +
                            $"Сейчас разыгрывается: {gift} см.\n" +
                            $"Игроков:\n" +
                            $"{fillPlayerBar(ref playerInGame, ref maxPlayers)}\n" +
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
                    $"Следующий розыгрыш через " +
                    $"{((timeForGiveaway - DateTime.Now).Hours > 0 ? $"{(timeForGiveaway - DateTime.Now).Hours} ч. " : $"{(timeForGiveaway - DateTime.Now).Minutes} мин.")}";
            }
        }

        private static uint[] NotificationsTimes = { 30, 10, 5 }; // На каких минутах будет уведомление о том что "скоро розыгрыш", в порядке убывания

        public long chatId { get; set; }
        public bool isAdminStart { get; set; }
        public bool isGiveaway { get; set; }
        private bool isMaxPlayersEnd;

        public static string command { get; } = "/giveaway";

        private int playerInGame,
            maxPlayers = 5,
            bonusPerPlayer = 3,
            indexOfNotification,
            gift;

        public Timer aTimer, gTimer;
        private Random rand = new Random();
        private DateTime timeForGame, timeForGiveaway;

        private List<Username> players = new List<Username>();

        public void Dispose()
        {
            aTimer.Dispose(); gTimer.Dispose();

            throw new NotImplementedException();
        }
    }
}
