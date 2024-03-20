using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace DarkBot.EventHandlers
{
    public class DiscordLogHelper
    {
        public static async Task SendLogMessageDeleted(DiscordChannel logChannel, string title, string messageContent, DiscordChannel messageChannel, string authorMention, DiscordUser deleter)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = title,
                Description = $"**Nachricht:** ```{messageContent}```\n" +
                              $"**Channel:** {messageChannel.Mention} - {messageChannel.Name}\n" +
                              $"**Nachrichteninhaber:** {authorMention}\n\n" +
                              $"**Gelöscht von:** {deleter?.Mention ?? "Unbekannt"}",
                Color = DiscordColor.Chartreuse,
                Timestamp = DateTime.UtcNow,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = deleter?.AvatarUrl,
                    Name = deleter?.Username ?? "Unbekannt"
                }
            };

            await logChannel.SendMessageAsync(embed: embed);
        }
    }
}
