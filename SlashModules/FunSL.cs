using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.SlashCommands;
using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.Interactivity.Extensions;
using System.Linq;
using System.Collections.Generic;
using DSharpPlus.EventArgs;
using X89Bot.EventHandlers;
using DSharpPlus.CommandsNext;
using System.Text;

namespace X89Bot.SlashModules
{
    public class FunSL : ApplicationCommandModule
    {
        [SlashCommand("pingspam", "Pingt die Person nach beliebiger Anzahl")]
        [RequireBotPermissions(DSharpPlus.Permissions.Administrator, true)]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator, true)]
        public async Task PingSpam(InteractionContext ctx,
                            [Option("user", "User, der gepingt werden soll", autocomplete: false)] DiscordUser user,
                            [Option("anzahl", "Anzahl, wie oft der User gepingt werden soll", autocomplete: false)] double anzahl)
        {
            await ctx.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                                     .WithContent(($"PingSpam an {user.Mention} wird durchgeführt. Dies kann einige Zeit in Anspruch nehmen")).AsEphemeral(true));
            
            for (int i = 0; i < anzahl; i++)
                await ctx.Channel.SendMessageAsync(user.Mention);
        }

        [SlashCommand("virus", "Sende der Person einen Virus")]
        [RequireBotPermissions(DSharpPlus.Permissions.Administrator, true)]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator, true)]
        public async Task Virus(InteractionContext ctx,
            [Option("user", "Ziel")] DiscordUser user)
        {
            await ctx.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                                     .WithContent(("Der Virus wird installiert...")).AsEphemeral(true));

            await ctx.Channel.SendMessageAsync(user.Locale);
        }
        public static async Task ProcessPollVote(ComponentInteractionCreateEventArgs e)
        {
            // Hier wird die Stimme des Benutzers gezählt und verarbeitet
            // Die CustomId des Buttons enthält die Option-Nummer
            string optionText = e.Interaction.Data.CustomId.Replace("pollOption", "");

            if (true) 
            {
                // Die Stimme für diese Option erhöhen
                //voteCounts[optionText]++;

                // Hier können Sie die Stimme des Benutzers für die Option verarbeiten
                await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                                                .WithContent($"Du hast für **{optionText}** abgestimmt!").AsEphemeral(true));
            }
        }

    [SlashCommand("poll", "Starte eine Abstimmung")]
        [RequireBotPermissions(DSharpPlus.Permissions.Administrator, true)]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator, true)]
        public async Task Poll(InteractionContext ctx,
                        [Option("polltitel", "Titel der Abstimmung")] string pollTitle,
                        [Option("pollzeit", "Länge der Abstimmung in Minuten")] long pollTime,
                        [Option("option1", "Option 1")] string option1,
                        [Option("option2", "Option 2")] string option2,
                        [Option("option3", "Option 3")] string option3)
        {
            var interactivity = ctx.Client.GetInteractivity();
            DateTimeOffset endTime = DateTimeOffset.UtcNow.AddMinutes(pollTime);

            var pollOption1Button = new DiscordButtonComponent(ButtonStyle.Danger, "pollOption1Button", option1);
            var pollOption2Button = new DiscordButtonComponent(ButtonStyle.Success, "pollOption2Button", option2);
            var pollOption3Button = new DiscordButtonComponent(ButtonStyle.Primary, "pollOption3Button", option3);

            var pollEmbed = new DiscordEmbedBuilder()
                .WithTitle(pollTitle)
                .WithDescription("**Klicke auf einen Button um abzustimmen**. Du hast 1 Vote!\n\n" +
                                $"Ende der Abstimmung: <t:{endTime.ToUnixTimeSeconds()}:R>")
                .WithColor(DiscordColor.HotPink);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(pollEmbed).AddComponents(pollOption1Button, pollOption2Button, pollOption3Button));

            // Dictionary zum Speichern der Anzahl der Stimmen für jede Option
            var voteCounts = new Dictionary<string, int>
            {
                { "Option 1", 0 },
                { "Option 2", 0 },
                { "Option 3", 0 }
            };

            // Warte bis zum Ende der Abstimmungszeit
            await Task.Delay(TimeSpan.FromMinutes(pollTime));

            int totalVotes = voteCounts.Values.Sum();

            var pollResultEmbed = new DiscordEmbedBuilder
            {
                Title = "Ergebnis der Abstimmung",
                Description = $"```{pollTitle}```\n" +
                              $":tada: **Gewonnen hat {option2}**\n\n" +
                              $"Insgesamte Votes: **{totalVotes}**\n\n" +
                              $"- {voteCounts["Option 1"]} - **{option1}**\n" +
                              $"- {voteCounts["Option 2"]} - **{option2}**\n" +
                              $"- {voteCounts["Option 3"]} - **{option3}**\n",
                Color = DiscordColor.Green
            }.Build();

            // Ergebnis der Abstimmung senden
            await ctx.Channel.SendMessageAsync(embed: pollResultEmbed);
        }


        [SlashCommand("valorant", "Valorant Statistiken")]
        public async Task Valorant(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Hier werden bald deine Valorant Statistiken angezeigt"));

            var embedMessage = new DiscordEmbedBuilder()
            {
                Title = "... coming soon"
            };

            // Riot API: RGAPI-3e72ae2c-69a7-4ab8-8d51-97ce23d5ee43
            await ctx.Channel.SendMessageAsync(embedMessage);
        }

        [SlashCommand("Spritrechner", "Rechne den Sprit aus")]
        public async Task Spritrechner(InteractionContext ctx,
                                       [Option("Kilometer", "Gib die gefahrene Strecke in km ein")] double kilometres,
                                       [Option("Spritverbrauch", "Gib den Durschnittsverbrauch ein")] double avgConsumption,
                                       [Option("Spritpreis", "Gib den Spritpreis pro Liter ein")] double fuelPrice,
                                       [Option("Mitfahrer", "Gib die Anzahl der Mitfahrer ein")] double passenger = 0,
                                       [Option("Rückfahrt", "Soll die Rückfahrt berücksichtigt werden?")] bool returnJourney = false,
                                       [Option("Mautgebühren", "Gib die Mautgebühren ein")] double maut = 0)
        {
            double price = ((kilometres * avgConsumption / 100) * fuelPrice) * (returnJourney ? 2 : 1);
            double totalPassengers = passenger + 1;
            double pricePerPassenger = (price + maut) / totalPassengers;

            var embedMessage = new DiscordEmbedBuilder()
            {
                Title = "**Spritrechner Auswertung**\n",
                Description = $"Kilometer insgesamt: {kilometres}\n" +
                              $"Durchschnittsverbrauch: {avgConsumption} l/100km\n" +
                              $"Spritpreis/l: {fuelPrice:F2}€\n" +
                              $"Anzahl der Mitfahrer: {passenger}\n" +
                              $"Rückfahrt berechnet?: {(returnJourney ? "Ja" : "Nein")}\n" +
                              $"Mautgebühren: {maut}€\n\n" +
                              $"Der **Gesamtpreis** beträgt **{price:F2}** Euro.\n" +
                              $"Der Preis **pro Person** beträgt **{pricePerPassenger:F2}** Euro.",
                Color = DiscordColor.Sienna
            };

            await ctx.CreateResponseAsync(embedMessage);
        }

        [SlashCommand("coinflip", "Wirf eine Münze.")]
        public async Task GetCoinFlip(InteractionContext ctx)
        {
            await DiscordHelper.SendResponseAsync(ctx, $":coin:  **{ctx.User.Username}** hat eine Münze geworfen und bekam {Formatter.Bold(Convert.ToBoolean(new Random().Next(0, 2)) ? "Kopf" : "Zahl")}.").ConfigureAwait(false);
        }

        [SlashCommand("ssp", "Schere, Stein, Papier")]
        [Cooldown(1, 3, CooldownBucketType.Channel)]
        public async Task SSP(InteractionContext ctx,
                             [Choice("Schere", "SCISSORS")]
                             [Choice("Stein", "ROCK")]
                             [Choice("Papier", "PAPER")]
                             [Option("Auswahl", "Schere/Stein/Papier")] string choice)
        {
            string[] choices = new string[3] { "rock", "paper", "scissors" };

            Random rnd = new Random();
            int n = rnd.Next(0, 12);
            if (n > 2)
            { n %= 3; }
            //the resulting win
            string[] resultStr = new string[3] { $"**{ctx.User.Username}** has won"/* 0 */, "It's a **tie**" /* 1 */ , "Haha loser, I won" /* 2 */};
            //emded for the winning result
            var resultEmbed = new DiscordEmbedBuilder { Title = "RPS Results" };
            StringBuilder resultSb = new StringBuilder();

            resultSb.AppendLine($"{ctx.User.Username} chose: **{choice}**");
            resultSb.AppendLine($"I whip out: **{choices[n]}**");
            resultSb.AppendLine(resultStr[SspFight(choice, choices[n])]);

            resultEmbed.Description = resultSb.ToString();


            switch (SspFight(choice, choices[n]))
            {
                case 0:
                    resultEmbed.WithThumbnail(ctx.User.AvatarUrl);
                    resultEmbed.Color = DiscordColor.SapGreen;
                    break;
                case 1:
                    resultEmbed.Color = DiscordColor.Cyan;
                    break;
                case 2:
                    resultEmbed.WithThumbnail("https://cdn.discordapp.com/embed/avatars/0.png");
                    resultEmbed.Color = DiscordColor.DarkRed;
                    break;
            }

            await ctx.Channel.SendMessageAsync(embed: resultEmbed.Build()).ConfigureAwait(false);
        }

        //rps fucntion returns 0 [Player Won] 1[tie] 2 [Player lost]
        private int SspFight(string userChoice, string compChoice) //param: usrChoice of type string 
        {
            userChoice = userChoice.ToUpper();
            compChoice = compChoice.ToUpper();

            if (userChoice == "ROCK" && compChoice == "SCISSORS")
            {
                return 0;
            }
            else if (userChoice == "ROCK" && compChoice == "PAPER")
            {
                return 2;
            }
            else if (userChoice == "PAPER" && compChoice == "ROCK")
            {
                return 0;
            }
            else if (userChoice == "PAPER" && compChoice == "SCISSORS")
            {
                return 2;
            }
            else if (userChoice == "SCISSORS" && compChoice == "ROCK")
            {
                return 2;
            }
            else if (userChoice == "SCISSORS" && compChoice == "PAPER")
            {
                return 0;
            }
            else
            {
                return 1;
            }

        }
    }
}
