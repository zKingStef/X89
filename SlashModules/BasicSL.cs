using DSharpPlus.SlashCommands;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using X89Bot.EventHandlers;

namespace X89Bot.SlashModules
{
    public class BasicSL : ApplicationCommandModule
    {
        [SlashCommand("help", "Liste mit allen Befehlen")]
        public async Task HelpCommand(InteractionContext ctx)
        {
            var funButton = new DiscordButtonComponent(ButtonStyle.Success, "funButton", "Fun");
            var gameButton = new DiscordButtonComponent(ButtonStyle.Success, "gameButton", "Games");
            var modButton = new DiscordButtonComponent(ButtonStyle.Success, "modButton", "Mod");

            var helpMessage = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()

                .WithColor(DiscordColor.Aquamarine)
                .WithTitle("Command Menü")
                .WithDescription("Klicke auf einen Button um die Commands der jeweiligen Kategorien zu sehen")
                )
                .AddComponents(funButton, gameButton, modButton);

            await ctx.Channel.SendMessageAsync(helpMessage);
        }

        [SlashCommand("server", "Zeigt Informationen zum Server an")]
        [Aliases("info", "serverinfo")]
        public async Task ServerEmbed(InteractionContext ctx)
        {
            string serverDescription = $"**Servername:** {ctx.Guild.Name}\n" +
                                        $"**Server ID:** {ctx.Guild.Id}\n" +
                                        $"**Erstellt am:** {ctx.Guild.CreationTimestamp:dd/M/yyyy}\n" +
                                        $"**Owner:** {ctx.Guild.Owner.Mention}\n\n" +
                                        $"**Users:** {ctx.Guild.MemberCount}\n" +
                                        $"**Channels:** {ctx.Guild.Channels.Count}\n" +
                                        $"**Rollen:** {ctx.Guild.Roles.Count}\n" +
                                        $"**Emojis:** {ctx.Guild.Emojis.Count}";

            var serverInformation = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Gold,
                Title = "Server Informationen",
                Description = serverDescription
            };

            var response = new DiscordInteractionResponseBuilder().AddEmbed(serverInformation.WithImageUrl(ctx.Guild.IconUrl));
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
        }


        [SlashCommand("avatar", "Zeigt die Avatar-URL eines Users an")]
        [Aliases("profilbild")]
        public async Task AvatarCommand(InteractionContext ctx, [Option("user", "Der User, dessen Avatar angezeigt werden soll")] DiscordUser user = null)
        {
            var targetUser = user ?? ctx.User;

            var avatarUrl = targetUser.AvatarUrl;

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{targetUser.Username}'s Avatar",
                ImageUrl = avatarUrl,
                Color = DiscordColor.HotPink,
                Description = ctx.User.AvatarUrl,
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));
        }

        [SlashCommand("botmessage", "Sendet eine Nachricht in den Chat")]
        public async Task BotMessage(InteractionContext ctx,
                                    [Option("Titel", "Titel der Nachricht")] string title,
                                    [Option("Nachricht", "Nachricht")] string message,
                                    [Choice("Rot", "red")]
                                    [Choice("Blau", "blue")]
                                    [Choice("Schwarz", "black")]
                                    [Choice("Grün", "green")]
                                    [Choice("Gelb", "yellow")]
                                    [Choice("Weiß", "white")]
                                    [Choice("Cyan", "cyan")]
                                    [Choice("Orange", "orange")]
                                    [Choice("Gold", "gold")]
                                    [Choice("Pink", "hotpink")]
                                    [Choice("Lila", "violet")]
                                    [Option("Farbe", "Farbe")] string color)
        {
            await DiscordHelper.SendAsEphemeral(ctx, "Nachricht wurde gesendet.");

            DiscordColor embedColor = DiscordColor.White;
            switch (color)
            {
                case "red":
                    embedColor = DiscordColor.Red;
                    break;
                case "blue":
                    embedColor = DiscordColor.Blue;
                    break;
                case "black":
                    embedColor = DiscordColor.Black;
                    break;
                case "green":
                    embedColor = DiscordColor.Green;
                    break;
                case "yellow":
                    embedColor = DiscordColor.Yellow;
                    break;
                case "white":
                    embedColor = DiscordColor.White;
                    break;
                case "cyan":
                    embedColor = DiscordColor.Cyan;
                    break;
                case "orange":
                    embedColor = DiscordColor.Orange;
                    break;
                case "gold":
                    embedColor = DiscordColor.Gold;
                    break;
                case "hotpink":
                    embedColor = DiscordColor.HotPink;
                    break;
                case "violet":
                    embedColor = DiscordColor.Violet;
                    break;
            }

            var pollResultEmbed = new DiscordEmbedBuilder
            {
                Title = title,
                Description = message,
                Color = embedColor
			}.Build();

            await ctx.Channel.SendMessageAsync(embed: pollResultEmbed);
        }
    }
}