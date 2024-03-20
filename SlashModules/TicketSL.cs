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

namespace X89Bot.SlashModules
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
                               [Option("system", "Buttons oder Dropdown")] long systemChoice = 1,
                               [Choice("fortnite", 0)]
                               [Choice("cod", 1)]
                               [Choice("staff", 2)]
                               [Choice("studio", 3)]
                               [Option("Kategorie", "Ticketkategorie")] long categoryChoice = 0)
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
                var options = new List<DiscordSelectComponentOption>();

                switch (categoryChoice)
                {
                    case 0:
                        options = new List<DiscordSelectComponentOption>()
                        {
                            new DiscordSelectComponentOption(
                            "Fortnite",
                            "ticketSupportDropdown",
                            "Application for the Fortnite Team",
                            emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":emoji_74:"))),
                        };
                        break;
                    case 1:
                        options = new List<DiscordSelectComponentOption>()
                        {
                            new DiscordSelectComponentOption(
                            "COD",
                            "ticketCodDropdown",
                            "Application for the COD Team",
                            emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":emoji_79:"))),
                        };
                        break;
                    case 2:
                        options = new List<DiscordSelectComponentOption>()
                        {
                            new DiscordSelectComponentOption(
                            "Staff",
                            "ticketStaffDropdown",
                            "Application for the X89 Staff Team",
                            emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":emoji_92~1:"))),
                        };
                        break;
                    case 3:
                        options = new List<DiscordSelectComponentOption>()
                        {
                            new DiscordSelectComponentOption(
                            "Studio",
                            "ticketStudioDropdown",
                            "Application for the Studio Team",
                            emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":emoji_65:"))),
                        };
                        break;

                }

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
                    .SendAsync(ctx.Guild.GetChannel(1185697806997000314));
            }

            await Task.Delay(TimeSpan.FromSeconds(60));

            await ctx.Channel.DeleteAsync("Ticket geschlossen");
        }

        private async Task<bool> CheckIfChannelIsTicket(InteractionContext ctx)
        { 
            const ulong categoryId = 1219947750129532929;

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
