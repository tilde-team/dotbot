﻿using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace dotbot.Commands
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IConfigurationRoot _config;

        public Help(CommandService service, IConfigurationRoot config)
        {
            _service = service;
            _config = config;
        }

        [Command("help")]
        public async Task HelpAsync()
        {
            var embed = new EmbedBuilder()
            {
                Title = $"{Context.Client.CurrentUser.Username} Help",
                Color = new Color(114, 137, 218),
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Description = "These are the commands you can use"
            };

            var cmds = new List<string>();
            foreach (var module in _service.Modules)
            {
                string description = "";
                foreach (var cmd in module.Commands)
                {
                    if (cmds.Contains(cmd.Name)) continue;
                    cmds.Add(cmd.Name);
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        description += $"**{_config["prefix"]}{cmd.Aliases.First()}** {cmd.Summary}\n";
                }

                if (!string.IsNullOrWhiteSpace(description))
                    embed.AddField(module.Name, description);
            }

            await ReplyAsync("", embed: embed);
        }


        [Command("help")]
        public async Task HelpAsync(string command)
        {
            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return;
            }

            var embed = new EmbedBuilder()
            {
                Title = $"{Context.Client.CurrentUser.Username} Help",
                Color = new Color(114, 137, 218),
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;
                embed.AddField(
                    $"{_config["prefix"]}{cmd.Aliases.First()}",
                    $"{cmd.Summary}\nArgs: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n{(cmd.Aliases.Count > 1 ? $"Aliases: {string.Join(", ", cmd.Aliases.Skip(1))}" : "")}"
                );
            }

            await ReplyAsync("", embed: embed);
        }

    }
}
