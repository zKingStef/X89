using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkBot.SlashModules
{
    public class ChatGPTModule : ApplicationCommandModule
    {
        //private readonly OpenAIClient client;
        //
        //public ChatGPTModule(string apiKey)
        //{
        //    client = new OpenAIClient(apiKey);
        //}
        
        //[SlashCommand("chatgpt", "Verwende ChatGPT, um auf eine Nachricht zu antworten")]
        //public async Task ChatGPT(InteractionContext ctx,
        //                          [Option("suche", "Die Nachricht, auf die ChatGPT antworten soll")] string query)
        //{
        //    var prompt = "Ich sage: " + query;
        //    var response = await client.Completions.CreateCompletionAsync(prompt, temperature: 0.7, maxTokens: 50);
        //   var reply = response.Choices.FirstOrDefault()?.Text;
        //
        //    if (!string.IsNullOrEmpty(reply))
        //    {
        //        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
        //            new DiscordInteractionResponseBuilder().WithContent(reply));
        //    }
        //    else
        //    {
        //        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
        //            new DiscordInteractionResponseBuilder().WithContent("Es tut mir leid, ich konnte keine Antwort finden."));
        //    }
        //}
    }
}