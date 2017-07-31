using Discord.Commands;
using Discord;
using Lolobot.Preconditions;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using System;

namespace Lolobot.Modules
{
    [Name("Faq")]
    public class FaqModule : ModuleBase<SocketCommandContext>
    {
        [Command("addfaq")]
        [Remarks("Lets you add a new FAQ. (mchelpers and admins/mods only)")]
        [Alias("newfaq")]
        [MinPermissions(AccessLevel.User)] //*********************** Remember this ^ test
        public async Task waifuinfo([Remainder] IUser user = null)
        {
          
        }

    }


}