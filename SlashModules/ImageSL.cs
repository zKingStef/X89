
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Google.Apis.Services;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System;
using System.Threading.Tasks;

namespace X89Bot.SlashModules
{
    public class ImageSL : ApplicationCommandModule
    {
        [SlashCommand("googlebild", "Google Bild Suche")]
        public async Task GoogleImageSearch(InteractionContext ctx,
                                            [Option("suche", "Nach welchem Bild möchtest du suchen ? ")] string search)
        {
            var imageUrl = await GetImageUrl(search);
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Bildsuchergebnis für: " + search)
                    .WithImageUrl(imageUrl)
                    .WithColor(DiscordColor.Gold);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Keine Bilder gefunden."));
            }
        }

        private async Task<string> GetImageUrl(string query)
        {
            using (var httpClient = new HttpClient())
            {
                var url = $"https://www.googleapis.com/customsearch/v1?key={"AIzaSyAPQIMZWLLuU_bl2YN1CNfhIN3UrfOo-Ig"}&cx={"c64d4b9929e6b4551"}&searchType=image&q={Uri.EscapeDataString(query)}";
                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);
                var items = json["items"];
                if (items != null && items.HasValues)
                {
                    return items[0]["link"].ToString();
                }
                return null;
            }
        }

        [SlashCommand("hund", "Generiere ein zufälliges Bild von einem Hund")]
        public async Task Hund(InteractionContext ctx)
        {
            var dog = "http://random.dog/" + await SearchHelper.GetResponseStringAsync("https://random.dog/woof").ConfigureAwait(false);
            var embed = new DiscordEmbedBuilder().WithImageUrl(dog).WithTitle("so ein Feini").WithUrl(dog);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));

        }
    }
}
