using System;
using System.Collections.Generic;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace X89Bot.EventHandlers
{
	public class TicketHandler
    {
		[Obsolete]
		public static async void HandleTicketInteractions(ComponentInteractionCreateEventArgs e, string customId)
        {
            DiscordMember user = e.User as DiscordMember;
            DiscordGuild guild = e.Guild;

            var category = guild.GetChannel(1219947750129532929) as DiscordChannel;
            if (category == null || category.Type != ChannelType.Category)
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Fehler beim Erstellen des Tickets: Eine Kategorie für Tickets konnte nicht gefunden werden.").AsEphemeral(true));
                return;
            }

            var overwrites = new List<DiscordOverwriteBuilder>
                {
                    new DiscordOverwriteBuilder().For(guild.EveryoneRole).Deny(Permissions.AccessChannels),
                    new DiscordOverwriteBuilder().For(user).Allow(Permissions.None).Allow(Permissions.AccessChannels),
                };

            DiscordChannel channel = await guild.CreateTextChannelAsync($"{e.User.Username}-Ticket", category, overwrites: overwrites, position: 0);

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(($"Dein neues Ticket ({channel.Mention}) wurde erstellt!")).AsEphemeral(true));

            var closeButton = new DiscordButtonComponent(ButtonStyle.Secondary, "closeTicketButton", "🔒 Ticket schließen");

            await channel.SendMessageAsync($"||{user.Mention}||");

            string ticketDesc = "Hallo";

            switch (e.Id)
            {
                case "ticketSupportDropdown":
                case "ticketSupportButton":
                    ticketDesc = "**Beachte:** Bitte beschreibe dein Problem mit ein paar Worten, " +
                                 "damit wir schnellstmöglich auf dein Ticket reagieren können, um " +
                                 "dein Anliegen schnellstmöglich zu lösen.";
                    break;
                case "ticketUnbanDropdown":
                case "ticketUnbanButton":
                    ticketDesc = "**Beachte:** Bitte beschreibe dein Problem mit ein paar Worten, " +
                                 "damit wir schnellstmöglich auf dein Ticket reagieren können, um " +
                                 "dein Anliegen schnellstmöglich zu lösen.";
                    break;
                case "ticketDonationDropdown":
                case "ticketDonationButton":
                    ticketDesc = "**Beachte:** Bitte beschreibe dein Problem mit ein paar Worten, " +
                                 "damit wir schnellstmöglich auf dein Ticket reagieren können, um " +
                                 "dein Anliegen schnellstmöglich zu lösen.";
                    break;
                case "ticketOwnerDropdown":
                case "ticketOwnerButton":
                    ticketDesc = "**Beachte:** Bitte beschreibe dein Problem mit ein paar Worten, " +
                                 "damit wir schnellstmöglich auf dein Ticket reagieren können, um " +
                                 "dein Anliegen schnellstmöglich zu lösen.";
                    break;
            }

            var ticketMessage = new DiscordMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Orange)
                    .WithTitle("**__Ticketsystem__**")
                    .WithThumbnail(guild.IconUrl)
                    .WithTimestamp(DateTime.UtcNow)
                    .WithDescription("**In Kürze wird sich jemand um dich kümmern!**\n" +
                                 "Sollte dein Anliegen bereits erledigt sein dann drücke auf 🔒 um dein Ticket zu schließen!\n\n" + ticketDesc)
                    )
                    .AddComponents(closeButton);
            await channel.SendMessageAsync(ticketMessage);
        }
    }
}
