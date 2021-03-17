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
        => ReplyAsync($"Ain't your slave. You say: \"{echo.Replace("@", "")}\" if You want.");

        [Command("insult")]
        [Summary("Insults someone or something.")]
        public Task InsultAsync([Remainder][Summary("What you want to insult")] string target)
        => ReplyAsync($"You don't need my help for that. You're managing to insult {target.Replace("@","")} just fine on your own.");

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

        [Command("listroles")]
        [Summary("Returns list of available roles.")]
        public async Task ListRolesAsync()
        {
            string rolesList = "";
            foreach (var token in tokens.Tokens)
            {
                if (rolesList != "")
                    rolesList += ", ";
                rolesList += token.ShortName;
            }
            await ReplyAsync($"The following roles are available: {rolesList}.");

            return;
        }

        [Command("listusers")]
        [Summary("Returns list of users with given role.")]
        public async Task ListUsersByRoleAsync(
            [Summary("The role name to list")]
            string roleName = null)
        {
            var requestedToken = tokens.Tokens.FirstOrDefault(t => t.ShortName == roleName || t.LongName == roleName);
            if (requestedToken == null)
            {
                string rolesList = "";
                foreach (var token in tokens.Tokens)
                {
                    if (rolesList != "")
                        rolesList += ", ";
                    rolesList += token.ShortName;
                }
                await ReplyAsync($"{roleName.Replace("@", "")} is not a Role you can select through this command.\r\nThe following roles are available: {rolesList}.");

                return;
            }

            var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == requestedToken.LongName);

            if (role == null)
            {
                await ReplyAsync($"{roleName.Replace("@", "")} seems to be improperly created, you should complain to the Game Master.");
                return;
            }

            //var users = Context.Guild.Users;
            var guild = Context.Guild;
            //await guild.DownloadUsersAsync();

            if (!guild.HasAllMembers)
            {
                await ReplyAsync($"Discord didn't let us download the entire user list. This command is out of commission until this works again. Sorry. (HasAllMembers:false)");
                return;
            }

            var users = guild.Users;

            var usersWithRole = new List<SocketGuildUser>();

            foreach (var user in users)
            {
                foreach (var userRole in user.Roles)
                {
                    if (userRole.Id == role.Id)
                        usersWithRole.Add(user);
                }
            }

            string usersList = "";
            foreach (var user in usersWithRole)
            {
                usersList += "\r\n";
                usersList += user.Nickname ?? user.Username + "#" + user.Discriminator;
            }
            await ReplyAsync($"{role.Name} has the following users: {usersList}");
        }

        [Command("addrole")]
        [Summary("Gives you a role so you can show your affection towards your favorite GC Token.")]
        [Alias("support", "token")]
        public async Task GetRoleAsync(
            [Summary("Your favorite GC token")]
            string input)
        {
            var user = Context.User;

            var requestedToken = tokens.Tokens.FirstOrDefault(t => t.ShortName == input || t.LongName == input);
            if (requestedToken == null)
            {
                string rolesList = "";
                foreach (var token in tokens.Tokens)
                {
                    if (rolesList != "")
                        rolesList += ", ";
                    rolesList += token.ShortName;
                }
                await ReplyAsync($"{input.Replace("@", "")} is not a Role you can select through this command.\r\nThe following roles are available: {rolesList}.");

                return;
            }

            var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == requestedToken.LongName);

            if (role == null)
            {
                await ReplyAsync($"{input.Replace("@", "")} seems to be improperly created, you should complain to the Game Master.");
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

        [Command("remrole")]
        [Summary("Removes your favorite GC Token role.")]
        [Alias("remsupport", "remtoken")]
        public async Task RemoveRoleAsync()
        {
            var user = Context.User;
            string result = "";
            foreach (var currentRole in (user as SocketGuildUser).Roles)
            {
                if (tokens.Tokens.FirstOrDefault(t => t.LongName == currentRole.Name) != null)
                {
                    await (user as SocketGuildUser).RemoveRoleAsync(currentRole);
                    if (result != "")
                        result += "\r\n";
                    result += $"{user.Username} no longer supports {currentRole.Name}.";
                }
            }
            await ReplyAsync(result);
        }
        [Command("remrole")]
        [Summary("Removes your favorite GC Token role.")]
        [Alias("remsupport", "remtoken")]
        public async Task RemoveRoleAsync(
            string toDiscard)
        {
            await RemoveRoleAsync();
        }
    }
}
