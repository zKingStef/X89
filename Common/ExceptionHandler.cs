using X89Bot.EventHandlers;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace X89Bot.Common
{
	class ExceptionHandler
	{
        public static async Task Process(CommandErrorEventArgs e, EventId eventId)
        {
            if (e.Exception is CommandNotFoundException)
            {
                await DiscordHelper.SendResponseAsync(e.Context, e.Exception.Message, ResponseType.Missing).ConfigureAwait(false);
            }
            else if (e.Exception is InvalidOperationException)
            {
                await DiscordHelper.SendResponseAsync(e.Context, e.Exception.Message, ResponseType.Warning).ConfigureAwait(false);
            }
            else if (e.Exception is ChecksFailedException cfe)
            {
                await DiscordHelper.SendResponseAsync(e.Context, $"Befehl {Formatter.Bold(e.Command.QualifiedName)} konnte nicht ausgeführt werden.", ResponseType.Error).ConfigureAwait(false);
                foreach (var check in cfe.FailedChecks)
                {
                    if (check is RequireUserPermissionsAttribute perms)
                    {
                        await DiscordHelper.SendResponseAsync(e.Context, $"- Du hast nicht genügend Berechtigungen ({perms.Permissions.ToPermissionString()})!", ResponseType.Error).ConfigureAwait(false);
                    }
                    else if (check is RequireBotPermissionsAttribute perms2)
                    {
                        await DiscordHelper.SendResponseAsync(e.Context, $"- Ich habe nicht genügend Berechtigungen ({perms2.Permissions.ToPermissionString()})!", ResponseType.Error).ConfigureAwait(false);
                    }
                    else if (check is RequireOwnerAttribute)
                    {
                        await DiscordHelper.SendResponseAsync(e.Context, "- Dieser Befehl ist nur für den Bot-Besitzer reserviert.", ResponseType.Error).ConfigureAwait(false);
                    }
                    else if (check is RequirePrefixesAttribute pa)
                    {
                        await DiscordHelper.SendResponseAsync(e.Context, $"- Dieser Befehl kann nur mit den folgenden Prefix ausgeführt werden: {string.Join("! ", pa.Prefixes)}.", ResponseType.Error).ConfigureAwait(false);
                    }
                    else
                    {
                        await DiscordHelper.SendResponseAsync(e.Context, "Unbekannter Fehler aufgetreten. Bitte benachrichtige den Entwickler mit dem Befehl *!bot report*.", ResponseType.Error).ConfigureAwait(false);
                    }
                }
            }
        }

        public static async Task Process(SlashCommandErrorEventArgs e, EventId eventId)
        {
            if (e.Exception is CommandNotFoundException)
            {
                await DiscordHelper.SendResponseAsync(e.Context, e.Exception.Message, ResponseType.Missing).ConfigureAwait(false);
            }
            else if (e.Exception is InvalidOperationException)
            {
                await DiscordHelper.SendResponseAsync(e.Context, e.Exception.Message, ResponseType.Warning).ConfigureAwait(false);
            }
            else if (e.Exception is ChecksFailedException cfe)
            {
                await DiscordHelper.SendResponseAsync(e.Context, $"Befehl {Formatter.Bold(e.Context.QualifiedName)} konnte nicht ausgeführt werden.", ResponseType.Error).ConfigureAwait(false);
                foreach (var check in cfe.FailedChecks)
                {
                    if (check is RequireUserPermissionsAttribute perms)
                    {
                        await DiscordHelper.SendResponseAsync(e.Context, $"- Du hast nicht genügend Berechtigungen ({perms.Permissions.ToPermissionString()})!", ResponseType.Error).ConfigureAwait(false);
                    }
                    else if (check is RequireBotPermissionsAttribute perms2)
                    {
                        await DiscordHelper.SendResponseAsync(e.Context, $"- Ich habe nicht genügend Berechtigungen ({perms2.Permissions.ToPermissionString()})!", ResponseType.Error).ConfigureAwait(false);
                    }
                    else if (check.GetType() == typeof(RequireOwnerAttribute))
                    {
                        await DiscordHelper.SendResponseAsync(e.Context, "- Dieser Befehl ist nur für den Bot-Besitzer reserviert.", ResponseType.Error).ConfigureAwait(false);
                    }
                    else if (check is RequirePrefixesAttribute pa)
                    {
                        await DiscordHelper.SendResponseAsync(e.Context, $"- Dieser Befehl kann nur mit den folgenden Präfixen ausgeführt werden: {string.Join(" ", pa.Prefixes)}.", ResponseType.Error).ConfigureAwait(false);
                    }
                    else
                    {
                        await DiscordHelper.SendResponseAsync(e.Context, "Unbekannter Check ausgelöst. Bitte benachrichtige den Entwickler mit dem Befehl *.bot report*.", ResponseType.Error).ConfigureAwait(false);
                    }
                }
            }
            else if (e.Exception is ArgumentNullException || e.Exception is ArgumentException)
            {
                await DiscordHelper.SendResponseAsync(e.Context, $"Ungültige oder fehlende Parameter. Verwende für Hilfe den Befehl `.help {e.Context?.QualifiedName}`", ResponseType.Warning);
            }
            else if (e.Exception is UnauthorizedException)
            {
                await DiscordHelper.SendResponseAsync(e.Context, "Einer von uns hat nicht die erforderlichen Berechtigungen.", ResponseType.Warning);
            }
            else if (e.Exception is NullReferenceException || e.Exception is InvalidDataException)
            {
                e.Context.Client.Logger.LogWarning(eventId, e.Exception, $"[{e.Context.Guild.Name} : {e.Context.Channel.Name}] {e.Context.User.Username} hat den Befehl '{e.Context?.QualifiedName ?? "<unbekannt>"}' ausgeführt, aber es ist ein Fehler aufgetreten:");
                await DiscordHelper.SendResponseAsync(e.Context, e.Exception.Message, ResponseType.Error).ConfigureAwait(false);
            }
            else
            {
                e.Context.Client.Logger.LogError(eventId, $"[{e.Exception.GetType()}] Nicht behandelte Ausnahme. {e.Exception.Message}");
            }
        }
    }
}