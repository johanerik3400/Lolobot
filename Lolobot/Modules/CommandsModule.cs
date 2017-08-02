using Discord.Commands;
using Discord;
using Lolobot.Preconditions;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using System;

namespace Lolobot.Modules
{
    [Name("Commands")]
    public class CommandsModule : ModuleBase<SocketCommandContext>
    {
        private CommandService _service;

        public CommandsModule(CommandService service)           /* Create a constructor for the commandservice dependency */
        {
            _service = service;
        }

        [Command("help")]
        [Remarks("Shows a list of all available commands per module.")]
        public async Task HelpAsync()
        {
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync(); /* A channel is created so that the commands will be privately sent to the user, and not flood the chat. */

            Console.WriteLine($"help: [{Context.User}] did help");

            var builder = new EmbedBuilder()
            {
                Color = new Color(255, 105, 180),
                Description = "These are the commands you can use.\nFor information about specific commands do !help [command]\nExample: !help claim"
            };

            foreach (var module in _service.Modules) /* we are now going to loop through the modules taken from the service we initiated earlier ! */
            {
                string description = null;
                foreach (var cmd in module.Commands) /* and now we loop through all the commands per module aswell, oh my! */
                {
                    var result = await cmd.CheckPreconditionsAsync(Context); /* gotta check if they pass */
                    if (result.IsSuccess)
                    {
                        description += $"{Configuration.Load().Prefix}{cmd.Aliases.First()}\n"; /* if they DO pass, we ADD that command's first alias (aka it's actual name) to the description tag of this embed */
                    }
                        
                }

                if (!string.IsNullOrWhiteSpace(description)) /* if the module wasn't empty, we go and add a field where we drop all the data into! */
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }
            await dmChannel.SendMessageAsync("", false, builder.Build()); /* then we send it to the user. */
        }

        [Command("help")]
        [Remarks("Shows what a specific command does and what parameters it takes.")]
        public async Task HelpAsync(string command)
        {
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
            var result = _service.Search(Context, command);

            Console.WriteLine($"help: [{Context.User}] did help for [{command}]");

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return;
            }

            var builder = new EmbedBuilder()
            {
                Color = new Color(255, 105, 180),
                Description = $"Here are some commands like **{command}**"
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                                $"Remarks: {cmd.Remarks}";
                    x.IsInline = false;
                });
            }
            await dmChannel.SendMessageAsync("", false, builder.Build());
        }

        [Command("StreamerTest")]
        [Remarks("StreamerTest")]
        public async Task StreamerTest(IUser user)
        {
            if(user.Game.GetValueOrDefault().StreamType == StreamType.Twitch)
            {
                await ReplyAsync($"User is streaming on {user.Game.GetValueOrDefault().StreamType}");
            }
            else
            {
                await ReplyAsync($"User is not streaming: {user.Game.GetValueOrDefault().StreamType}");
            }
            /*var eb = new EmbedBuilder();
            eb.WithColor(0xFF69B4);

            eb.WithAuthor("Girl Rating For Agnese Sanctis#5694");
            eb.AddField("**Hot**","9.88", true);
            eb.AddField("**Crazy**","6.88", true);
            eb.AddField("**Advice**","Above an 8 hot, and between about 7 and a 5 crazy - this is WIFE ZONE. If you meet this girl, you should consider long-term relationship.Rare.", true);
            eb.WithImageUrl("https://puu.sh/wcOqN/aec0cd6536.jpg");

            await ReplyAsync("", false, eb);*/
        
    }
    }

}


