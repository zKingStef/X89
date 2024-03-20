using X89Bot.docs;
using X89Bot.EventHandlers;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace X89Bot.SlashModules
{
    public class ModSL : ApplicationCommandModule
    {
        [SlashCommand("clear", "Lösche Nachrichten aus dem Chat")]
        public async Task Clear(InteractionContext ctx, 
                               [Option("Anzahl", "Anzahl der Nachrichten die gelöscht werden sollen")] double delNumber)
        {
            if (!DiscordHelper.CheckPermissions(ctx, Permissions.ManageMessages))
            {
                await DiscordHelper.SendAsEphemeral(ctx, ":x: Du hast nicht die nötigen Rechte, um Nachrichten zu löschen");
                return;
            }

            await ctx.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                                     .WithContent(($"Die letzten {delNumber} Nachrichten wurden erfolgreich gelöscht!")).AsEphemeral(true));

            var messages = await ctx.Channel.GetMessagesAsync((int)(delNumber));

            var content = new StringBuilder();
            content.AppendLine("Geloeschte Nachrichten:");
            foreach (var message in messages)
            {
                content.AppendLine($"{message.Author.Username} ({message.Author.Id}) - {message.Content}");
            }

            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content.ToString())))
            {
                var msg = await new DiscordMessageBuilder()
                    .AddFile("deleted_messages.txt", memoryStream)
                    .SendAsync(ctx.Guild.GetChannel(1143516841357086870));
            }

            await DiscordHelper.SendNotification(ctx,
                                                 $"Nachrichten mit /clear gelöscht!",
                                                 $"Anzahl Nachrichten: **{delNumber}**\n\n" +
                                                 $"Channel: {ctx.Channel.Mention} - {ctx.Channel.Name}\n" +
                                                 $"Verantwortlicher Moderator: {ctx.User.Mention}",
                                                 DiscordColor.Yellow,
                                                 1143516841357086870);

            await ctx.Channel.DeleteMessagesAsync(messages);
        }

        [SlashCommand("ban", "Banne einen User vom Discord")]
        [RequireBotPermissions(DSharpPlus.Permissions.Administrator, true)]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator, true)]
        public async Task Ban(InteractionContext ctx,
                          [Option("User", "Der User der gebannt werden soll")] DiscordUser user,
                          [Option("Grund", "Der Grund für den Bann")] string reason,
                          [Option("AnzahlTage", "Lösche alle Nachrichten, die innerhalb der letzten ... Tage vom User geschrieben wurden")] double deleteDays = 0)
        {
            await ctx.DeferAsync();
            await DiscordHelper.ExecuteBanAction(ctx, (DiscordMember)user, (int)deleteDays, reason);
        }

        [SlashCommand("banid", "Banne einen User vom Discord")]
        [RequireBotPermissions(DSharpPlus.Permissions.Administrator, true)]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator, true)]
        public async Task BanId(InteractionContext ctx,
                          [Option("UserID", "Die ID des Users, der gebannt werden soll")] string userId,
                          [Option("Grund", "Der Grund für den Bann")] string reason,
                          [Option("AnzahlTage", "Lösche alle Nachrichten, die innerhalb der letzten ... Tage vom User geschrieben wurden")] double deleteDays = 0)
        {
            await ctx.DeferAsync();
            await DiscordHelper.ExecuteBanAction(ctx, userId, (int)deleteDays, reason);
        }

        [SlashCommand("unban", "Entbanne einen User vom Discord oder zeige gebannte Benutzer an, wenn keine ID angegeben ist.")]
        public async Task UnbanOrListBans(InteractionContext ctx,
                                   [Option("UserId", "Die ID des Users, der entbannt werden soll. Lasse leer, um gebannte Benutzer anzuzeigen.")] string userId = null,
                                   [Option("Grund", "Der Grund für den Unban")] string reason = "Kein Grund angegeben")
        {
            await ctx.DeferAsync();
            await DiscordHelper.ExecuteUnbanAction(ctx, userId, reason);
            
        }

        [SlashCommand("banlist", "Zeige alle gebannten Spieler")]
        [RequireBotPermissions(DSharpPlus.Permissions.Administrator, true)]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator, true)]
        public async Task Banlist(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            await DiscordHelper.ShowBanList(ctx);
        }

        [SlashCommand("mute", "Setze einen Timeout auf einen Benutzer.")]
        public async Task Timeout(InteractionContext ctx,
                          [Option("User", "Der User, der einen Timeout erhalten soll")] DiscordUser user,
                          [Option("Duration", "Dauer des Timeouts in Minuten")] long durationInMinutes,
                          [Option("Reason", "Grund für den Timeout")] string reason = "Kein Grund angegeben")
        {
            await ctx.DeferAsync();
            await DiscordHelper.ExecuteTimeoutAction(ctx, (DiscordMember)user, durationInMinutes, reason);
        }

        [SlashCommand("unmute", "Hebt den Timeout eines Benutzers auf.")]
        public async Task RemoveTimeout(InteractionContext ctx,
                                [Option("User", "Der User, dessen Timeout aufgehoben werden soll")] DiscordUser user,
                                [Option("Reason", "Grund für die Aufhebung des Timeouts")] string reason = "Kein Grund angegeben")
        {
            await ctx.DeferAsync();
            await DiscordHelper.ExecuteRemoveTimeoutAction(ctx, (DiscordMember)user, reason);
        }

        [SlashCommand("lock", "Sperrt temporär den Schreibzugriff für alle User.")]
        public async Task Lock(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            await ctx.Channel.AddOverwriteAsync(ctx.Guild.EveryoneRole, Permissions.None, Permissions.SendMessages);

            await DiscordHelper.SendNotification(ctx, "Channel gesperrt!", "Bitte warte bis der Channel von einem Admin freigeschalten wird.", DiscordColor.HotPink, 0);
        }

        [SlashCommand("unlock", "Gibt einen Channel wieder frei.")]
        public async Task Unlock(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            await ctx.Channel.AddOverwriteAsync(ctx.Guild.EveryoneRole, Permissions.SendMessages, Permissions.None);

            await DiscordHelper.SendNotification(ctx, "Channel freigeschalten!", "Der Channel ist jetzt wieder offen. Danke für eure Geduld!", DiscordColor.HotPink, 0);
        }

        // FUNCTIONS
    }
}

