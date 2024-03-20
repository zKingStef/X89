using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X89Bot.EventHandlers
{
	public class CasinoHandler
	{
        public static async void ShowGameRules(ComponentInteractionCreateEventArgs e, string gameType)
        {
            var embedMessage = new DiscordEmbedBuilder() { };

            switch (gameType)
            {
                case "glücksrad":
                    embedMessage = new DiscordEmbedBuilder()
                    {
                        Title = "**Glücksrad Regeln**",
                        Description = $"test123\n\n",
                        Timestamp = DateTime.UtcNow
                    };
                    break;

                case "blackjack":
                    embedMessage = new DiscordEmbedBuilder()
                    {
                        Title = "**Blackjack Spielregeln**",
                        Description = $"test123\n\n",
                        Timestamp = DateTime.UtcNow
                    };
                    break;

                case "poker":
                    embedMessage = new DiscordEmbedBuilder()
                    {
                        Title = "**Poker Spielregeln**",
                        Description = $"test123\n\n",
                        Timestamp = DateTime.UtcNow
                    };
                    break;

                case "roulette":
                    embedMessage = new DiscordEmbedBuilder()
                    {
                        Title = "**Roulette Spielregeln**",
                        Description = $"test123\n\n",
                        Timestamp = DateTime.UtcNow
                    };
                    break;

                case "slots":
                    embedMessage = new DiscordEmbedBuilder()
                    {
                        Title = "**Slots Spielregeln**",
                        Description = "1. Du brauchst 3 gleiche Zahlen um zu gewinnen" +
                                      "2. Bei 4 gleichen Zahlen erhältst du einen Jackpot" +
                                      "Der Gewinn ist das 30x fache von deiner Wettsumme",
                        Timestamp = DateTime.UtcNow
                    };
                    break;

                case "zahlenraten":
                    embedMessage = new DiscordEmbedBuilder()
                    {
                        Title = "**Zahlenraten Spielregeln**",
                        Description = "1. Du hast 5 Versuche, um eine zufällige Zahl zwischen 1 und 100 zu erraten.\n" +
                                      "2. Der Gewinn ist das 10x fache von deinem Wetteinsatz €€€.",
                        Timestamp = DateTime.UtcNow
                    };
                    break;
            }

        await e.Channel.SendMessageAsync(embedMessage);
        }
    }
}
