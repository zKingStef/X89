using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DarkBot.SlashModules
{
    [SlashCommandGroup("ticket", "Slash Commands für das Ticketsystem.")]
    public class TicketSL : ApplicationCommandModule
    {
        [SlashCommand("system", "Erschaffe das Ticketsystem mit Buttons oder Dropdown Menu :)")]
        [RequireBotPermissions(DSharpPlus.Permissions.Administrator, true)]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator, true)]
        public async Task Ticketsystem(InteractionContext ctx,
                                [Choice("Button", 0)]
                                [Choice("Dropdown Menu", 1)]
                                [Option("system", "Buttons oder Dropdown")] long systemChoice = 1)
        {
            if (systemChoice == 0)
            {
                var embedTicketButtons = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()

                .WithColor(DiscordColor.Goldenrod)
                .WithTitle("**Ticketsystem**")
                .WithDescription("Klicke auf einen Button, um ein Ticket der jeweiligen Kategorie zu erstellen")
                )
                .AddComponents(new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Success, "ticketSupportButton", "Support"),
                    new DiscordButtonComponent(ButtonStyle.Danger, "ticketUnbanButton", "Entbannung"),
                    new DiscordButtonComponent(ButtonStyle.Primary, "ticketDonationButton", "Spenden"),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "ticketOwnerButton", "Inhaber")
                });

                var response = new DiscordInteractionResponseBuilder().AddEmbed(embedTicketButtons.Embeds[0]).AddComponents(embedTicketButtons.Components);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
            }

            else if (systemChoice == 1)
            {
                var options = new List<DiscordSelectComponentOption>()
                {
                    new DiscordSelectComponentOption(
                        "Support",
                        "ticketSupportDropdown",
                        "Ticket für allgemeine Probleme, Wünsche und sonstiges!",
                        emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":envelope:"))),

                    new DiscordSelectComponentOption(
                        "Entbannung",
                        "ticketUnbanDropdown",
                        "Hier kannst du über eine Entbannung diskutieren!",
                        emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":tickets:"))),

                    new DiscordSelectComponentOption(
                        "Spenden",
                        "ticketDonationDropdown",
                        "Ticket für Donations!",
                        emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":moneybag:"))),

                    new DiscordSelectComponentOption(
                        "Inhaber",
                        "ticketOwnerDropdown",
                        "Dieses Ticket geht speziell an den Inhaber des Servers!",
                        emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":factory_worker:"))),
                };

                var ticketDropdown = new DiscordSelectComponent("ticketDropdown", "Wähle eine passende Kategorie aus", options, false, 0, 1);

                var embedTicketDropdown = new DiscordMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()

                    .WithColor(DiscordColor.Goldenrod)
                    .WithTitle("**Ticketsystem**")
                    .WithDescription("Öffne das Dropdown Menü und wähle eine passende Kategorie aus, um ein Ticket deiner Wahl zu erstellen")
                    )
                    .AddComponents(ticketDropdown);

                var response = new DiscordInteractionResponseBuilder().AddEmbed(embedTicketDropdown.Embeds[0]).AddComponents(embedTicketDropdown.Components);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
            }
        }

        [SlashCommand("add", "Füge einen User zum Ticket hinzu")]
        [RequireBotPermissions(DSharpPlus.Permissions.Administrator, true)]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator, true)]
        public async Task Add(InteractionContext ctx,
                             [Option("User", "Der User, der zum Ticket hinzugefügt werden soll")] DiscordUser user)
        {
            await CheckIfChannelIsTicket(ctx);

            var embedMessage = new DiscordEmbedBuilder()
            {
                Title = "User hinzugefügt!",
                Description = $"{user.Mention} wurde von {ctx.User.Mention} zum Ticket {ctx.Channel.Mention} hinzugefügt!\n",
                Timestamp = DateTime.UtcNow
            };
            await ctx.CreateResponseAsync(embedMessage);

            await ctx.Channel.AddOverwriteAsync((DiscordMember)user, Permissions.AccessChannels);
        }

        [SlashCommand("remove", "Entferne einen User vom Ticket")]
        [RequireBotPermissions(DSharpPlus.Permissions.Administrator, true)]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator, true)]
        public async Task Remove(InteractionContext ctx,
                             [Option("User", "Der User, der von diesem Ticket entfernt werden soll")] DiscordUser user)
        {
            await CheckIfChannelIsTicket(ctx);

            var embedMessage = new DiscordEmbedBuilder()
            {
                Title = "User entfernt!",
                Description = $"{user.Mention} wurde von {ctx.User.Mention} aus diesem Ticket entfernt!\n",
                Timestamp = DateTime.UtcNow
            };
            await ctx.CreateResponseAsync(embedMessage);

            await ctx.Channel.AddOverwriteAsync((DiscordMember)user, Permissions.None);
        }

        [SlashCommand("rename", "Ändere den Namen vom Ticket")]
        [RequireBotPermissions(DSharpPlus.Permissions.Administrator, true)]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator, true)]
        public async Task Rename(InteractionContext ctx,
                             [Option("Name", "Gib dem Ticket einen neuen Namen")] string newChannelName)
        {
            await CheckIfChannelIsTicket(ctx);

            var oldChannelName = ctx.Channel.Mention;

            var embedMessage = new DiscordEmbedBuilder()
            {
                Title = "Ticket umbenannt!",
                Description = $"Das Ticket {ctx.Channel.Mention} wurde von {ctx.User.Mention} umbenannt!\n\n" +
                              $"Das Ticket heißt nun ```{newChannelName}```",
                Timestamp = DateTime.UtcNow
            };

            await ctx.CreateResponseAsync(embedMessage);

            await ctx.Channel.ModifyAsync(properties => properties.Name = newChannelName);
        }

        [SlashCommand("close", "Schließe ein Ticket")]
        [RequireBotPermissions(DSharpPlus.Permissions.Administrator, true)]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator, true)]
        public async Task Close(InteractionContext ctx)
        {
            await CheckIfChannelIsTicket(ctx);

            var embedMessage = new DiscordEmbedBuilder()
            {
                Title = "🔒 Ticket geschlossen!",
                Description = $"Das Ticket wurde von {ctx.User.Mention} geschlossen!\n" +
                              $"Der Channel wird in <t:{DateTimeOffset.UtcNow.AddSeconds(60).ToUnixTimeSeconds()}:R> gelöscht. ",
                Timestamp = DateTime.UtcNow
            };
            await ctx.CreateResponseAsync(embedMessage);

            var messages = await ctx.Channel.GetMessagesAsync(999);

            var content = new StringBuilder();
            content.AppendLine($"Transcript für Ticket {ctx.Channel.Name}:");
            foreach (var message in messages)
            {
                content.AppendLine($"{message.Author.Username} ({message.Author.Id}) - {message.Content}");
            }

            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content.ToString())))
            {
                var msg = await new DiscordMessageBuilder()
                    .AddFile("transript.txt", memoryStream)
                    .SendAsync(ctx.Guild.GetChannel(978669571483500574));
            }

            await Task.Delay(TimeSpan.FromSeconds(60));

            await ctx.Channel.DeleteAsync("Ticket geschlossen");
        }

        private async Task<bool> CheckIfChannelIsTicket(InteractionContext ctx)
        { 
            const ulong categoryId = 1207086767623381092;

            if (ctx.Channel.Parent.Id != categoryId || ctx.Channel.Parent == null)
            {
                await ctx.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("´´Fehler!´´ **Dieser Befehl kann nur in einem Ticket verwendet werden**").AsEphemeral(true));

                return true;
            }

            return false;
        }
    }
}
