﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace GCBot.Commands
{

    // Keep in mind your module **must** be public and inherit ModuleBase.
    // If it isn't, it will not be discovered by AddModulesAsync!
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        private ListService lists;
        public FunModule(IServiceProvider services)
        {
            lists = services.GetRequiredService<ListService>();
        }

        [Command("hello")]
        [Summary("Says hi to the bot.")]
        public async Task HelloAsync()
        {
            if (Context.Channel.Name != lists.BotChannel)
                return;
            await ReplyAsync($"Oh. Hello, {Context.User.Mention}!");
            return;
        }

        private bool HasChoiceBeenMade(string choice)
        {
            if (lists.Chosen.Where(x => x == choice.ToUpperInvariant().Trim()).Any())
                return true;
            lists.Chosen.Add(choice.ToUpperInvariant().Trim());

            return false;
        }
        [Command("Choose")]
        [Summary("Choose an option.")]
        public async Task ChooseAsync([Remainder] string choice)
        {
            if (Context.Channel.Name != lists.BotChannel)
                return;

            if (HasChoiceBeenMade(choice))
            {
                await ReplyAsync($"I've already choosen between {choice}. Not picking THAT again.");
                return;
            }    

            if (!choice.ToUpperInvariant().Contains(" OR "))
            {
                await ReplyAsync($"Options need to be separated by \" OR \"");
                return;
            }

            var options = choice.ToUpperInvariant().Split(" OR ");

            for (int i = 1; i < options.Length; i++)
            {
                if(options[i].Trim() == options[i-1].Trim())
                {
                    await ReplyAsync($"Those are the same, you're trying to trick me!");
                    return;
                }

            }

            var chosen = options[(new Random()).Next(options.Count())];
            var n = (new Random()).Next(lists.Choices.Count);
            await ReplyAsync(lists.Choices[n].Replace("#choice#", chosen));
            return;
        }

        [Command("bye")]
        [Summary("Say goodbye to the bot.")]
        [Alias("goodbye")]
        public async Task ByeAsync()
        {
            if (Context.Channel.Name != lists.BotChannel)
                return;
            await ReplyAsync($"Goodbye, {Context.User.Mention}. Have fun!");
            return;
        }

        [Command("praise")]
        [Summary("Praises someone or something.")]
        public async Task PraiseAsync([Remainder][Summary("What you want to praise")] string input)
        {
            if (Context.Channel.Name != lists.BotChannel)
                return;

            if (input.Contains("@"))
            {
                await ReplyAsync($"You can't praise that!");
                return;
            }

            if(input.ToLowerInvariant().Trim() == "you" || input.ToLowerInvariant().Trim() == "yourself")
            {
                await ReplyAsync($"Oh, thank you kind sir!");
                return;
            }

            if (input.ToLowerInvariant().Trim() == "me")
            {
                await ReplyAsync($"Looking for some menaningless boost to self esteem are we?");
                return;
            }

            await ReplyAsync(GetPraise(input, Context.User));
            return;
        }

        [Command("praise")]
        [Summary("Praises someone or something.")]
        public async Task PraiseAsync([Remainder][Summary("What you want to praise")] SocketUser user)
        {
            if (Context.Channel.Name != lists.BotChannel)
                return;

            if (user.IsBot)
            {
                await ReplyAsync($"I know, we bots are the best, aren't we?");
                return;
            }

            if (user == Context.User)
            {
                await ReplyAsync($"Looking for some meaningless boost to self esteem are we?");
                return;
            }

            await ReplyAsync(GetPraise(user.Mention, Context.User));
            return;
        }
        private string GetPraise(string target, SocketUser user)
        {
            var i = (new Random()).Next(lists.Praises.Count);

            return lists.Praises[i].Replace("#target#", target).Replace("#user#", user.Mention);
        }

        [Command("insult")]
        [Summary("Insults someone or something.")]
        public async Task InsultAsync([Remainder][Summary("What you want to insult")] string input)
        {
            if (Context.Channel.Name != lists.BotChannel)
                return;

            if (input.Contains("@"))
            {
                await ReplyAsync($"You can't insult that!");
                return;
            }

            if (input.ToLowerInvariant().Trim() == "you" || input.ToLowerInvariant().Trim() == "yourself")
            {
                await ReplyAsync($"We're back to insulting the bots, are we?");
                return;
            }

            if (input.ToLowerInvariant().Trim() == "me" || input.ToLowerInvariant().Trim() == "myself")
            {
                await ReplyAsync($"Oh. Come on. You're not that bad! Here, have a cookie.");
                return;
            }


            await ReplyAsync(GetInsult(input, Context.User));
            return;
        }

        [Command("insult")]
        [Summary("Insults someone or something.")]
        public async Task InsultAsync([Remainder][Summary("What you want to insult")] SocketUser user)
        {
            if (Context.Channel.Name != lists.BotChannel)
                return;

            if (user.IsBot)
            {
                await ReplyAsync ($"Insulting the Bots, really??");
                return;
            }

            if (user == Context.User)
            {
                await ReplyAsync($"Oh. Come on. You're not that bad! Here, have a cookie.");
                return;
            }

            await ReplyAsync(GetInsult(user.Mention, Context.User));
            return;
        }

        private string GetInsult(string target, SocketUser user)
        {
            var i = (new Random()).Next(lists.Insults.Count);

            return lists.Insults[i].Replace("#target#", target).Replace("#user#", user.Mention);
        }
    }
}
