using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using GCBot.Extensions;

namespace GCBot.Commands
{
    public class RoleModule : ModuleBase<SocketCommandContext>
    {
        private ListService lists;
        public RoleModule(IServiceProvider services)
        {
            lists = services.GetRequiredService<ListService>();
        }

        [Command("orator")]
        [Summary("Calls !gimme Orator")]
        [Alias("meme","banter")]
        public async Task GiveOratorToSelf()
        {
            await GiveRoleToSelf("Orator");
        }

        [Command("giveme")]
        [Summary("Adds a specific role")]
        [Alias("gimme")]
        public async Task GiveRoleToSelf([Remainder][Summary("The role name from a list")]
            string roleName = null)
        {
            if (Context.Channel.Name != lists.BotChannel)
                return;
            var realRoleName = lists.AutoRoles.FirstOrDefault(x => x.ToUpperInvariant() == roleName.ToUpperInvariant());
            if (string.IsNullOrWhiteSpace(realRoleName))
            {
                await ReplyAsync($"That role is not available to pick with this command. Current roles available: {String.Join(", ",lists.AutoRoles)}.");
                return;
            }
            var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == realRoleName);
            if (role == null)
            {
                await ReplyAsync($"Role could not be found {realRoleName}.");
                return;
            }

            var user = (SocketGuildUser)Context.User;
            await user.AddRoleAsync(role);
            await ReplyAsync($"Added {role.Name} to {user.Username}");
        }

        [Command("takefromme")]
        [Summary("Removes a specific role")]
        [Alias("take")]
        public async Task TakeRoleFromSelf([Remainder][Summary("The role name from a list")]
            string roleName = null)
        {
            if (Context.Channel.Name != lists.BotChannel)
                return;

            var realRoleName = lists.AutoRoles.FirstOrDefault(x => x.ToUpperInvariant() == roleName.ToUpperInvariant());

            if (string.IsNullOrWhiteSpace(realRoleName))
            {
                await ReplyAsync($"That role is not available to pick with this command. Current roles available: {String.Join(", ", lists.AutoRoles)}.");
                return;
            }
            var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == realRoleName);
            if (role == null)
            {
                await ReplyAsync($"Role could not be found {realRoleName}.");
                return;
            }

            var user = (SocketGuildUser)Context.User;
            await user.RemoveRoleAsync(role);
            await ReplyAsync($"Removed {role.Name} from {user.Username}");
        }

        [Command("listregiments")]
        [Summary("Returns list of available regiments.")]
        [Alias("list")]
        public async Task ListRolesAsync()
        {
            if (Context.Channel.Name != lists.BotChannel)
                return;

            string rolesList = "";
            foreach (var token in lists.Regiments)
            {
                if (rolesList != "")
                    rolesList += ", ";
                rolesList += token.ShortName;
            }
            await ReplyAsync($"The following regiments are available: {rolesList}.");

            return;
        }

        [Command("addstrategy")]
        [Summary("Add someone to strategy channel.")]
        [Alias("strategy", "addstrat")]
        public async Task AddToStrategyAsync(
            [Summary("The user to add to strategy channel")]
            SocketGuildUser user
            )
        {
            if (Context.Channel.Name != lists.BotChannel)
                return;
            var requestingUser = Context.User;

            // User requesting has role?
            SocketRole foundStrategyRole = null;
            foreach(var strategyRole in lists.StrategyRoles)
            {
                var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == strategyRole);
                foreach (var userRole in ((SocketGuildUser)requestingUser).Roles)
                {
                    if (userRole.Id == role.Id)
                        foundStrategyRole = role;
                }
            }

            if (foundStrategyRole== null)
            {
                await ReplyAsync($"You need to have a strategy role to be able to add someone to a strategy role.");
                return;
            }

            await user.AddRoleAsync(foundStrategyRole);
            await ReplyAsync($"Added {user.Mention} to {foundStrategyRole.Name}");
            return;
        }

        [Command("remstrategy")]
        [Summary("Remove someone from strategy channel.")]
        [Alias("removestrategy","remstrat")]
        public async Task RemoveFromStrategyAsync(
        [Summary("The user to remove from strategy channel")]
            SocketGuildUser user
            )
        {
            if (Context.Channel.Name != lists.BotChannel)
                return;
            var requestingUser = Context.User;

            // User requesting has role?
            SocketRole foundStrategyRole = null;
            foreach (var strategyRole in lists.StrategyRoles)
            {
                var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == strategyRole);
                foreach (var userRole in ((SocketGuildUser)requestingUser).Roles)
                {
                    if (userRole.Id == role.Id)
                        foundStrategyRole = role;
                }
            }

            if (foundStrategyRole == null)
            {
                await ReplyAsync($"You need to have a strategy role to be able to remove someone from a strategy role.");
                return;
            }

            await user.RemoveRoleAsync(foundStrategyRole);
            await ReplyAsync($"Removed {user.Mention} from {foundStrategyRole.Name}");
            return;
        }


        [Command("add")]
        [Summary("Add someone to your regiment.")]
        public async Task AddToRegimentAsync(
            [Summary("The user to add to your regiment")]
            SocketGuildUser user)
        {
            if (Context.Channel.Name != lists.BotChannel)
                return;

            var requestingUser = Context.User;

            #region Representative Officer permissions
            var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == lists.RegimentalAdminRole);
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
            Regiment regiment = null;
            foreach (var userRole in ((SocketGuildUser)requestingUser).Roles)
            {
                foreach (var r in lists.Regiments)
                {
                    if (userRole.Name == r.LongName)
                    {
                        regimentRole = userRole;
                        regiment = r;
                    }
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
                foreach (var r in lists.Regiments)
                {
                    if (userRole.Name == r.LongName)
                    {
                        await ReplyAsync($"The user you're trying to add to your regiment is already in another regiment. Talk with the representative officer of {userRole.Name}");
                        return;
                    }
                }
            }
            #endregion

            var userMention = string.IsNullOrWhiteSpace(user.Nickname) ? user.Mention : user.Nickname;

            if (regiment.Faction != Faction.Unaligned)
            {
                var factionRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == regiment.Faction.ToString());
                await user.AddRoleAsync(factionRole);
                await ReplyAsync($"Added {userMention} to {factionRole.Name}");
            }

            await user.AddRoleAsync(regimentRole);
            await ReplyAsync($"Added {userMention} to {regimentRole.Name}");
            return;

        }

        [Command("listusers")]
        [Summary("Returns list of users in a given Regiment.")]
        public async Task ListUsersByRoleAsync(
            [Remainder][Summary("The role name to list")]
            string roleName = null)
        {
            if (Context.Channel.Name != lists.BotChannel)
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

        [Command("leave")]
        [Summary("Leave your regiment.")]
        [Alias("remove")]
        public async Task RemoveRoleAsync()
        {
            if (Context.Channel.Name != lists.BotChannel)
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
                if (Enum.GetNames(typeof(Faction)).Contains(currentRole.Name))
                {
                    await (user as SocketGuildUser).RemoveRoleAsync(currentRole);
                    if (result != "")
                        result += "\r\n";
                    result += $"{user.Username} no longer belongs to {currentRole.Name}.";
                }
            }
            await ReplyAsync(result);
        }

        [Command("leave")]
        [Summary("Leave your regiment.")]
        [Alias("remove")]
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
            if (Context.Channel.Name != lists.BotChannel)
                return;

            var requestingUser = Context.User;

            if (((SocketGuildUser)requestingUser) == user)
            {
                await RemoveRoleAsync();
                return;
            }

            #region Representative Officer permissions
            var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == lists.RegimentalAdminRole);
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
            Regiment regiment = null;
            foreach (var userRole in ((SocketGuildUser)requestingUser).Roles)
            {
                foreach (var r in lists.Regiments)
                {
                    if (userRole.Name == r.LongName)
                    {
                        regimentRole = userRole;
                        regiment = r;
                    }
                }
            }
            if (regimentRole == null)
            {
                await ReplyAsync($"How can you be in a Representative Officer and not be in a recognized Regiment?");
                return;
            }
            #endregion

            bool wasInRegiment = false;
            foreach (var userRole in user.Roles)
            {
                if (userRole == regimentRole)
                {
                    await (user as SocketGuildUser).RemoveRoleAsync(regimentRole);
                    await ReplyAsync($"Remove the regiment {regimentRole.Name} from {user.Mention}.");
                    wasInRegiment = true;
                }
                else if (userRole.Name == regiment.Faction.ToString())
                {
                    await (user as SocketGuildUser).RemoveRoleAsync(userRole);
                    await ReplyAsync($"Remove the faction {regiment.Faction.ToString()} from {user.Mention}.");
                }
            }
            if (wasInRegiment)
            {
                return;
            }

            await ReplyAsync($"{user.Nickname} was not in your regiment {regimentRole.Name} anyway... you're welcome, I guess.");
            return;

        }

        [Command("tag")]
        [Summary("Fix tags for someone in your Regiment.")]
        public async Task ChangeNickname(
            [Summary("User whose tags you want to change")] SocketGuildUser user, 
            [Remainder][Summary("New tags for that user")] string tag)
        {
            if (Context.Channel.Name != lists.BotChannel)
                return;

            var tagLimitation = 32;
            if (tag.Length>32)
            {
                tag = tag.Substring(0, tagLimitation);
                await ReplyAsync($"Tag is longer than {tagLimitation} characters, and will be truncated to:{tag}");
            }

            if(user == ((SocketGuildUser)Context.User))
            {
                await ReplyAsync($"You're too fancy to use 'Change Nickname' are you?");
                return;
            }

            var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == lists.RegimentalAdminRole);
            if (role == null)
            {
                await ReplyAsync($"This command is temporarily disabled until Agentsvr knows what he's doing.");
                return;
            }

            var requestingUser = Context.User;
            if (!((SocketGuildUser)requestingUser).HasRole(role))
            {
                await ReplyAsync($"Only {lists.RegimentalAdminRole}s can use this command.");
                return;
            }

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
                await ReplyAsync($"How can you be in a {lists.RegimentalAdminRole} and not be in a recognized Regiment?");
                return;
            }

            var userBelongsToRegiment = false;
            foreach (var userRole in user.Roles)
            {
                if (userRole == regimentRole)
                {
                    userBelongsToRegiment = true;
                }
            }
            if (!userBelongsToRegiment)
            {
                await ReplyAsync($"You can't change a tag for someone that is not in your Regiment.");
                return;
            }

            var oldName = user.Nickname;
            await user.ModifyAsync(x => { x.Nickname = tag; });
            await ReplyAsync($"{oldName} tags changed to:{tag}");
        }
    }
}
