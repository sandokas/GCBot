using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;

namespace GCBot.Commands
{

    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private ListService lists;
        private static CommandService commandService;
        public HelpModule(IServiceProvider services)
        {
            commandService = services.GetRequiredService<CommandService>();
            lists = services.GetRequiredService<ListService>();
        }

        [Command("Help")]
        public async Task Help()
        {
            if (Context.Channel.Name != lists.BotChannel)
                return;

            List<CommandInfo> commands = commandService.Commands.ToList();
            EmbedBuilder embedBuilder = new EmbedBuilder();

            foreach (CommandInfo command in commands)
            {
                // Get the command Summary attribute information
                string embedFieldText = command.Summary ?? "No description available\n";

                embedBuilder.AddField(command.Name, embedFieldText);
            }

            await ReplyAsync("Here's a list of commands and their description: ", false, embedBuilder.Build());
        }
    }
}
