using DSharpPlus.SlashCommands;
using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.EventArgs;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Channels;
using DSharpPlus.CommandsNext.Attributes;

namespace DarkBot.SlashModules
{
	public class CasinoSL : ApplicationCommandModule
	{
		private static Dictionary<ulong, DateTime> lastSpinTimes = new Dictionary<ulong, DateTime>(); // Speichert die letzte Drehzeit für jeden Benutzer
		private static TimeSpan spinCooldown = TimeSpan.FromDays(1); // Cooldown-Zeit zwischen Drehungen

        [SlashCommand("Casino", "Öffne das Casino Menü")]
        [RequireBotPermissions(DSharpPlus.Permissions.Administrator, true)]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator, true)]
        public async Task Casino(InteractionContext ctx)
        {
			await ctx.CreateResponseAsync("https://i.imgur.com/85wY3qf.gif");

            var embedTicketButtons = new DiscordMessageBuilder()
			.AddEmbed(new DiscordEmbedBuilder()
			    .WithColor(DiscordColor.MidnightBlue)
			    .WithTitle("**DarkCasino**")
			    .WithDescription("Klicke auf einen Button, um das Spiel deiner Wahl zu starten.")
			)
			.AddComponents(new DiscordComponent[]
			{
			    new DiscordButtonComponent(ButtonStyle.Secondary, "BtnCasinoBlackjack", "Blackjack"),
			    new DiscordButtonComponent(ButtonStyle.Secondary, "BtnCasinoRoulette", "Roulette"),
			    new DiscordButtonComponent(ButtonStyle.Secondary, "BtnCasinoSlots", "Slots"),
			    new DiscordButtonComponent(ButtonStyle.Secondary, "BtnCasinoGlücksrad", "Glücksrad")
			});

            await ctx.Channel.SendMessageAsync(embedTicketButtons);
        }

        [SlashCommand("glücksrad", "Dreh am Glücksrad!")]
		public async Task Gluecksrad(InteractionContext ctx)
		{
			// Überprüfen, ob der Benutzer bereits gedreht hat und genügend Zeit vergangen ist
			if (lastSpinTimes.TryGetValue(ctx.User.Id, out DateTime lastSpinTime))
			{
				var timeSinceLastSpin = DateTime.Now - lastSpinTime;

				if (timeSinceLastSpin < spinCooldown)
				{
					TimeSpan remainingTime = spinCooldown - timeSinceLastSpin;
					int remainingHours = (int)remainingTime.TotalHours;
					int remainingMinutes = remainingTime.Minutes;
					await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Du kannst das Glücksrad nur alle {spinCooldown.TotalHours} Stunden drehen. Bitte warte noch {remainingHours} Stunden | {remainingMinutes} Minuten."));
				}
			}
			string gif = "https://media1.giphy.com/media/v1.Y2lkPTc5MGI3NjExZjhneWUyMGRhcmJoOHFlZWd0dG9oZWthYjQ0bmY4MGJuNGNxbjF1MSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/70OiJhMBae5j6AFL3f/giphy.gif";

			// Anzeige des Glücksrad-GIFs im Chat
			var embedBuilder = new DiscordEmbedBuilder
			{
				Title = "Glücksrad wird gedreht...",
				ImageUrl = gif
			};
			var embed = embedBuilder.Build();
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));

			int prize = 0;
			// Gewinnchancen und Preise
			int[] prizes = { 0, 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000 };
			double[] chances = { 40, 30, 20, 10, 7, 5, 3, 2, 1, 0.5, 0.3 }; // Prozentuale Gewinnchancen

			// Zufällige Auswahl des Preises basierend auf den Chancen
			double totalWeight = chances.Sum();
			double randomNumber = new Random().NextDouble() * totalWeight;
			double accumulatedWeight = 0;
			for (int i = 0; i < prizes.Length; i++)
			{
				accumulatedWeight += chances[i];
				if (randomNumber <= accumulatedWeight)
				{
					prize = prizes[i];
					break;
				}
			}

			// Nachricht löschen und Ergebnis senden
			if (prize > 0)
			{
				embedBuilder = new DiscordEmbedBuilder
				{
					Title = $"Glückwunsch!",
					Description = $"{ctx.Member.Mention}**, du hast {prize}€ gewonnen**",
					ImageUrl = gif,
					Color = DiscordColor.HotPink
				};
			}
			else
			{
				embedBuilder = new DiscordEmbedBuilder
				{
					Title = $"Schade! ",
					Description = $"{ctx.Member.Mention}**, du hast leider nichts gewonnen** :C",
					ImageUrl = gif,
					Color = DiscordColor.IndianRed
				};
			}
			embed = embedBuilder.Build();
			await Task.Delay(6000);
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));

			lastSpinTimes[ctx.User.Id] = DateTime.Now;
		}

        [SlashCommand("blackjack", "Spiel eine Runde Blackjack!")]
        public async Task Blackjack(InteractionContext ctx, [Option("einsatz", "Wie viel möchtest du setzen?")] double bettingAmount)
        {
			await ctx.CreateResponseAsync("https://i.imgur.com/U6JvSki.gif");

            // Initialisierung des Spiels
            var deck = GenerateDeck();
            var playerHand = new List<int>();
            var dealerHand = new List<int>();

            // Initial deal
            DealCard(deck, playerHand);
            DealCard(deck, dealerHand);

            // Send initial game state
            var initialEmbed = new DiscordEmbedBuilder
            {
                Title = "Blackjack",
                Description = "",
                Color = DiscordColor.Aquamarine,
                Timestamp = DateTime.UtcNow,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = ctx.User.AvatarUrl,
                    Name = ctx.User.Username
                }
            };

            initialEmbed.AddField("Deine Hand:", string.Join(", ", playerHand), true);
            initialEmbed.AddField("Dealer Hand:", $"{dealerHand[0]}", true);

            var buttonRow = new DiscordComponent[]
            {
				new DiscordButtonComponent(ButtonStyle.Primary, "BtnBlackjackHit", "\U0001f7e9Ziehen"),
				new DiscordButtonComponent(ButtonStyle.Success, "BtnBlackjackStand", "\U0001f7e5Halten")
            };

            var EmbedMessage = new DiscordMessageBuilder()
                .AddEmbed(initialEmbed)
                .AddComponents(buttonRow);

            await ctx.Channel.SendMessageAsync(EmbedMessage);


            // Button Handler
            async Task ButtonHandler(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                var interaction = e.Interaction;
                var user = interaction.User;

                // Überprüfen, ob der Benutzer, der den Button gedrückt hat, der Autor der ursprünglichen Nachricht ist
                if (user.Id != interaction.User.Id)
                {
                    await interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent("Du bist nicht berechtigt, auf diese Buttons zu reagieren.").AsEphemeral(true));
                    return;
                }

                // Logik basierend auf dem gedrückten Button
                switch (e.Id)
                {
                    case "BtnBlackjackHit":
						PlayerTurn(deck, playerHand);
                        initialEmbed = new DiscordEmbedBuilder
                        {
                            Title = "Blackjack",
                            Description = "",
                            Color = DiscordColor.Aquamarine,
                            Timestamp = DateTime.UtcNow,
                            Author = new DiscordEmbedBuilder.EmbedAuthor
                            {
                                IconUrl = ctx.User.AvatarUrl,
                                Name = ctx.User.Username
                            }
                        };

                        initialEmbed.AddField("Deine Hand:", string.Join(", ", playerHand), true);
                        initialEmbed.AddField("Dealer Hand:", $"{dealerHand[0]}", true);

                        buttonRow = new DiscordComponent[]
                        {
							new DiscordButtonComponent(ButtonStyle.Primary, "BtnBlackjackHit", "\U0001f7e9Ziehen"),
							new DiscordButtonComponent(ButtonStyle.Success, "BtnBlackjackStand", "\U0001f7e5Halten")
                        };

                        var WebhookMessage = new DiscordWebhookBuilder()
							.AddEmbed(initialEmbed)
							.AddComponents(buttonRow);

                        await ctx.EditResponseAsync(WebhookMessage);



                        break;
                    case "BtnBlackjackStand":
						DealerTurn(deck, dealerHand);
                        break;
                    default:
						await ctx.CreateResponseAsync("Exception thrown at Casino! Please contact any Developer");
                        break;
                }
            }


            // Event Handler hinzufügen
            DiscordClient client = ctx.Client;
            client.ComponentInteractionCreated += ButtonHandler;

            // Event abwarten
            await Task.Delay(TimeSpan.FromMinutes(10));

            // Event Handler entfernen
            client.ComponentInteractionCreated -= ButtonHandler;
        }

        private List<int> GenerateDeck()
		{
			var deck = new List<int>();
			for (int i = 1; i <= 10; i++)
			{
				deck.AddRange(Enumerable.Repeat(i, 4)); // 4 Karten von jeder Wertigkeit
			}
			return deck;
		}

		private void DealCard(List<int> deck, List<int> hand)
		{
			var rand = new Random();
			int index = rand.Next(deck.Count);
			int card = deck[index];
			hand.Add(card);
			deck.RemoveAt(index);
		}

		private bool PlayerTurn(List<int> deck, List<int> playerHand)
		{
			while (true)
			{
				int playerTotal = CalculateTotal(playerHand);
				if (playerTotal >= 21)
				{
					break;
				}

				DealCard(deck, playerHand);
			}

			return CalculateTotal(playerHand) > 21;
		}

		private void DealerTurn(List<int> deck, List<int> dealerHand)
		{
			while (CalculateTotal(dealerHand) < 17)
			{
				DealCard(deck, dealerHand);
			}
		}

		private int CalculateTotal(List<int> hand)
		{
			int total = hand.Sum();
			int numAces = hand.Count(card => card == 1);
			for (int i = 0; i < numAces; i++)
			{
				if (total + 10 <= 21)
				{
					total += 10;
				}
			}
			return total;
		}

		[SlashCommand("poker", "Spiel eine Runde Poker!")]
		public async Task Poker(InteractionContext ctx,
									[Option("Euro", "Wie viel möchtest du setzen?")] double bettingAmount)
		{
			await Task.Delay(1000);
		}

		[SlashCommand("roulette", "Spiel eine Runde Roulette!")]
		public async Task Roulette(InteractionContext ctx,
									[Option("Euro", "Wie viel möchtest du setzen?")] double bettingAmount)
		{
			await Task.Delay(1000);
		}

		[SlashCommand("slots", "Spiel eine Runde Slots!")]
		public async Task Slots(InteractionContext ctx,
									[Option("Euro", "Wie viel möchtest du setzen?")] double bettingAmount)
		{
			await Task.Delay(1000);
		}

		[SlashCommand("zahlenraten", "Spiel eine Runde Zahlenraten!")]
		public async Task Zahlenraten(InteractionContext ctx,
									[Option("Euro", "Wie viel möchtest du setzen?")] double bettingAmount)
		{
			Random r = new Random();
			int dice = r.Next(0, 100);
			int tries = 5;

			for (int i = 0; tries > 0; i++)
			{
				await ctx.Channel.SendMessageAsync("Errate die richtige Zahl zwischen 1 und 100");

				//if (ctx.InteractionAuthor.Id == ctx.Message.Interaction.User.Id)
				//{
				//	string userMessageContent = e.Message.Content;
				//
				//	await e.Channel.SendMessageAsync(userMessageContent);
				//}

				int guess = 50;
				if (guess <= 0 || guess > 100)
				{
					await ctx.Channel.SendMessageAsync("\nDie Zahl muss zwischen 1 und 100 sein\n" +
													   "Versuche es erneut");
					continue;
				}

				if (guess > dice)
				{
					await ctx.Channel.SendMessageAsync("\nDie gesuchte Zahl ist kleiner\n");
					tries--;
				}
				else if (guess < dice)
				{
					await ctx.Channel.SendMessageAsync("\nDie gesuchte Zahl ist größer\n");
					tries--;
				}
				else
					break;
				await ctx.Channel.SendMessageAsync($"Du hast {tries} Versuche übrig\n");
			}
		}
	}
}
