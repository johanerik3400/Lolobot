using Discord.Commands;
using Discord;
using Lolobot.Preconditions;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using System;

namespace Lolobot.Modules
{
    [Name("Waifu")]
    public class WaifuModule : ModuleBase<SocketCommandContext>
    {
        [Command("waifuinfo")]
        [Remarks("Check waifu information for user.")]
        [Alias("waifu")]
        [MinPermissions(AccessLevel.User)]
        public async Task waifuinfo([Remainder] IUser user = null)
        {
            if (user == null) // If no user as parameter, set user to caller
            {
                user = Context.User;
            }
            var users = Database.GetUserInfo(user);
            var claimedwaifus = Database.GetWaifus(user);

            var eb = new EmbedBuilder();
            eb.WithColor(0xFF69B4);

            Console.WriteLine($"waifuinfo: [{Context.User}] checked [{user}]");


            int countWaifus = 0; //How many waifus user has
            string waifuField = "Nobody"; // Some magical/shitty way to show a list of claimed waifus usernames... If user changes name in discord database won't update it! (might want to fix later)
            foreach (var waifu in claimedwaifus)
            {
                countWaifus++; // Every time a waifu added to string
                if (waifuField == "Nobody") // If it got here user has waifus, remove "Nobody" from the string.
                    waifuField = "";
                if (countWaifus < 30)
                    waifuField = waifuField + ($"\n{Program.client.GetUser(waifu.WaifuId)}"); // String added to embed at end of method
                else if(countWaifus == 30)
                    waifuField = waifuField + "\nToo many waifus <:zag:338784273081565184>";
            }
            string titleWaifus = "";

            if (countWaifus == 0)
                titleWaifus = "The Lonely";
            else if (countWaifus == 1)
                titleWaifus = "The Devoted";
            else if (countWaifus < 4)
                titleWaifus = "The Confizzeled";
            else if (countWaifus < 6)
                titleWaifus = "The Schemer";
            else if (countWaifus < 8)
                titleWaifus = "The Tamer Seducer";
            else if (countWaifus < 10)
                titleWaifus = "The Waifu Stealer";
            else if (countWaifus < 14)
                titleWaifus = "The Sora";
            else if (countWaifus < 17)
                titleWaifus = "The Succubus";
            else if (countWaifus < 20)
                titleWaifus = "The master of NTR";
            else if (countWaifus < 26)
                titleWaifus = "The Harem King";
            else if (countWaifus < 50)
                titleWaifus = "The Harem God";
            else
                titleWaifus = "Owner Of All Good Waifus";

            eb.WithAuthor($"{user.Username} - \"{titleWaifus}\"");

            eb.AddField("Price", $"{users.FirstOrDefault().Price}", true);

            ulong claimerId = Database.GetClaimerId(user);
            if(claimerId != 0)
            {
                eb.AddField("Claimed by", $"{Program.client.GetUser(claimerId)}", true);
            }
            else
            {
                eb.AddField("Claimed by", "Nobody", true);
            }

            if(users.FirstOrDefault().Affinity != 0)
            {
                eb.AddField("Likes", $"{Program.client.GetUser(users.FirstOrDefault().Affinity)}", true);
            }
            else
            {
                eb.AddField("Likes", "Nobody", true);
            }

            int countChanges = users.FirstOrDefault().ChangeOfHeart;
            string titleAffinity = "";

            if (countChanges == 0)
                titleAffinity = "The Pure";
            else if (countChanges == 1)
                titleAffinity = "The Faithful";
            else if (countChanges <4)
                titleAffinity = "The Confizzeled";
            else if (countChanges < 6)
                titleAffinity = "The Impure";
            else if (countChanges < 8)
                titleAffinity = "The Cheater";
            else if (countChanges < 11)
                titleAffinity = "The Sora";
            else if (countChanges < 14)
                titleAffinity = "The Corrupted";
            else if (countChanges < 18)
                titleAffinity = "The Lewd";
            else if (countChanges < 26)
                titleAffinity = "The Slut";
            else
                titleAffinity = "Should get banned";

            eb.AddField("Changes of heart", $"{countChanges} - \"{titleAffinity}\"", true);


            int countDivorces = users.FirstOrDefault().Divorces;

            if (countDivorces < 10)
                eb.AddField("Divorces", $"{countDivorces}", true);
            else
                eb.AddField("Divorces", $"{countDivorces} - \"Only Sora would do this\"", true);



            eb.AddField($"Waifus ({countWaifus})", $"{waifuField}", true);
            await ReplyAsync("", false, eb);
        }

        [Command("claim")]
        [Remarks("Claim a waifu for Lolos.")]
        [Alias("claimwaifu")]
        [MinPermissions(AccessLevel.User)]
        public async Task claim(int amount, [Remainder] IUser user)
        {
            if (user == Context.User || amount <= 0) // If user tries to claim himself or tries to claim with 0 or less return with no message.
            {
                return;
            }

            var users = Database.GetUserInfo(user);
            var contextUser = Database.GetUserInfo(Context.User);
            var claimedwaifus = Database.GetWaifus(user);

            int amountWithTax = 0;

            var eb = new EmbedBuilder();
            eb.WithColor(0xFF69B4);

            int isAffinity = 0;

            if(users.FirstOrDefault().Affinity == Context.User.Id) // If waifu to claim has affinity on claimer, give discount on waifu price
            {
                amountWithTax = (int)(users.FirstOrDefault().Price * 0.88f);
                eb.WithFooter("You get a 12% discount because they like you <3", null);
                isAffinity = 1;
            }
            else
            {
                amountWithTax = (int)(users.FirstOrDefault().Price * 1.1); // If waifu to claim doesn't have affinity on claimer, give 10% tax on price
                eb.WithFooter("You need to pay 10% more when forcefully claiming waifus!", null);
            }

            if (contextUser.FirstOrDefault().Lolos < amount) // If claiming user doesn't have enough Lolos for amount they try to claim with
            {
                Console.WriteLine($"claim: [{Context.User}] tried to claim [{user}] for [{amount}], not enough Lolos to claim");
                eb.WithDescription($"{Context.User.Mention} you don't have enough Lolos :lollipop: to claim that waifu!");
            }
            else if (amountWithTax > amount) // If user doesn't have enough Lolos for the price of waifu after adding or removing %
            {
                Console.WriteLine($"claim: [{Context.User}] tried to claim [{user}] for [{amount}], too low amount");
                eb.WithDescription($"{Context.User.Mention} you must pay {amountWithTax} or more to claim that waifu!");
            }
            else if (Database.GetClaimerId(user) == Context.User.Id) // If user already is claimed, send message with (again), and don't change anything in claimedwaifu table
            {
                var negAmount = amount * -1; // Remove Lolos from claimer
                Database.AddLolos(Context.User, negAmount);
                Console.WriteLine($"claim: [{Context.User}] claimed (again) [{user}] for [{amount}]");
                string destription = $"{Context.User.Mention} claimed **{user}** as their waifu (again) <:waifu:339399943796162562> for {amount} Lolos :lollipop:";
                if (isAffinity == 1) // If waifu to claim has affinity on claimer, increase waifu price by 25% and send cute message!
                {
                    int newValue = amount + (amount / 4);
                    Database.ChangePrice(user, newValue);
                    eb.WithDescription(destription + $"\n<:waifu:339399943796162562> Their love is fulfilled!<:waifu:339399943796162562>\n{user}'s new value is {newValue} Lolos :lollipop:");
                }
                else // Else just change price to amount claimed for
                {
                    Database.ChangePrice(user, amount);
                    eb.WithDescription(destription);
                }
            }
            else // Else, if user isn't claimed by the claimer 
            {
                var negAmount = amount * -1; // Remove Lolos from claimer
                Database.AddLolos(Context.User, negAmount);
                Database.DeleteClaim(Program.client.GetUser(Database.GetClaimerId(user)), user);
                Database.ClaimWaifu(Context.User, user, amount);
                Console.WriteLine($"claim: [{Context.User}] claimed [{user}] for [{amount}]");
                string destription = $"{Context.User.Mention} claimed **{user}** as their waifu <:waifu:339399943796162562> for {amount} Lolos :lollipop:";
                if (isAffinity == 1) // If waifu to claim has affinity on claimer, increase waifu price by 25% and send cute message!
                {
                    int newValue = amount + (amount / 4);
                    Database.ChangePrice(user, newValue);
                    eb.WithDescription(destription + $"\n<:waifu:339399943796162562> Their love is fulfilled !<:waifu:339399943796162562>\n{user}'s new value is {newValue} Lolos :lollipop:");
                }
                else // Else just change price to amount claimed for
                {
                    Database.ChangePrice(user, amount);
                    eb.WithDescription(destription);
                }
            }
 
            await ReplyAsync("", false, eb);
        }

        [Command("divorce")]
        [Remarks("Divorce a waifu and get half of the spent Lolos back.")]
        [MinPermissions(AccessLevel.User)]
        public async Task divorce([Remainder] IUser user)
        {
            var eb = new EmbedBuilder();
            eb.WithColor(0xFF69B4);

            if (user == Context.User) // If user tries to divorce temselves just return
            {
                Console.WriteLine($"divorce: [{Context.User}] tried to divorce themselves");
                eb.WithDescription($"{Context.User.Mention} Sorry, but you can't divorce yourself.");
                await ReplyAsync("", false, eb);
                return;
            }

            var users = Database.GetUserInfo(user);
            var contextUser = Database.GetUserInfo(Context.User);
            var claimer = Database.GetClaimerId(user);

            if (claimer == Context.User.Id) // If user you try to divorce has divorcing user as claimer, do the divorce  
            {
                var refund = users.FirstOrDefault().Price / 2;
                Database.AddLolos(Context.User, refund);
                Database.DeleteClaim(Context.User, user);
                Console.WriteLine($"divorce: [{Context.User}] divorced their waifu [{user}]");
                eb.WithDescription($"{Context.User.Mention} You have divorced a waifu who doesn't like you (or you are Sora). You received {refund} Lolos :lollipop: back.");
            }
            else // Else that tamer isn't your waifu so do nothing
            {
                Console.WriteLine($"divorce: [{Context.User}] tried to divorce someone who wasn't their waifu [{user}]");
                eb.WithDescription($"{Context.User.Mention} That tamer isn't even your waifu <:zag:338784273081565184>.");
            }

            await ReplyAsync("", false, eb);
        }

        [Command("affinity")]
        [Remarks("Set you affinity towards a tamer.")]
        [MinPermissions(AccessLevel.User)]
        public async Task affinity([Remainder] IUser user = null)
        {
            var contextUser = Database.GetUserInfo(Context.User);

            if (user == null && contextUser.FirstOrDefault().Affinity != 0) // if no user as parameter, and user has someone as affinity delete affinity
            {
                var eb2 = new EmbedBuilder();
                eb2.WithColor(0xFF69B4);
                Database.ResetAffinity(Context.User);
                Console.WriteLine($"affinity: [{Context.User}] has reset their affinity");
                eb2.WithDescription($"{Context.User.Mention} Your affinity is reset.You no longer have a person you like.");
                await ReplyAsync("", false, eb2);
                return;
            }
            else if (user == null) // Else if user not chosen and user doesn't have affinity do nothing
            {
                var eb2 = new EmbedBuilder();
                eb2.WithColor(0xFF69B4);
                Console.WriteLine($"affinity: [{Context.User}] tried to remove bad affinity");
                eb2.WithDescription($"{Context.User.Mention} You don't have a person you like.");
                await ReplyAsync("", false, eb2);
                return;
            }

            if (user == Context.User) // if user trying to affinity themselves just return
            {
                return;
            }

            var users = Database.GetUserInfo(user);

            var eb = new EmbedBuilder();
            eb.WithColor(0xFF69B4);

            if (user.Id != contextUser.FirstOrDefault().Affinity) // if the person isn't already as your affinity 
            {
                if(contextUser.FirstOrDefault().Affinity == 0) // if you don't have anyone as affinity 
                {
                    Database.ChangeAffinity(Context.User, user);
                    Console.WriteLine($"affinity: [{Context.User}] did affinity on [{user}]");
                    eb.WithDescription($"{Context.User.Mention} Wants to be **{user}**'s waifu <:waifu:339399943796162562>");
                    Database.AddOneToChangeOfHeart(Context.User);
                }
                else // else you have someone as affinity and change to someone else
                {
                    Console.WriteLine($"affinity: [{Context.User}] changed affinity to [{user}] from [{Program.client.GetUser(contextUser.FirstOrDefault().Affinity)}]");
                    eb.WithDescription($"{Context.User.Mention} Changed their affinity from **{Program.client.GetUser(contextUser.FirstOrDefault().Affinity)}** to **{user}**" +
                        $"\n*This is morally questionable* <:zag:338784273081565184>");
                    Database.ChangeAffinity(Context.User, user);
                    Database.AddOneToChangeOfHeart(Context.User);
                }

            }
            else // if you already have person as your affinity do nothing
            {
                Console.WriteLine($"affinity: [{Context.User}] did affinity on [{user}] - already had affinity");
                eb.WithDescription($"{Context.User.Mention} You already like that tamer!");
            }

            await ReplyAsync("", false, eb);
        }
    }


}