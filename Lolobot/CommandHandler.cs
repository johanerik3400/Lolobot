using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Lolobot
{
    /// <summary> Detect whether a message is a command, then execute it. </summary>
    public class CommandHandler
    {
        private DiscordSocketClient _client;
        private CommandService _cmds;

        public async Task InstallAsync(DiscordSocketClient c)
        {
            _client = c;                                                 // Save an instance of the discord client.
            _cmds = new CommandService();                                // Create a new instance of the commandservice.                              

            await _cmds.AddModulesAsync(Assembly.GetEntryAssembly());    // Load all modules from the assembly.

            _client.MessageReceived += HandleCommandAsync;               // Register the messagereceived event to handle commands.
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null)                                          // Check if the received message is from a user.
                return;

            var context = new SocketCommandContext(_client, msg);     // Create a new command context.

            int argPos = 0;                                           // Check if the message has either a string or mention prefix.
            if (msg.HasStringPrefix(Configuration.Load().Prefix, ref argPos) ||
                msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {                                                         // Try and execute a command with the given context.
                var result = await _cmds.ExecuteAsync(context, argPos);
                
                if (!result.IsSuccess)                                // If execution failed, reply with the error message.
                {
                    string firstWord = msg.ToString().Split(' ').First().Substring(1);
                    // string firstErrorMsg = result.ToString().ToString().Split(' ').First();
                    var eb = new EmbedBuilder();
                    eb.WithColor(0xFF0000);
                    if(result.ToString() == "MultipleMatches: Multiple matches found.") // All of this is temporary, should make a better solution later (I hope)
                    {
                        eb.WithDescription("There are multiple users with that name! Use @[user] to specify which one.");
                    }
                    else
                    {
                        if (firstWord == "claim")
                        {
                            if (result.ToString() == "ParseFailed: Failed to parse Int32")
                                eb.WithDescription("!claim [amount] [user]\nAmount has to be positive integer");
                            else if (result.ToString() == "ObjectNotFound: User not found.")
                                eb.WithDescription("!claim [amount] [user]\nUnknown user");
                            else if (result.ToString() == "BadArgCount: The input text has too few parameters.")
                                eb.WithDescription("!claim [amount] [user]\nEnter amount and user");
                            else
                                eb.WithDescription("!claim [amount] [user]\nEnter a user");
                        }
                        else if (firstWord == "affinity")
                        {
                            if (result.ToString() == "ObjectNotFound: User not found.")
                                eb.WithDescription("!affinity [user]\nUnknown user");
                        }
                        else if (firstWord == "divorce")
                        {
                            if (result.ToString() == "ObjectNotFound: User not found.")
                                eb.WithDescription("!divorce [user]\nUnknown user");
                            else if (result.ToString() == "BadArgCount: The input text has too few parameters.")
                                eb.WithDescription("!divorce [user]\nEnter user");
                        }
                        else if (firstWord == "waifuinfo")
                        {
                            if (result.ToString() == "ObjectNotFound: User not found.")
                                eb.WithDescription("!waifuinfo [user]\nUnknown user");
                        }
                        else if (firstWord == "br")
                        {
                            if (result.ToString() == "ParseFailed: Failed to parse Int32")
                                eb.WithDescription("!br [amount]\nAmount has to be positive integer");
                            else if (result.ToString() == "BadArgCount: The input text has too few parameters.")
                                eb.WithDescription("!br [amount]\nEnter amount");
                            else if (result.ToString() == "BadArgCount: The input text has too many parameters.")
                                eb.WithDescription("!br [amount]");
                        }
                        else if (firstWord == "bf")
                        {
                            if (result.ToString() == "ParseFailed: Failed to parse Int32")
                                eb.WithDescription("!bf [amount] [Zag or Zig]\nAmount has to be positive integer");
                            else if (result.ToString() == "BadArgCount: The input text has too few parameters.")
                                eb.WithDescription("!bf [amount] [Zag or Zig]\nEnter Zag or Zig after amount");
                            else if (result.ToString() == "BadArgCount: The input text has too many parameters.")
                                eb.WithDescription("!bf [amount] [Zag or Zig]");

                        }
                        else if (firstWord == "give")
                        {
                            if (result.ToString() == "ParseFailed: Failed to parse Int32")
                                eb.WithDescription("!give [amount] [user]\nAmount has to be positive integer");
                            else if (result.ToString() == "ObjectNotFound: User not found.")
                                eb.WithDescription("!give [amount] [user]\nUnknown user");
                            else if (result.ToString() == "BadArgCount: The input text has too few parameters.")
                                eb.WithDescription("!give [amount] [user]\nEnter amount and user");
                        }
                        else if (firstWord == "lolos")
                        {
                            if (result.ToString() == "ObjectNotFound: User not found.")
                                eb.WithDescription("!lolos [user]\nUnknown user");
                        }
                        else if (firstWord == "award")
                        {
                            if (result.ToString() == "ParseFailed: Failed to parse Int32")
                                eb.WithDescription("!award [amount] [user]\nAmount has to be positive integer");
                            else if (result.ToString() == "ObjectNotFound: User not found.")
                                eb.WithDescription("!award [amount] [user]\nUnknown user");
                            else if (result.ToString() == "BadArgCount: The input text has too few parameters.")
                                eb.WithDescription("!award [amount] [user]\nEnter amount and user");
                        }
                        else
                        {
                            if (result.ToString() == "UnknownCommand: Unknown command.")
                                eb.WithDescription("Unknown command, use **!help** to get a list of commands");
                            else
                                eb.WithDescription(result.ToString());
                        }
                    }      
                    await context.Channel.SendMessageAsync("", false, eb);

                }
                    
            }
        }
    }
}