using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GCBot
{
    using Discord;
    using Discord.Commands;
    using Discord.Rest;
    using Discord.WebSocket;
    using Microsoft.Extensions.DependencyInjection;
    using System.Threading.Tasks;

    // Keep in mind your module **must** be public and inherit ModuleBase.
    // If it isn't, it will not be discovered by AddModulesAsync!
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private TokenService tokens;
        public InfoModule(IServiceProvider services)
        {
            tokens = services.GetRequiredService<TokenService>();
        }

        [Command("say")]
        [Summary("Echoes a message.")]
        public Task SayAsync([Remainder][Summary("The text to echo")] string echo)
        => ReplyAsync($"Ain't your slave. You say: \"{echo}\" if You want.");

        [Command("userinfo")]
        [Summary("Returns info about the current user, or the user parameter, if one passed.")]
        [Alias("user", "whois")]
        public async Task UserInfoAsync(
            [Summary("The (optional) user to get info from")]
            SocketUser user = null)
        {
            var userInfo = user ?? Context.Client.CurrentUser;

            await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
        }
        [Command("getrole")]
        [Summary("Gives you a role so you can show your affection towards your favorite GC Token.")]
        [Alias("role", "token")]
        public async Task GetRoleAsync(
            [Summary("Your favorite GC token")]
            string input)
        {
            var user = Context.User;

            var requestedToken = tokens.Tokens.FirstOrDefault(t => t.ShortName == input || t.LongName == input);
            if  (requestedToken == null)
            {
                await ReplyAsync($"{input} is not a Role you can select through this command."); 
                return;
            }

            var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == requestedToken.LongName);

            if (role == null)
            {
                await ReplyAsync($"{input} is not a Role you can select through this command.");
                return;
            }

            string result = "";

            foreach (var currentRole in (user as SocketGuildUser).Roles)
            {
                if (currentRole.Name == requestedToken.LongName)
                {
                    result += $"{user.Username} already supports {requestedToken.LongName}.";
                    await ReplyAsync(result);
                    return;
                }
                if (tokens.Tokens.FirstOrDefault(t => t.LongName == currentRole.Name) != null)
                {
                    await (user as SocketGuildUser).RemoveRoleAsync(currentRole);
                    result += $"{user.Username} no longer supports {currentRole.Name}.\r\n";
                }
            }

            await (user as SocketGuildUser).AddRoleAsync(role);

            result += $"{user.Username} now supports {requestedToken.LongName}.";
            await ReplyAsync(result);
        }
    }

}
