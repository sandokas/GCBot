using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace GCBot
{

    // Keep in mind your module **must** be public and inherit ModuleBase.
    // If it isn't, it will not be discovered by AddModulesAsync!
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private ListService lists;
        private string botChannel = "bot-commands";
        public InfoModule(IServiceProvider services)
        {
            lists = services.GetRequiredService<ListService>();
        }

        [Command("hello")]
        [Summary("Says hi to the bot.")]
        public async Task HelloAsync()
        {
            if (Context.Channel.Name != botChannel)
                return;
            await ReplyAsync($"Oh. Hello, {Context.User.Mention}!");
            return;
        }

        [Command("bye")]
        [Summary("Say goodbye to the bot.")]
        [Alias("goodbye")]
        public async Task ByeAsync()
        {
            if (Context.Channel.Name != botChannel)
                return;
            await ReplyAsync($"Goodbye, {Context.User.Mention}. Have fun!");
            return;
        }

        [Command("praise")]
        [Summary("Praises someone or something.")]
        public async Task PraisetAsync([Remainder][Summary("What you want to praise")] string input)
        {
            if (Context.Channel.Name != botChannel)
                return;

            if (input.Contains("@"))
            {
                await ReplyAsync($"You can't praise that!");
                return;
            }


            await ReplyAsync(GetPraise(input, Context.User));
            return;
        }

        [Command("praise")]
        [Summary("Praises someone or something.")]
        public async Task PraiseAsync([Remainder][Summary("What you want to praise")] SocketUser user)
        {
            if (Context.Channel.Name != botChannel)
                return;

            if (user.IsBot)
            {
                await ReplyAsync($"I know, we bots are the best, aren't we?");
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
            if (Context.Channel.Name != botChannel)
                return;

            if (input.Contains("@"))
            {
                await ReplyAsync($"You can't insult that!");
                return;
            }

            
            await ReplyAsync(GetInsult(input, Context.User));
            return;
        }

        [Command("insult")]
        [Summary("Insults someone or something.")]
        public async Task InsultAsync([Remainder][Summary("What you want to insult")] SocketUser user)
        {
            if (Context.Channel.Name != botChannel)
                return;

            if (user.IsBot)
            {
                await ReplyAsync ($"Insulting the Bots, really??");
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

        [Command("listroles")]
        [Summary("Returns list of available roles.")]
        [Alias("list")]
        public async Task ListRolesAsync()
        {
            if (Context.Channel.Name != botChannel)
                return;

            string rolesList = "";
            foreach (var token in lists.Regiments)
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
            if (Context.Channel.Name != botChannel)
                return;

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
                foreach (var regiment in lists.Regiments)
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
                foreach (var regiment in lists.Regiments)
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
            [Remainder][Summary("The role name to list")]
            string roleName = null)
        {
            if (Context.Channel.Name != botChannel)
                return;

            var requestedToken = lists.Regiments.FirstOrDefault(t => t.ShortName == roleName || t.LongName == roleName);
            if (requestedToken == null)
            {
                string rolesList = "";
                foreach (var token in lists.Regiments)
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

        [Command("remove")]
        [Summary("Removes your favorite GC Token role.")]
        [Alias("leave")]
        public async Task RemoveRoleAsync()
        {
            if (Context.Channel.Name != botChannel)
                return;

            var user = Context.User;
            string result = "";
            foreach (var currentRole in (user as SocketGuildUser).Roles)
            {
                if (lists.Regiments.FirstOrDefault(t => t.LongName == currentRole.Name) != null)
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
        [Alias("leave")]
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
            if (Context.Channel.Name != botChannel)
                return;

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
                foreach (var regiment in lists.Regiments)
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
