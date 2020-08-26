using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace GCBot
{
    class Program
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly IConfiguration config;
        private readonly IServiceProvider services;
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public Program()
        {
            client = new DiscordSocketClient(
                new DiscordSocketConfig
                {
                    MessageCacheSize = 100,
                });


            commands = new CommandService(
                new CommandServiceConfig
                {
                    CaseSensitiveCommands = false,
                });

            client.Log += Log;
            commands.Log += Log;

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "appsettings.json");

            config = builder.Build();

            services = ConfigureServices();
        }
        public async Task MainAsync()
        {
            await services.GetRequiredService<CommandHandler>().InstallCommandsAsync();

            await client.LoginAsync(TokenType.Bot, config["DiscordToken"]);
            await client.StartAsync();

            var token = new TokenService(config);

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(config)
                .AddSingleton(client)
                .AddSingleton(commands)
                .AddSingleton<CommandHandler>()
                .AddSingleton<TokenService>()
                .BuildServiceProvider()
                ;
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
