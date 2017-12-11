﻿using Discord;
using Discord.Commands;
using dotbot.Services;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace dotbot.Commands
{
    [Group("poll")]
    public class Polls : ModuleBase<SocketCommandContext>
    {
        private Dictionary<ulong, Poll> _polls;
        private IConfigurationRoot _config;

        public Polls(PollService polls, IConfigurationRoot config)
        {
            _polls = polls.currentPolls;
            _config = config;
        }

        [Command]
        [Priority(0)]
        [Summary("create a new poll")]
        public async Task CreatePoll([Summary("poll options")] string[] options = null)
        {
            var pollId = Context.Channel.Id;
            if (_polls.ContainsKey(pollId))
            {
                await ReplyAsync($"respond with some more options or start the poll with `{_config["prefix"]}poll start`");
            }
            else
            {
                _polls[pollId] = new Poll
                {
                    Owner = Context.User,
                    IsOpen = false,
                    Options = options == null ? new List<PollOption>() : options.Select(o => new PollOption{ Text = o }).ToList()
                };
                await ReplyAsync($"you started a new poll. respond with some options and then start the poll with `{_config["prefix"]}poll start`");
            }
        }


        [Command("start")]
        [Priority(1)]
        [Summary("starts a poll!")]
        public async Task StartPoll()
        {
            var pollId = Context.Channel.Id;
            if (_polls.ContainsKey(pollId) && Context.User == _polls[pollId].Owner)
            {
                foreach (var o in _polls[pollId].Options)
                {
                    o.Message = await ReplyAsync($"{o.Text}");
                    await o.Message.AddReactionAsync(Emote.Parse(":thumbsup:"));
                }
                _polls[pollId].IsOpen = true;
            }
            else await ReplyAsync($"no poll ready to start");
        }


        [Command("stop")]
        [Alias("finish", "resolve")]
        [Priority(1)]
        [Summary("gets winner of poll")]
        public async Task StopPoll()
        {
            var pollId = Context.Channel.Id;
            if (_polls.ContainsKey(pollId) && Context.User == _polls[pollId].Owner)
            {
                var poll = _polls[pollId];
                poll.IsOpen = false;
                await ReplyAsync($"the winner was **{poll.Winner}**\nwith {poll.Winner.Votes} votes");
            }
            else await ReplyAsync("you haven't started any polls");
        }

    }
}
