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

        [Command("insult")]
        [Summary("Insults someone or something.")]
        public Task InsultAsync([Remainder][Summary("What you want to insult")] string input)
        {
            if (input.Contains("@"))
                return ReplyAsync($"You can't insult that!");

            return ReplyAsync(GetInsult(input));
        }

        [Command("insult")]
        [Summary("Insults someone or something.")]
        public Task InsultAsync([Remainder][Summary("What you want to insult")] SocketUser user)
        {

            if (user.IsBot)
                return ReplyAsync($"Insulting the Bots, really??");

            return ReplyAsync(GetInsult(user.Mention));
        }

        private string GetInsult(string target)
        {
            var listInsults = new List<string>();
            listInsults.Add("You don't need my help for that. You're managing to insult #target# just fine on your own. Whatever that is...");
            listInsults.Add("#target# sounds like a boy!");
            listInsults.Add("#target# doesn't press T...");
            listInsults.Add("#target#'s mother is a fine lady!");
            listInsults.Add("Can't be bothered, you do it...");
            listInsults.Add("How much will you pay me, to do that?");
            listInsults.Add("Yes AL. Whatever you say AL.");
            var i = (new Random()).Next(listInsults.Count);

            return listInsults[i].Replace("#target#", target);
        }

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
        [Alias("list")]
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

        [Command("add")]
        [Summary("Add someone to your regiment.")]
        public async Task AddToRegimentAsync(
            [Summary("The role name to list")]
            SocketGuildUser user)
        {
            var requestingUser = Context.User;

            #region Representative Officer permissions
            var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Representative Officer");
            if (role == null)
            {
                await ReplyAsync($"This command is temporarily disabled until Agentsvr knows what he's doing.");
                return;
            }
            bool hasPermission = false;
            foreach (var userRole in ((SocketGuildUser)requestingUser).Roles)
            {
                if (userRole.Id == role.Id)
                    hasPermission = true;
            }
            if (!hasPermission)
            {
                await ReplyAsync($"Only Representative Officers can use this command.");
                return;
            }
            #endregion

            #region Load New Regiment
            SocketRole regimentRole = null;
            foreach (var userRole in ((SocketGuildUser)requestingUser).Roles)
            {
                foreach (var regiment in tokens.Tokens)
                {
                    if (userRole.Name == regiment.LongName)
                        regimentRole = userRole;
                }
            }
            if (regimentRole == null)
            {
                await ReplyAsync($"How can you be in a Representative Officer and not be in a recognized Regiment?");
                return;
            }
            #endregion

            #region Check if the user is already in a regiment
            foreach (var userRole in user.Roles)
            {
                foreach (var regiment in tokens.Tokens)
                {
                    if (userRole.Name == regiment.LongName)
                    {
                        await ReplyAsync($"The user you're trying to add to your regiment is already in another regiment. Talk with the representative officer of {regiment.LongName}");
                        return;
                    }
                }
            }
            #endregion

            await user.AddRoleAsync(regimentRole);
            await ReplyAsync($"Added {user.Nickname} to {regimentRole.Name}");
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
                    {
                        usersWithRole.Add(user);
                        break;
                    }
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

        /*
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
        */
        
        [Command("remove")]
        [Summary("Removes your favorite GC Token role.")]
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
                    result += $"{user.Username} no longer belongs to {currentRole.Name}.";
                }
            }
            await ReplyAsync(result);
        }

        [Command("remove")]
        [Summary("Removes your favorite GC Token role.")]
        public async Task RemoveRoleAsync(
            string toDiscard)
        {
            await RemoveRoleAsync();
        }

        [Command("remove")]
        [Summary("Removes someone from your Regiment.")]
        public async Task RemoveRoleAsync(
            SocketGuildUser user)
        {
            var requestingUser = Context.User;

            if (((SocketGuildUser)Context.User) == user)
            {
                await RemoveRoleAsync();
                return;
            }

            #region Representative Officer permissions
            var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Representative Officer");
            if (role == null)
            {
                await ReplyAsync($"This command is temporarily disabled until Agentsvr knows what he's doing.");
                return;
            }
            bool hasPermission = false;
            foreach (var userRole in ((SocketGuildUser)requestingUser).Roles)
            {
                if (userRole.Id == role.Id)
                    hasPermission = true;
            }
            if (!hasPermission)
            {
                await ReplyAsync($"Only Representative Officers can use this command.");
                return;
            }
            #endregion

            #region Load New Regiment
            SocketRole regimentRole = null;
            foreach (var userRole in ((SocketGuildUser)requestingUser).Roles)
            {
                foreach (var regiment in tokens.Tokens)
                {
                    if (userRole.Name == regiment.LongName)
                        regimentRole = userRole;
                }
            }
            if (regimentRole == null)
            {
                await ReplyAsync($"How can you be in a Representative Officer and not be in a recognized Regiment?");
                return;
            }
            #endregion

            foreach (var userRole in user.Roles)
            {
                if (userRole == regimentRole)
                {
                    await (user as SocketGuildUser).RemoveRoleAsync(regimentRole);
                    await ReplyAsync($"Remove the regiment {regimentRole.Name} from {user.Nickname}.");
                    return;
                }
            }

            await ReplyAsync($"{user.Nickname} was not in your regiment {regimentRole.Name} anyway... you're welcome, I guess.");
            return;

        }
    }
}
