using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Lolobot
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().StartAsync().GetAwaiter().GetResult();

        public static DiscordSocketClient client;
        private CommandHandler _commands;

        public async Task StartAsync()
        {

            Configuration.EnsureExists();                    // Ensure the configuration file has been created.
                                                             // Create a new instance of DiscordSocketClient.
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,              // Specify console verbose information level.
                MessageCacheSize = 1000                      // Tell discord.net how long to store messages (per channel).
            });

            await client.SetGameAsync("with tamers");

            client.Log += (l)                               // Register the console log event.
                => Console.Out.WriteLineAsync(l.ToString());

            await client.LoginAsync(TokenType.Bot, Configuration.Load().Token);
            await client.StartAsync();


            _commands = new CommandHandler();                // Initialize the command handler service
            await _commands.InstallAsync(client);

            await Task.Delay(-1);                            // Prevent the console window from closing.
        }
    }
}