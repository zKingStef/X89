using X89Bot.EventHandlers;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace X89Bot.Commands
{
    public class BasicCommands : BaseCommandModule
    {
        [Command("ping"), Description("Check your ping")]
        [Aliases("pong")]
        public async Task Ping(CommandContext ctx)
        {
            var ping = DateTime.Now - ctx.Message.CreationTimestamp;
            string desc = $"Latenz ist `{ping.Milliseconds}ms`\nAPI Latenz ist `{ctx.Client.Ping}ms`";

            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Cyan)
                .WithTimestamp(DateTime.Now)
                .WithTitle(":ping_pong: " + Formatter.Bold("Pong!"))
                .WithFooter($"Angefordert von {ctx.User.Username}")
                .WithDescription(desc);


            await ctx.Channel.SendMessageAsync(embed.Build()).ConfigureAwait(false);

        }

        [Command("botmessage")]
        [Description("Sendet eine Nachricht in einen bestimmten Channel.")]
        public async Task SendMessage(CommandContext ctx, ulong channelId, [RemainingText] string message)
        {
            // Überprüfen, ob der Benutzer Administratorrechte hat
            if (!ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageMessages))
            {
                await ctx.RespondAsync("Du hast nicht die erforderlichen Berechtigungen, um diesen Befehl auszuführen.");
                return;
            }

            // Hole den Channel anhand der angegebenen ID
            var channel = await ctx.Client.GetChannelAsync(channelId);

            // Überprüfen, ob der Channel gefunden wurde
            if (channel == null)
            {
                await ctx.RespondAsync("Der angegebene Channel wurde nicht gefunden.");
                return;
            }

            // Sende die Nachricht in den Channel
            await channel.SendMessageAsync(message);
        }

        [Command("sendmessage")]
        [Description("Sendet eine Embed-Nachricht in einen bestimmten Channel.")]
        public async Task SendMessage(CommandContext ctx, ulong channelId, string colorHex, [RemainingText] string message)
        {
            // Überprüfen, ob der Benutzer Administratorrechte hat
            if (!ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.Administrator))
            {
                await ctx.RespondAsync("Du hast nicht die erforderlichen Berechtigungen, um diesen Befehl auszuführen.");
                return;
            }

            // Konvertiere die Hex-Farbe in Discord-Color
            DiscordColor color = new DiscordColor(colorHex);

            // Erstelle ein Embed mit der Nachricht und der angegebenen Farbe
            var embed = new DiscordEmbedBuilder
            {
                Description = message,
                Color = color
            };

            // Hole den Channel anhand der angegebenen ID
            var channel = await ctx.Client.GetChannelAsync(channelId);

            // Überprüfen, ob der Channel gefunden wurde
            if (channel == null)
            {
                await ctx.RespondAsync("Der angegebene Channel wurde nicht gefunden.");
                return;
            }

            // Sende das Embed in den Channel
            await channel.SendMessageAsync(embed: embed);
        }
    }
}
