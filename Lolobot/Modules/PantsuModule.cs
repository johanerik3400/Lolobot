using Discord.Commands;
using Discord;
using Lolobot.Preconditions;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using System;

namespace Lolobot.Modules
{
    [Name("Lolos")]
    public class PantsuModule : ModuleBase<SocketCommandContext>
    {
        [Command("award")]
        [Remarks("Awards user with Lolos (Server owner only).")]
        //[MinPermissions(AccessLevel.ServerOwner)] ------------------------ REMEMBER THIS
        [MinPermissions(AccessLevel.User)]
        public async Task award(int amount, [Remainder] IGuildUser user)
        {
            Database.AddLolos(user, amount);

            var eb = new EmbedBuilder();
            eb.WithColor(0xFF69B4);
            Console.WriteLine($"award: [{Context.User}] awarded [{amount}] Lolos to [{user}]");
            eb.WithDescription($"{user.Username} was awarded {amount} Lolos :lollipop:");
            await ReplyAsync("", false, eb);

        }

        [Command("lolos")]
        [Remarks("Check how many Lolos user has.")]
        [Alias("$")]
        [MinPermissions(AccessLevel.User)]
        public async Task lolos([Remainder] IUser user = null)
        {
            if (user == null) // if no user as parameter, set user to caller
            {
                user = Context.User;
            }


            var users = Database.GetUserInfo(user);

            var eb = new EmbedBuilder();
            eb.WithColor(0xFF69B4);
            Console.WriteLine($"lolos: [{Context.User}] checked [{user}]");
            eb.WithDescription($"**{user.Username}** has {users.FirstOrDefault().Lolos} Lolos :lollipop:");
            await ReplyAsync("", false, eb);
        }

        [Command("give")]
        [Remarks("Give Lolos to user.")]
        [MinPermissions(AccessLevel.User)]
        public async Task give(int amount, [Remainder] IUser user)
        {
            if (user == Context.User || amount <= 0) // if user calls themselves or is trying to give 0 or less return 
            {
                return;
            }

            var eb = new EmbedBuilder();
            eb.WithColor(0xFF69B4);

            var users = Database.GetUserInfo(Context.User);

            if(users.FirstOrDefault().Lolos < amount) // if user doesn't have enough Lolos to give message and return
            {
                eb.WithDescription($"You don't have enough Lolos :lollipop:");
                await ReplyAsync("", false, eb);
                return;
            }

            Database.AddLolos(user, amount);
            var negAmount = amount * -1;
            Database.AddLolos(Context.User, negAmount); // else the user had enough lolos

            Console.WriteLine($"give: [{Context.User}] gave [{amount}] Lolos to [{user}]");

            eb.WithDescription($"{Context.User.Mention} has gifted {amount} Lolos :lollipop: to **{user}**");
            await ReplyAsync("", false, eb);
        }

        [Command("bf")]
        [Remarks("Bet flip, 2x amount on win. Bet for Zag or Zig.")]
        [MinPermissions(AccessLevel.User)]
        public async Task betflip(int amount, string zigzag)
        {
            var eb = new EmbedBuilder();
            eb.WithColor(0xFF0000);

            if (zigzag == "Zig" || zigzag == "zig" || zigzag == "Zag" || zigzag == "zag") // check that user has entered Zig or Zag as 2nd parameter
            {
                eb.WithColor(0xFF69B4);
                if (amount <= 0) // can't bet 0 or less
                {
                    return;
                }


                var users = Database.GetUserInfo(Context.User);

                if (users.FirstOrDefault().Lolos < amount) // if user has enough lolos to bet
                {
                    eb.WithDescription($"You don't have enough Lolos :lollipop:");
                    await ReplyAsync("", false, eb);
                    return;
                }

                Random random = new Random();
                int randomNumber = random.Next(0, 2); // 0 = Zag, 1 = Zig

                int negAmount = amount * -1;

                if (randomNumber == 0) // if randomizer landed on Zag
                {
                    Console.WriteLine($"bf: [{Context.User}] bet [{amount}] on [{zigzag}], Landed on: [Zag]");
                    eb.WithImageUrl("http://i.imgur.com/UgucKv1.png");
                    if (zigzag == "Zag" || zigzag == "zag")
                    {
                        Database.AddLolos(Context.User, amount);
                        eb.WithDescription($"{Context.User.Mention} You guessed it! <:zag:338784273081565184> has given you {amount} Lolos :lollipop:");
                        await ReplyAsync("", false, eb);
                    }
                    else if (zigzag == "Zig" || zigzag == "zig")
                    {

                        Database.AddLolos(Context.User, negAmount);
                        eb.WithDescription($"{Context.User.Mention} <:zag:338784273081565184> stole your Lolos :lollipop: ;_;");
                        await ReplyAsync("", false, eb);
                    }
                }
                else if (randomNumber == 1) // if randomizer landed on Zig
                {
                    Console.WriteLine($"bf: [{Context.User}] bet [{amount}] on [{zigzag}], Landed on: [Zig]");
                    eb.WithImageUrl("http://i.imgur.com/4QI0cID.png");
                    if (zigzag == "Zig" || zigzag == "zig")
                    {
                        Database.AddLolos(Context.User, amount);
                        eb.WithDescription($"{Context.User.Mention} You guessed it! <:zig:338784285974724621> has given you {amount} Lolos :lollipop:");
                        await ReplyAsync("", false, eb);
                    }
                    else if (zigzag == "Zag" || zigzag == "zag")
                    {
                        Database.AddLolos(Context.User, negAmount);
                        eb.WithDescription($"{Context.User.Mention} <:zig:338784285974724621> stole your Lolos :lollipop: ;_;");
                        await ReplyAsync("", false, eb);
                    }
                }
                else // this should never be printed...
                {
                    eb.WithDescription($"Something went pretty wrong");
                    eb.WithImageUrl("http://i.imgur.com/oshHgpm.png");
                    await ReplyAsync("", false, eb);
                }


            }
            else
            {
                eb.WithDescription($"!bf [amount] [Zag or Zig]\nEnter Zag or Zig after amount");
                await ReplyAsync("", false, eb);
            }

        }

        [Command("br")]
        [Remarks("Bet roll, 2x amount over 66, 4x amount over 90, 10x amount on 100.")]
        [MinPermissions(AccessLevel.User)]
        public async Task betroll(int amount)
        {
            if (amount <= 0) // can't bet 0 or less
            {
                return;
            }

            var eb = new EmbedBuilder();
            eb.WithColor(0xFF69B4);

            var users = Database.GetUserInfo(Context.User);

            if (users.FirstOrDefault().Lolos < amount) // if user has enough lolos to bet
            {
                eb.WithDescription($"You don't have enough Lolos :lollipop:");
                await ReplyAsync("", false, eb);
                return;
            }

            Random random = new Random();
            int randomNumber = random.Next(0, 101); // The roll

            string rollMessage = $"You rolled {randomNumber}.";

            Console.WriteLine($"br: [{Context.User}] bet [{amount}], rolled [{randomNumber}]");

            if (randomNumber < 67)
            {
                int negAmount = amount * -1;
                Database.AddLolos(Context.User, negAmount);
                rollMessage = rollMessage + " Better luck next time!";
            }
            else
            {
                int price;

                if (randomNumber < 91)
                {
                    price = amount * 2;
                    Database.AddLolos(Context.User, price);
                    rollMessage = rollMessage + $" Congratulations! You won {price} Lolos :lollipop: for rolling above 66!";
                }
                else if (randomNumber < 100)
                {
                    price = amount * 4;
                    Database.AddLolos(Context.User, price);
                    rollMessage = rollMessage + $" Congratulations! You won {price} Lolos :lollipop: for rolling above 90!";
                }
                else
                {
                    price = amount * 10;
                    Database.AddLolos(Context.User, price);
                    rollMessage = rollMessage + $" Congratulations! You won {price} Lolos :lollipop: for rolling 100!";
                }
            }
            eb.WithDescription(rollMessage);
            await ReplyAsync("", false, eb);
        }


    }


}