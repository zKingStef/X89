﻿using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using X89Bot.Common;
using X89Bot.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using X89Bot.Services;
using X89Bot.Models.Music;

namespace X89Bot.SlashModules
{
	[SlashCommandGroup("music", "Slash command group for music commands.")]
    public class MusicBotSL : ApplicationCommandModule
    {
        private MusicService Service { get; }
        private MusicPlayer Player { get; set; }

        public MusicBotSL(MusicService service)
        {
            Service = service;
        }

        [SlashCommand("stop", "Stops audio playback and leave the voice channel.")]
        public async Task StopSong(InteractionContext ctx)
        {
            await BeforeExecutionAsync(ctx);
            await Player.StopAsync();
            await Player.DestroyPlayerAsync();
            await ctx.CreateResponseAsync(":stop_button: Stopping Playback...").ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync(ctx.CommandName);
        }

        [SlashCommand("pause", "Pauses audio playback.")]
        public async Task PauseSong(InteractionContext ctx)
        {
            await BeforeExecutionAsync(ctx);
            await Player.PauseAsync();
            await ctx.CreateResponseAsync(":pause_button: Pausing Playback...").ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync(ctx.CommandName);
        }

        [SlashCommand("resume", "Resumes audio playback.")]
        public async Task ResumeAsync(InteractionContext ctx)
        {
            await BeforeExecutionAsync(ctx);
            await Player.ResumeAsync();
            await ctx.CreateResponseAsync(":play_pause: Resuming Playback...").ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync(ctx.CommandName);
        }

        [SlashCommand("volume", "Changes audio playback volume.")]
        public async Task SetVolume(InteractionContext ctx, [Option("volume", "Volume of the playback.")] double volume = 100)
        {
            await BeforeExecutionAsync(ctx);
            if (volume < 0 || volume > 150)
            {
                await ctx.CreateResponseAsync(":warning: Volume must be greater than 0, and less than or equal to 150.").ConfigureAwait(false);
                return;
            }

            await Player.SetVolumeAsync((int)volume);
            await ctx.CreateResponseAsync($":speaker: Volume set to {volume}%.").ConfigureAwait(false);
        }

        [SlashCommand("restart", "Restarts audio playback.")]
        public async Task RestartSong(InteractionContext ctx)
        {
            await BeforeExecutionAsync(ctx);
            var track = Player.NowPlaying;
            await Player.RestartAsync();
            await ctx.CreateResponseAsync($":play_pause: Restarting {Formatter.Bold(Formatter.Sanitize(track.Track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Track.Author))}...").ConfigureAwait(false);
        }

        [SlashCommand("nowplaying", "Returns the current audio track.")]
        public async Task NowPlaying(InteractionContext ctx)
        {
            await BeforeExecutionAsync(ctx);
            var track = Player.NowPlaying;
            if (Player.NowPlaying.Track?.TrackString == null)
                await ctx.CreateResponseAsync("Currently not playing anything...").ConfigureAwait(false);
            else
                await ctx.CreateResponseAsync($":musical_note: Now playing: {Formatter.Bold(Formatter.Sanitize(track.Track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Track.Author))} [{MusicService.ToDurationString(Player.GetCurrentPosition())}/{MusicService.ToDurationString(Player.NowPlaying.Track.Length)}] requested by {Formatter.Bold(Formatter.Sanitize(Player.NowPlaying.Requester.DisplayName))}.").ConfigureAwait(false);
        }

        [SlashCommand("play", "Plays audio from a URL or search query.")]
        public async Task PlaySong(InteractionContext ctx, [Option("url", "URL to playback.")] string url)
        {
            await BeforeExecutionAsync(ctx);
            var trackLoad = await Service.GetTracksAsync(new Uri(url, UriKind.Absolute));
            var audio = trackLoad.Tracks.FirstOrDefault();
            if (trackLoad.LoadResultType == LavalinkLoadResultType.LoadFailed || audio is null)
            {
                await ctx.CreateResponseAsync(":mag: No tracks were found at specified link.").ConfigureAwait(false);
                return;
            }

            Player.Enqueue(new MusicData(audio, ctx.Member));
            await Player.CreatePlayerAsync(ctx.Member.VoiceState.Channel);
            await Player.PlayAsync();
            await ctx.CreateResponseAsync($":play_pause: Now playing: {Formatter.Bold(Formatter.Sanitize(audio.Title))} by {Formatter.Bold(Formatter.Sanitize(audio.Author))}.").ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync(ctx.CommandName);
        }

        public async Task BeforeExecutionAsync(InteractionContext ctx)
        {
            // Check that the user is in a voice channel
            var channel = ctx.Member.VoiceState?.Channel;
            if (channel == null)
            {
                await ctx.CreateResponseAsync("You need to be in a voice channel.").ConfigureAwait(false);
                return;
            }

            // Check that the user in the same voice channel
            var userState = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (userState != null && channel != userState)
            {
                await ctx.CreateResponseAsync("You need to be in the same voice channel.").ConfigureAwait(false);
                return;
            }

            // Connect the music play to the voice channel
            Player = await Service.GetOrCreateDataAsync(ctx.Guild);
            Player.CommandChannel = ctx.Channel;
        }
    }
}