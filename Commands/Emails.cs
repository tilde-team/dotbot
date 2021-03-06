﻿using Discord;
using Discord.Commands;
using dotbot.Core;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace dotbot.Commands
{
    [Group("email")]
    public class Emails : ModuleBase<SocketCommandContext>
    {
        public DotbotDb db { get; set; }
        private NetworkCredential EmailCredential;

        public Emails(IConfigurationRoot config)
        {
            EmailCredential = new NetworkCredential(config["gmail_login"], config["tokens:gmail"]);
        }


        [Command]
        public async Task SendEmail(
            [Summary("user to send to")] IUser recipient,
            [Summary("message to send")] [Remainder] string message
        ) {
            if (!db.Emails.Any(e => e.Id == recipient.Id))
            {
                await ReplyAsync($"{recipient.Mention} does not have a saved email");
                return;
            }

            var status = await ReplyAsync($"sending message");

            (new SmtpClient("smtp.gmail.com")
            {
                EnableSsl   = true,
                Port        = 587,
                Credentials = EmailCredential,
            })
            .Send(
                from:       $"{Context.User.Username}-{Context.Guild.Name}@benbot.tilde.team",
                recipients: db.Emails.Find(recipient.Id).EmailAddress,
                subject:    $"benbot message from {Context.User.Username}",
                body:       message
            );

            await Context.Message.DeleteAsync();
            await status.ModifyAsync(m => m.Content = $"message sent to {recipient.Mention}!");
        }


        [Command("save")]
        [Summary("saves an email address to your profile")]
        public async Task SaveEmail([Summary("email to save")] string email)
        {
            var id = Context.User.Id;
            await Context.Message.DeleteAsync();
            if (db.Emails.Any(e => e.Id == id))
                db.Emails.Find(id).EmailAddress = email;
            else
                db.Emails.Add(new Email
                {
                    Id = id,
                    EmailAddress = email
                });
            db.SaveChanges();
            await ReplyAsync("your email has been saved");
        }


        [Command("show")]
        [Summary("shows your saved email address")]
        public async Task ShowEmail()
        {
            await Context.Message.DeleteAsync();
            await ReplyAsync($"{Context.User.Mention}, your saved email is {db.Emails.Find(Context.User.Id)?.EmailAddress ?? "non existent"}");
        }

    }
}
