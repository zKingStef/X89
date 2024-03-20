using System;
using System.Threading.Tasks;
using DSharpPlus;
using X89Bot.Commands;
using X89Bot.SlashModules;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.EventArgs;
using X89Bot.EventHandlers;
using System.Linq;
using DSharpPlus.VoiceNext;
using DSharpPlus.Lavalink;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.Interactivity.Enums;
using System.IO;
using X89Bot.Services;
using X89Bot.Common;
using DSharpPlus.SlashCommands.EventArgs;

namespace X89Bot
{
	internal sealed class X89Bot
	{
        private static DiscordClient Client { get; set; } 
        private IServiceProvider Services { get; }
        private static EventId EventId { get; } = new EventId(1000, Program.Settings.Name);
        private CommandsNextExtension Commands { get; }
        private InteractivityExtension Interactivity { get; }
        private VoiceNextExtension Voice { get; }
        private LavalinkExtension Lavalink { get; }
        private SlashCommandsExtension Slash { get; }

        public X89Bot(int shardId = 0)
         {
            // Get Settings
            var settings = Program.Settings;

            Client = new DiscordClient(new DiscordConfiguration
            {
                Token = settings.Tokens.DiscordToken,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                ReconnectIndefinitely = true,
                MinimumLogLevel = LogLevel.Information,
                GatewayCompressionLevel = GatewayCompressionLevel.Stream,
                LargeThreshold = 250,
                MessageCacheSize = 2048,
                LogTimestampFormat = "yyyy-MM-dd HH:mm:ss zzz",
                ShardId = shardId,
                ShardCount = settings.ShardCount
            });

            // Setup Services
            Services = new ServiceCollection()
                .AddSingleton<MusicService>()
                .AddSingleton(new LavalinkService(Client))
                .AddSingleton(this)
                .BuildServiceProvider(true);

            // Setup Commands
            Commands = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                CaseSensitive = false,
                IgnoreExtraArguments = true,
                Services = Services,
                PrefixResolver = PrefixResolverAsync, // Set the command prefix that will be used by the bot
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = true,
            });
			Commands.CommandExecuted += Command_Executed;
			Commands.CommandErrored += Command_Errored;
            Commands.SetHelpFormatter<HelpFormatter>();

            //4. Set the default timeout for Commands that use interactivity
            Interactivity = Client.UseInteractivity(new InteractivityConfiguration
            {
                PaginationBehaviour = PaginationBehaviour.Ignore,
                Timeout = TimeSpan.FromSeconds(30)
            });

            Voice = Client.UseVoiceNext(new VoiceNextConfiguration
            {
                AudioFormat = AudioFormat.Default,
                EnableIncoming = true
            });

            // Register Commands
            Commands.RegisterCommands<BasicCommands>();
            RegisterSlashCommands(Client);

            //5. Set up the Task Handler Ready event
            Client.Ready += OnClientReady;
            Client.ComponentInteractionCreated += ButtonPressResponse;
            Client.ClientErrored += ClientErrorHandler;
            Client.GuildMemberAdded += UserJoinHandler;
            Client.GuildMemberRemoved += UserLeaveHandler;
			Client.GuildAvailable += Guild_Available;
            Client.UserUpdated += UserUpdatedHandler;
            // Message Log
            Client.MessageDeleted += MessageDeletedHandler;
            Client.MessageUpdated += MessageUpdatedHandler;
            Client.MessagesBulkDeleted += MessageBulkDeletedHandler;
            // Ban/Unban Log
            Client.GuildBanAdded += BanHandler;
            Client.GuildBanRemoved += UnbanHandler;
            // Roles Log
            Client.GuildRoleCreated += RoleCreatedHandler;
            Client.GuildRoleDeleted += RoleDeletedHandler;
            Client.GuildRoleUpdated += RoleUpdatedHandler;
            // Channel Log
            Client.ChannelCreated += ChannelCreatedHandler;
            Client.ChannelDeleted += ChannelDeletedHandler;
            Client.ChannelUpdated += ChannelUpdatedHandler;
            // Voice Log
            Client.VoiceServerUpdated += VoiceServerUpdatedHandler;
            Client.VoiceStateUpdated += VoiceStateUpdatedHandler;
            // Invite Log
            Client.InviteCreated += InviteCreatedHandler;
            Client.InviteDeleted += InviteDeletedHandler;


            // Setup Lavalink
            if (settings.Lavalink.Enabled)
            {
                if (File.Exists($"{Directory.GetCurrentDirectory()}/Lavalink.jar"))
                {
                    Client.Logger.LogInformation(EventId, "Lavalink wird konfiguriert...");
                    Lavalink = Client.UseLavalink();
                }
                else
                {
                    Client.Logger.LogInformation(EventId, $"Lavalink Node konnte nicht gefunden werden: {Directory.GetCurrentDirectory()}");
                }
            }

            // Start the uptime counter
            Console.Title = $"{settings.Name}-{settings.Version}";
            settings.ProcessStarted = DateTime.Now;
        }

		private Task Guild_Available(DiscordClient sender, GuildCreateEventArgs e)
		{
            sender.Logger.LogInformation(EventId, $"Connected to server: {e.Guild.Name}");
            return Task.CompletedTask;
        }

        private static async Task Command_Errored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            await ExceptionHandler.Process(e, EventId);
        }

        private Task Command_Executed(CommandsNextExtension sender, CommandExecutionEventArgs e)
		{
            e.Context.Client.Logger.LogInformation(EventId, $"[{e.Context.Guild.Name} : {e.Context.Channel.Name}] {e.Context.User.Username} executed the command '{e.Command.QualifiedName}'");
            return Task.CompletedTask;
        }

        private static async Task SlashCommand_Errored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            await ExceptionHandler.Process(e, EventId);
        }

        //
        private static async Task MessageBulkDeletedHandler(DiscordClient sender, MessageBulkDeleteEventArgs e)
        {
            await Task.Delay(1);
        }

        private static async Task InviteDeletedHandler(DiscordClient sender, InviteDeleteEventArgs e)
        {
            await DiscordHelper.SendLogMessage(Client,
                                            1185697809874288671,
                                            AuditLogActionType.InviteDelete,
                                            "Einladung gelöscht!",
                                            $"**User:** {e.Invite.Inviter.Mention} \n" +
                                            $"**Einladung:** {e.Invite.Code} \n" +
                                            $"**Verwendungen**: {e.Invite.Uses}\n" +
                                            $"**max. Verwendungen:** {e.Invite.MaxUses}\n",
                                            DiscordColor.IndianRed);
        }

        private static async Task InviteCreatedHandler(DiscordClient sender, InviteCreateEventArgs e)
        {
            await DiscordHelper.SendLogMessage(Client,
                                            1185697809874288671,
                                            "Neue Einladung erstellt!",
                                            $"**User:** {e.Invite.Inviter.Mention} \n" +
                                            $"**Einladung:** {e.Invite.Code}, **läuft ab am** {e.Invite.ExpiresAt} \n" +
                                            $"**max. Verwendungen:** {e.Invite.MaxUses} \n",
                                            DiscordColor.SpringGreen);
        }

        private static async Task ClientErrorHandler(DiscordClient sender, ClientErrorEventArgs e)
        {
            await DiscordHelper.SendLogMessage(Client,
                                           1185697808485978163,
                                           "Ein Fehler ist aufgetreten!",
                                           $"**Falls dieses Problem weiterhin bestehen bleibt, kontaktiere einen Administrator.**\n\n" +
                                           $"**{e.EventName}**\n{e.Exception}\n{e.Exception.Message}\n\n{e.Exception.InnerException}",
                                           DiscordColor.SpringGreen);
        }

        private static async Task VoiceServerUpdatedHandler(DiscordClient sender, VoiceServerUpdateEventArgs e)
        {
            await DiscordHelper.SendLogMessage(Client,
                                                   1143519998669758584,
                                                   "",
                                                   $"**{e.Endpoint} {e}**\n",
                                                   DiscordColor.SpringGreen);
        }

        private static async Task VoiceStateUpdatedHandler(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            if (e.Before == null && e.After != null && e.After.Channel != null)
            {
                await DiscordHelper.SendLogMessage(Client,
                                                   1187080025737003059,
                                                   "Voicechat",
                                                   $"**{e.User.Mention} hat den Sprachkanal {e.After.Channel.Mention} betreten.**\n",
                                                   DiscordColor.SpringGreen);
            }
            else if (e.After == null)
            {
                await DiscordHelper.SendLogMessage(Client,
                                                   1187080025737003059,
                                                   "Voicechat",
                                                   $"**{e.User.Mention} hat den Sprachkanal {e.Before.Channel.Mention} verlassen.**\n",
                                                   DiscordColor.IndianRed);
            }
        }



        private static async Task UserUpdatedHandler(DiscordClient sender, UserUpdateEventArgs e)
        {
            await DiscordHelper.SendLogMessage(Client,
                                           1185697809874288671,
                                           AuditLogActionType.MemberUpdate,
                                           "Nickname geändert!",
                                           $"**User:** {e.UserAfter.Mention}" +
                                           $"**Alter Nickname:** ```{e.UserBefore.Username}```\n " +
                                           $"**Neuer Nickname:** ```{e.UserAfter.Username}```" +
                                           $"**Verantwortlicher Moderator:** ",
                                           DiscordColor.Cyan);
        }

        private static async Task ChannelUpdatedHandler(DiscordClient sender, ChannelUpdateEventArgs e)
        {
            if (e.ChannelAfter.Name != e.ChannelBefore.Name)
            {
                await DiscordHelper.SendLogMessage(Client,
                                               1185697811543633980,
                                               AuditLogActionType.ChannelUpdate,
                                               "Kanalname geändert!",
                                               $"**Channel:** {e.ChannelAfter.Mention}\n" +
                                               $"**Alter Name:**\n```{e.ChannelBefore.Name}```\n " +
                                               $"**Neuer Name:**\n```{e.ChannelAfter.Name}```\n " +
                                               $"**Verantwortlicher Moderator:** ",
                                               DiscordColor.Cyan);
            }
        }

        private static async Task ChannelDeletedHandler(DiscordClient sender, ChannelDeleteEventArgs e)
        {
            await DiscordHelper.SendLogMessage(Client,
                                               1185697811543633980,
                                               AuditLogActionType.ChannelDelete,
                                               "Ein Kanal wurde gelöscht!",
                                               $"Channel Name: ```{e.Channel.Name}```\n" +
                                               $"**Verantwortlicher Moderator:** ",
                                               DiscordColor.IndianRed);
        }

        private static async Task ChannelCreatedHandler(DiscordClient sender, ChannelCreateEventArgs e)
        {
            await DiscordHelper.SendLogMessage(Client,
                                               1185697811543633980,
                                               AuditLogActionType.ChannelCreate,
                                               "Ein Neuer Kanal wurde erstellt!",
                                               $"**Channel:** {e.Channel.Mention}\n " +
                                               $"Channel Name: ```{e.Channel.Name}```\n" +
                                               $"**Verantwortlicher Moderator:** ",
                                               DiscordColor.SpringGreen);
        }

        private static async Task RoleUpdatedHandler(DiscordClient sender, GuildRoleUpdateEventArgs e)
        {
            if (e.RoleAfter.Name != e.RoleBefore.Name)
            {
                await DiscordHelper.SendLogMessage(Client,
                                                   1185697811543633980,
                                                   AuditLogActionType.RoleUpdate,
                                                   "Eine Rolle wurde umbenannt!",
                                                   $"**Rolle:** {e.RoleAfter.Mention}\n\n " +
                                                   $"**Alter Name:** ```{e.RoleBefore.Name}```\n" +
                                                   $"**Neuer Name:** ```{e.RoleAfter.Name}```\n" +
                                                   "**Verantwortlicher Moderator:** ",
                                                   DiscordColor.Cyan);
            }
        }

        private static async Task RoleDeletedHandler(DiscordClient sender, GuildRoleDeleteEventArgs e)
        {
            await DiscordHelper.SendLogMessage(Client,
                                               1185697811543633980,
                                               AuditLogActionType.RoleDelete,
                                               "Eine Rolle wurde gelöscht!",
                                               $"**Rolle:** {e.Role.Name}\n\n " +
                                               "**Verantwortlicher Moderator:** ",
                                               DiscordColor.IndianRed);
        }

        private static async Task RoleCreatedHandler(DiscordClient sender, GuildRoleCreateEventArgs e)
        {
            await DiscordHelper.SendLogMessage(Client,
                                               1185697811543633980,
                                               AuditLogActionType.RoleCreate,
                                               "Eine neue Rolle wurde erstellt!",
                                               $"**Rolle:** {e.Role.Name}  -  {e.Role.Mention}\n\n " +
                                               "**Verantwortlicher Moderator:** ",
                                               DiscordColor.SpringGreen);
        }

        private static async Task MessageUpdatedHandler(DiscordClient sender, MessageUpdateEventArgs e)
        {
            await DiscordHelper.SendLogMessage(Client,
                                               1185697809874288671,
                                               $"Eine Nachricht wurde bearbeitet! {e.Message.JumpLink}",
                                               $"**User:** {e.Message.Author.Mention}\n" +
                                               $"**Alte Nachricht:** {e.MessageBefore.Content}\n" +
                                               $"**Neue Nachricht:** {e.Message.Content}",
                                               DiscordColor.Cyan);
        }

        private static async Task UnbanHandler(DiscordClient sender, GuildBanRemoveEventArgs e)
        {
            var auditLogs = await e.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.Unban);
            var lastLog = auditLogs.FirstOrDefault();

            await DiscordHelper.SendLogMessage(Client,
                                               1185697809874288671,
                                               AuditLogActionType.Unban,
                                               "User entbannt!",
                                               $"**User:** {e.Member.Mention}\n " +
                                               $"Discord Name: {e.Member.DisplayName}\n" +
                                               $"Discord ID: {e.Member.Id}\n\n" +
                                               $"Grund: {lastLog?.Reason}\n" +
                                               "**Verantwortlicher Moderator:** ",
                                               DiscordColor.SpringGreen);
        }

        private static async Task BanHandler(DiscordClient sender, GuildBanAddEventArgs e)
        {
            var auditLogs = await e.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.Ban);
            var lastLog = auditLogs.FirstOrDefault();

            await DiscordHelper.SendLogMessage(Client,
                                               1185697809874288671,
                                               AuditLogActionType.Ban,
                                               "User gebannt!",
                                               $"**User:** {e.Member.Mention}\n " +
                                               $"Discord Name: {e.Member.DisplayName}\n" +
                                               $"Discord ID: {e.Member.Id}\n\n" +
                                               $"Grund: {lastLog?.Reason}\n" +
                                               "**Verantwortlicher Moderator:** ",
                                               DiscordColor.IndianRed);
        }

        private static async Task MessageDeletedHandler(DiscordClient sender, MessageDeleteEventArgs e)
        {
            await DiscordHelper.SendLogMessage(Client,
                                               1185697809874288671,
                                               AuditLogActionType.MessageDelete,
                                               $"Eine Nachricht wurde gelöscht {e.Message.JumpLink}",
                                               $"**Nachricht:** \n {e.Message.Content}\n " +
                                               $"Nachrichteninhaber: {e.Message.Author.Mention}\n\n" +
                                               "**Gelöscht von:** ",
                                               DiscordColor.IndianRed);
        }

        private static async Task UserLeaveHandler(DiscordClient sender, GuildMemberRemoveEventArgs e)
        {
            await DiscordHelper.SendLogMessage(Client,
                                               978350423482191924,
                                               "Server",
                                               $"{e.Member.Mention} hat den Server verlassen.\n " +
                                               $"Ist beigetreten am: {e.Guild.JoinedAt.DateTime}",
                                               DiscordColor.SpringGreen);
        }

        private static async Task UserJoinHandler(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            var welcomeChannel = e.Guild.GetChannel(978346565418770433);

            var welcomeEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Magenta,
                Title = $"Herzlich Wilkommen!",
                Description = $"{e.Member.Mention} hat den Discord betreten!",
                Timestamp = DateTimeOffset.Now,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = e.Guild.Name
                },
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = e.Member.AvatarUrl,
                    Name = e.Member.Username
                }
            };

            await welcomeChannel.SendMessageAsync(embed: welcomeEmbed);

            await DiscordHelper.SendLogMessage(Client,
                                               978350400216399932,
                                               "Server",
                                               $"{e.Member.Mention} trat dem Server bei.\n " +
                                               $"**Alter des Kontos:** ```{e.Member.CreationTimestamp.DateTime}```",
                                               DiscordColor.SpringGreen);
        }

        private static Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            var settings = Program.Settings;
            sender.Logger.LogInformation(EventId, $"{settings.Name}, version {settings.Version}");
            return Task.CompletedTask;
        }

        private static async Task ButtonPressResponse(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            switch (e.Interaction.Data.CustomId)
            {
                case "entryGiveawayButton":
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                                    .WithContent("Du bist dem **Gewinnspiel** erfolgreich beigetreten! Viel Glück:tada:").AsEphemeral(true));
                    break;
                case "funButton":
                    string funCommandsList = "/pingspam" +
                                             "/poll" +
                                             "/giveaway" +
                                             "/avatar" +
                                             "/server";

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent(funCommandsList));
                    break;
                case "gameButton":
                    string gameCommandsList = "/" +
                                              "/";

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent(gameCommandsList));
                    break;
                case "modButton":
                    string modCommandsList = "/clear\n" +
                                             "/ban";

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent(modCommandsList));
                    break;
                case string customId when customId.StartsWith("ticket"):
                    TicketHandler.HandleTicketInteractions(e, customId);
                    break;
                case "pollOption1Button":
                case "pollOption2Button":
                case "pollOption3Button":
                    await FunSL.ProcessPollVote(e);
                    break;
                default:
                    //await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Ein Fehler ist aufgetreten. Bitte kontaktiere einen <@&1210230414011011124>"));
                    break;
            }

        }

        //
        private static void RegisterSlashCommands(DiscordClient Client)
        {
            var slash = Client.UseSlashCommands();

            slash.RegisterCommands<FunSL>(); // 1076192773776081029 GuildID
            slash.RegisterCommands<ModSL>();
            slash.RegisterCommands<BasicSL>();
            slash.RegisterCommands<TicketSL>();
            slash.RegisterCommands<GiveawaySL>();
            slash.RegisterCommands<CalculatorSL>();
            slash.RegisterCommands<ImageSL>();
            slash.RegisterCommands<CasinoSL>();
            slash.RegisterCommands<MusicBotSL>();
			slash.SlashCommandErrored += SlashCommand_Errored;
        }

		//
		public async Task RunAsync()
        {
            // Update any other services that are being used.
            Client.Logger.LogInformation(EventId, "Bot wird gestartet...");

            // Set the initial activity and connect the bot to Discord
            var act = new DiscordActivity("X89", ActivityType.ListeningTo);
            await Client.ConnectAsync(act, UserStatus.DoNotDisturb).ConfigureAwait(false);
        }

        public async Task StopAsync()
        {
            await Client.DisconnectAsync().ConfigureAwait(false);
        }

        private static Task<int> PrefixResolverAsync(DiscordMessage m)
        {
            return Task.FromResult(m.GetStringPrefixLength(Program.Settings.Prefix));
        }
    }
}

