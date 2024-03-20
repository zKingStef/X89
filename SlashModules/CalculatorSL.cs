using DSharpPlus.SlashCommands;
using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus;

namespace DarkBot.SlashModules
{
    public class CalculatorSL : ApplicationCommandModule
    {
        [SlashCommand("calculate", "Der beste Taschenrechner der Welt")]
        public async Task Calculate(InteractionContext ctx,
                                [Option("Zahl1", "Gib die erste Zahl ein")] double num1,
                                [Option("Operator", "Gib einen Operator ein ( +, -, *, / ) ")] string op,
                                [Option("Zahl2", "Gib die zweite Zahl ein")] double num2)
        {
            double result = 0;
            
            switch (op)
            {
                case "+":
                    result = num1 + num2;
                    break;

                case "-":
                    result = num1 - num2;
                    break;

                case "*":
                    result = num1 * num2;
                    break;

                case "/":
                    result = num1 / num2;
                    break;

                default:
                    await ctx.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                                     .WithContent(($"Ungültiger Rechenoperator. Versuche es erneut")).AsEphemeral(true));
                    break;
            }

            var calculatorEmbed = new DiscordEmbedBuilder
            {
                Title = "**Taschenrechner**",
                Color = DiscordColor.CornflowerBlue,
                Description = $"{num1} {op} {num2} = **{result}**",
                Timestamp = DateTime.UtcNow,
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(calculatorEmbed.Build()));

        }
    }
}
