using DSharpPlus.Entities;
using DSharpPlus;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using DSharpPlus.CommandsNext;
using X89Bot.Common;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System.IO;
using System.Net.Http;

namespace X89Bot.EventHandlers
{
    public static class DiscordHelper
    {
        // Methode zum Senden von Benachrichtigungen
        public static async Task SendNotification(InteractionContext ctx,
                                                  string title,
                                                  string description,
                                                  DiscordColor color,
                                                  ulong channelId)
        {
            var message = new DiscordEmbedBuilder
            {
                Title = title,
                Description = description,
                Color = color,
                Timestamp = DateTime.UtcNow,
            };

            if (channelId == 0)
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(message));
            else if (channelId == 1)
                await ctx.Channel.SendMessageAsync(message);
            else
                await ctx.Guild.GetChannel(channelId).SendMessageAsync(message);
        }

        public static async Task SendAsEphemeral(InteractionContext ctx,
                                                  string text)
        {
            await ctx.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                                     .WithContent((text)).AsEphemeral(true));
        }

        public static async Task SendLogMessage(DiscordClient client, ulong channelId, AuditLogActionType alaType, string title, string description, DiscordColor color)
        {
            var guild = await client.GetGuildAsync(1185696801936916530);
            var auditLogs = await guild.GetAuditLogsAsync(1, null, alaType);
            var lastLog = auditLogs.FirstOrDefault();
            var responsible = lastLog?.UserResponsible;

            var channel = await client.GetChannelAsync(channelId);

            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = title,
                Description = description + responsible.Mention,
                Color = color,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = responsible?.AvatarUrl,
                    Name = responsible?.Username
                }
            };

            var embed = embedBuilder.Build();
            await channel.SendMessageAsync(embed: embed);
        }

        public static async Task SendLogMessage(DiscordClient client, ulong channelId, string title, string description, DiscordColor color)
        {
            var channel = await client.GetChannelAsync(channelId);

            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = title,
                Description = description,
                Color = color,
            };

            var embed = embedBuilder.Build();
            await channel.SendMessageAsync(embed: embed);
        }


        public static async Task SendDirectMessage(InteractionContext ctx, DiscordMember user, string title, string description, DiscordColor color)
        {
            var message = new DiscordEmbedBuilder
            {
                Title = title,
                Description = "**Server:** " + ctx.Guild.Name +
                              "\n**Grund:** " + description +
                              "\n\n**Verantwortlicher Moderator:** " + ctx.Member.Mention,
                Color = color,
                Timestamp = DateTime.UtcNow
            };

            var channel = await user.CreateDmChannelAsync();
            await channel.SendMessageAsync(message);
        }

        // Methode zur Berechtigungsprüfung
        public static bool CheckPermissions(InteractionContext ctx, Permissions requiredPermissions)
        {
            return ctx.Member.Permissions.HasPermission(requiredPermissions);
        }

        // Methode zur Fehlerbehandlung
        public static async Task HandleException(InteractionContext ctx, Exception e)
        {
            string errorMessage = $"Ein Fehler ist aufgetreten: {e.Message}";
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(errorMessage));
        }

        // Methode für Ban-Aktionen
        public static async Task ExecuteBanAction(InteractionContext ctx, DiscordMember user, int deleteDays, string reason)
        {
            if (!CheckPermissions(ctx, Permissions.BanMembers))
            {
                await SendNotification(ctx, "Keinen Zugriff", "Du hast nicht die nötigen Rechte, um diesen Benutzer zu bannen.", DiscordColor.Red, 0);
                return;
            }

            try
            {
                var member = await ctx.Guild.GetMemberAsync(user.Id);

                await SendDirectMessage(ctx, user, "Du wurdest gebannt!", $"{reason}", DiscordColor.Red);

                await ctx.Guild.BanMemberAsync(member, deleteDays, reason + $" - **MOD:** {ctx.User.Mention} - **TIME:** {ctx.User.CreationTimestamp}");

                // BotChannel
                await SendNotification(ctx, $"{member.DisplayName} wurde vom Server gebannt",
                                            $"**User:** {member.Mention}\n" +
                                            $"**Verantwortlicher Moderator:** {ctx.User.Mention}",
                                            DiscordColor.IndianRed,
                                            0);

                // LogChannel
                await SendNotification(ctx, $"User durch SlashCommand gebannt!",
                                                $"**Discord Name:** ```{member.Username}``` - {member.Mention}\n" +
                                                $"**Verantwortlicher Moderator:** {ctx.User.Mention}",
                                                DiscordColor.Grayple,
                                                1143518462111658034);
            }
            catch (Exception e)
            {
                await HandleException(ctx, e);
            }
        }

        public static async Task ExecuteBanAction(InteractionContext ctx, string userId, int deleteDays, string reason)
        {
            if (!CheckPermissions(ctx, Permissions.BanMembers))
            {
                await SendNotification(ctx, "Keinen Zugriff", "Du hast nicht die nötigen Rechte, um diesen Benutzer zu bannen.", DiscordColor.Red, 0);
                return;
            }

            try
            {
                if (ulong.TryParse(userId, out ulong userBanId))
                {
                    var member = await ctx.Guild.GetMemberAsync(userBanId);

                    await ctx.Guild.BanMemberAsync(userBanId, deleteDays, reason + $"MOD: {ctx.User.Mention} TIME: {ctx.User.CreationTimestamp}");

                    // BotChannel
                    await SendNotification(ctx, $"User wurde vom Server gebannt",
                                                $"User: **{member.Mention}**",
                                                DiscordColor.IndianRed,
                                                0);

                    // LogChannel
                    await SendNotification(ctx, $"User durch SlashCommand gebannt!",
                                                $"**User:** {member.Mention} - {member.Username}\n" +
                                                $"**Verantwortlicher Moderator:** {ctx.User.Mention}",
                                                DiscordColor.Grayple,
                                                1143518462111658034);
                }
            }
            catch (Exception e)
            {
                await HandleException(ctx, e);
            }
        }

        // Methode für Unban-Aktionen
        public static async Task ExecuteUnbanAction(InteractionContext ctx, string userId, string reason)
        {
            if (!ctx.Member.Permissions.HasPermission(Permissions.BanMembers))
            {
                await SendNotification(ctx, "Keinen Zugriff", "Du hast nicht die nötigen Rechte, um diesen Benutzer zu entbannen.", DiscordColor.Red, 0);
                return;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                await ShowBanList(ctx);
            }
            else
            {
                // Benutzer-ID wurde angegeben, versuche zu entbannen
                if (ulong.TryParse(userId, out ulong userBanId))
                {
                    var bans = await ctx.Guild.GetBansAsync();
                    var ban = bans.FirstOrDefault(b => b.User.Id == userBanId);
                    if (ban == null)
                    {
                        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Benutzer nicht in der Banliste gefunden."));
                        return;
                    }

                    await ctx.Guild.UnbanMemberAsync(userBanId);
                    await SendNotification(ctx, $"{ban.User.Username} wurde vom Server entbannt",
                                                $"User: **{ban.User.Mention}**\n" +
                                                $"Verantwortlicher Moderator: {ctx.User.Mention}",
                                                DiscordColor.SpringGreen,
                                                0);

                    // LogChannel
                    await SendNotification(ctx, $"User durch SlashCommand entbannt!",
                                                $"Discord Name: **{ban.User.Username} - {ban.User.Mention}**\n" +
                                                $"Verantwortlicher Moderator: {ctx.User.Mention}",
                                                DiscordColor.Grayple,
                                                1143518613777678336);
                }
                else
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Ungültige User-ID."));
                }
            }
        }

        // Methode für Ban-Liste
        public static async Task ShowBanList(InteractionContext ctx)
        {
            if (!CheckPermissions(ctx, Permissions.BanMembers))
            {
                await SendNotification(ctx, "Keinen Zugriff", "Du hast nicht die nötigen Rechte, um die Ban-Liste aufzurufen.", DiscordColor.Red, 0);
                return;
            }

            try
            {
                var bans = await ctx.Guild.GetBansAsync();
                if (bans.Count == 0)
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Es gibt aktuell keine gebannten Benutzer."));
                    return;
                }

                var banListStringBuilder = new StringBuilder("**Gebannte Benutzer:**\n");
                foreach (var ban in bans)
                {
                    banListStringBuilder.AppendLine($"{ban.User.Username}#{ban.User.Discriminator} - **ID:** {ban.User.Id} - **Grund:** {ban.Reason}");
                }

                // Nachrichtenaufteilung bei Bedarf
                if (banListStringBuilder.Length >= 2000)
                {
                    // Implementiere eine Aufteilungslogik oder sende die Liste in Teilen
                }
                else
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(banListStringBuilder.ToString()));
                }
            }
            catch (Exception e)
            {
                await HandleException(ctx, e);
            }
        }

        // Methode für Timeout
        public static async Task ExecuteTimeoutAction(InteractionContext ctx, DiscordMember user, long durationInMinutes, string reason)
        {
            // Berechtigungsprüfung
            if (!CheckPermissions(ctx, Permissions.ModerateMembers))
            {
                await SendNotification(ctx, "Keinen Zugriff", "Du hast nicht die nötigen Rechte, um diesen Befehl auszuführen.", DiscordColor.Red, 0);
                return;
            }

            try
            {
                var member = await ctx.Guild.GetMemberAsync(user.Id);
                var timeoutUntil = DateTime.UtcNow.AddMinutes(durationInMinutes);

                await SendDirectMessage(ctx, user, "Du wurdest stumm geschalten!", $"{reason}", DiscordColor.DarkRed);

                await member.TimeoutAsync(timeoutUntil, reason);

                await SendNotification(ctx, $"{member.DisplayName} hat einen Timeout erhalten",
                                            $"User: **{member.Mention}**\n" +
                                            $"Verantwortlicher Moderator: {ctx.User.Mention}",
                                            DiscordColor.IndianRed,
                                            0);

                await SendNotification(ctx, $"{member.DisplayName} hat einen Timeout erhalten, der in {durationInMinutes} Minuten abläuft.",
                                            $"Discord Name: **{member.Mention}**\n" +
                                            $"Discord ID: {member.Id}\n\n" +
                                            $"Grund: **{reason}**\n" +
                                            $"Verantwortlicher Moderator: {ctx.User.Mention}",
                                            DiscordColor.IndianRed,
                                            1143512137914921101);
            }
            catch (NotFoundException)
            {
                await SendNotification(ctx, "Nicht gefunden", "Der Benutzer wurde auf dem Server nicht gefunden.", DiscordColor.Red, 0);
            }
            catch (Exception e)
            {
                await HandleException(ctx, e);
            }
        }

        public static async Task ExecuteRemoveTimeoutAction(InteractionContext ctx, DiscordMember user, string reason)
        {

            // Berechtigungsprüfung
            if (!CheckPermissions(ctx, Permissions.ModerateMembers))
            {
                await SendNotification(ctx, "Keinen Zugriff", "Du hast nicht die nötigen Rechte, um diesen Befehl auszuführen.", DiscordColor.Red, 0);
                return;
            }

            try
            {
                var member = await ctx.Guild.GetMemberAsync(user.Id);

                await SendDirectMessage(ctx, user, "Deine Timeout wurde aufgehoben!", $"{reason}", DiscordColor.SpringGreen);

                // Setze das Timeout-Enddatum auf null, um den Timeout aufzuheben
                await member.TimeoutAsync(null, reason);
                await SendNotification(ctx, $"Der Timeout für {member.DisplayName} wurde aufgehoben.",
                                            $"User: **{member.Mention}**\n" +
                                            $"Verantwortlicher Moderator: {ctx.User.Mention}",
                                            DiscordColor.SpringGreen,
                                            0);

                await SendNotification(ctx, $"Der Timeout für {member.DisplayName} wurde aufgehoben.",
                                            $"Discord Name: **{member.Mention}**\n" +
                                            $"Discord ID: {member.Id}\n\n" +
                                            $"Grund: **{reason}**\n" +
                                            $"Verantwortlicher Moderator: {ctx.User.Mention}",
                                            DiscordColor.SpringGreen,
                                            1143512137914921101);
            }
            catch (NotFoundException)
            {
                await SendNotification(ctx, "Nicht gefunden", "Der Benutzer wurde auf dem Server nicht gefunden.", DiscordColor.Red, 0);
            }
            catch (Exception e)
            {
                await HandleException(ctx, e);
            }
        }

        public static async Task SendResponseAsync(CommandContext ctx, string message, ResponseType type = ResponseType.Default)
        {
            if (type == ResponseType.Warning)
            {
                message = ":exclamation: " + message;
            }
            else if (type == ResponseType.Missing)
            {
                message = ":mag: " + message;
            }
            else if (type == ResponseType.Error)
            {
                message = ":no_entry: " + message;
            }
            // else bleibt unverändert


            await ctx.RespondAsync(message).ConfigureAwait(false);
        }

        public static async Task SendResponseAsync(InteractionContext ctx, string message, ResponseType type = ResponseType.Default)
        {
            if (type == ResponseType.Warning)
            {
                message = ":exclamation: " + message;
            }
            else if (type == ResponseType.Missing)
            {
                message = ":mag: " + message;
            }
            else if (type == ResponseType.Error)
            {
                message = ":no_entry: " + message;
            }

            await ctx.CreateResponseAsync(message).ConfigureAwait(false);
        }

        public static async Task SendUserStateChangeAsync(CommandContext ctx, UserStateChange state, DiscordMember user,
            string reason)
        {
            var output = new DiscordEmbedBuilder()
                .WithDescription($"{state}: {user.DisplayName}#{user.Discriminator}\nIdentifier: {user.Id}\nReason: {reason}\nIssued by: {ctx.Member.DisplayName}#{ctx.Member.Discriminator}")
                .WithColor(DiscordColor.Green);
            await ctx.RespondAsync(output.Build()).ConfigureAwait(false);
        }

        public static bool CheckChannelName(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && input.Length <= 100;
        }

        public static async Task<InteractivityResult<DiscordMessage>> GetUserInteractivity(CommandContext ctx, string keyword, int seconds)
        {
            return await ctx.Client.GetInteractivity().WaitForMessageAsync(m => m.Channel.Id == ctx.Channel.Id && string.Equals(m.Content, keyword, StringComparison.InvariantCultureIgnoreCase), TimeSpan.FromSeconds(seconds)).ConfigureAwait(false);
        }

        public static async Task<InteractivityResult<DiscordMessage>> GetUserInteractivity(InteractionContext ctx, string keyword, int seconds)
        {
            return await ctx.Client.GetInteractivity().WaitForMessageAsync(m => m.Channel.Id == ctx.Channel.Id && string.Equals(m.Content, keyword, StringComparison.InvariantCultureIgnoreCase), TimeSpan.FromSeconds(seconds)).ConfigureAwait(false);
        }

        public static int LimitToRange(double value, int min = 1, int max = 100)
        {
            if (value <= min) return min;
            return (int)(value >= max ? max : value);
        }

        public static async Task RemoveMessage(DiscordMessage message)
        {
            await message.DeleteAsync().ConfigureAwait(false);
        }

        public static async Task<MemoryStream> CheckImageInput(CommandContext ctx, string input)
        {
            var stream = new MemoryStream();
            if (input != null && !Uri.TryCreate(input, UriKind.Absolute, out _) && (!input.EndsWith(".img") || !input.EndsWith(".png") || !input.EndsWith(".jpg")))
            {
                await SendResponseAsync(ctx, DarkBot.Properties.Resources.URL_INVALID_IMG, ResponseType.Warning).ConfigureAwait(false);
            }
            else
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage httpResponse = null;
                try
                {
                    httpResponse = await client.GetAsync(input).ConfigureAwait(false);
                }
                finally
                {
                    httpResponse?.Dispose();
                    client?.Dispose();
                }

                var result = await httpResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                stream.Write(result, 0, result.Length);
                stream.Position = 0;
            }

            return stream;
        }

        public static async Task<MemoryStream> CheckImageInput(InteractionContext ctx, string input)
        {
            var stream = new MemoryStream();
            if (input != null && !Uri.TryCreate(input, UriKind.Absolute, out _) && (!input.EndsWith(".img") || !input.EndsWith(".png") || !input.EndsWith(".jpg")))
            {
                await SendResponseAsync(ctx, DarkBot.Properties.Resources.URL_INVALID_IMG, ResponseType.Warning).ConfigureAwait(false);
            }
            else
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage httpResponse = null;
                try
                {
                    httpResponse = await client.GetAsync(input).ConfigureAwait(false);
                }
                finally
                {
                    httpResponse?.Dispose();
                    client?.Dispose();
                }

                var result = await httpResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                stream.Write(result, 0, result.Length);
                stream.Position = 0;
            }
            return (stream.Length > 0) ? stream : null;
        }

        public static string GetCurrentUptime()
        {
            var settings = Program.Settings;
            var uptime = DateTime.Now - settings.ProcessStarted;
            var days = uptime.Days > 0 ? $"({uptime.Days:00} days)" : string.Empty;
            return $"{uptime.Hours:00}:{uptime.Minutes:00}:{uptime.Seconds} {days}";
        }
    }
}