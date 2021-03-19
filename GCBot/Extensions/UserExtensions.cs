using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCBot.Extensions
{
    public static class UserExtensions
    {
        public static bool HasRole(this SocketGuildUser user, SocketRole role)
        {
            foreach (var userRole in user.Roles)
            {
                if (userRole.Id == role.Id)
                    return true;
            }
            return false;
        }

    }
}
