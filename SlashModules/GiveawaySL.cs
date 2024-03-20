using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.SlashCommands;
using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus;
using System.Runtime.Remoting.Messaging;

namespace DarkBot.SlashModules
{
    public class GiveawaySL : ApplicationCommandModule
    {
        [SlashCommand("giveaway", "Starte ein Gewinnspiel")]
        [RequireBotPermissions(DSharpPlus.Permissions.Administrator, true)]
        public async Task Giveaway(InteractionContext ctx,
                           [Option("Preis", "Preis des Gewinnspiels", autocomplete: false)] string giveawayPrize,
                           [Option("Beschreibung", "Beschreibung", autocomplete: false)] string giveawayDescription,
                           [Option("Gewinner", "Anzahl der Gewinner", autocomplete: false)] double amountWinner,
                           [Option("Dauer", "Länge des Gewinnspiels in Minuten", autocomplete: false)] long giveawayTime)
        {
            await ctx.DeferAsync();

            var entryButton = new DiscordButtonComponent(ButtonStyle.Primary, "entryGiveawayButton", "\uD83C\uDF89");

            DateTimeOffset endTime = DateTimeOffset.UtcNow.AddMinutes(giveawayTime);
            int totalEntries = 1;
            string giveawayWinner = ctx.User.Mention;

            var giveawayMessage = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()

                .WithColor(DiscordColor.Rose)
                .WithTitle("**" + giveawayPrize + "** :gift:")
                .WithTimestamp(DateTime.UtcNow)
                .WithDescription(giveawayDescription +
                                $"\n\n" +
                                $":tada:Gewinner: {amountWinner}\n" +
                                $":man_standing:Teilnehmer: **{totalEntries}**\n" +
                                $"\nGewinnspiel Ende: <t:{endTime.ToUnixTimeSeconds()}:R>\n" +
                                $"Gehosted von: {ctx.User.Mention}\n")
                )
            .AddComponents(entryButton);

            var webhookBuilder = new DiscordWebhookBuilder()
                .AddEmbed(giveawayMessage.Embeds[0]) // Fügen Sie das eingebettete Objekt hinzu
                .AddComponents(giveawayMessage.Components); // Fügen Sie die Komponenten hinzu

            await ctx.EditResponseAsync(webhookBuilder);

            await Task.Delay(TimeSpan.FromMinutes(giveawayTime));
            {
                string giveawayResultDescription = $":man_standing: Teilnehmer: {totalEntries}\n" +
                                                   $":tada: Preis: **{giveawayPrize}**\n" +
                                                   $"\n:crown: **Gewinner:** {giveawayWinner}";

                var giveawayResultEmbed = new DiscordEmbedBuilder
                {
                    Title = "Gewinnspiel Ende",
                    Description = giveawayResultDescription,
                    Color = DiscordColor.Green,
                    Timestamp = DateTime.UtcNow
                };

                await ctx.Channel.SendMessageAsync(embed: giveawayResultEmbed);
            }
        }
    }
}