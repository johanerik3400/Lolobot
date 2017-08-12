using Discord.Commands;
using Discord;
using Lolobot.Preconditions;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using System;

namespace Lolobot.Modules
{
    [Name("Weekly")]
    public class WeeklyModule : ModuleBase<SocketCommandContext>
    {
        [Command("signup")]
        [Remarks("Sign up for weekly competition. Only works from Monday -> Thursday")]
        [MinPermissions(AccessLevel.User)]
        public async Task addentry([Remainder] string content = null)
        {
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();

            var eb = new EmbedBuilder();

            if (Configuration.Load().Signupphase == 0) // Signups only Monday -> Thursday. 
            {
                eb.WithColor(0xFF0000);
                eb.WithDescription("You can only sign up from Monday to Thrusday. *+- 30 min*");
                await dmChannel.SendMessageAsync("", false, eb);
                return;
            }

            if (content == null)
            {
                eb.WithColor(0xFF0000);
                eb.WithDescription("You need to add some content to your entry.");
                await dmChannel.SendMessageAsync("", false, eb);
                return;
            }

            eb.WithColor(0xFF69B4);

            content = content.Replace("'", "''");

            bool addsignup;

            addsignup = Database.AddWeekly(Context.User, content);

            if(addsignup)
            {
                eb.WithAuthor("Your entry has been saved.");
                eb.WithTitle("Preview:");
                eb.WithDescription(content);
            }
            else
            {
                eb.WithAuthor("You are already signed up.");
                eb.WithTitle("Your current entry:");
                eb.WithFooter("Delete old entry before making a new one with command: deleteentry");
                var Weekly = Database.GetWeekly(Context.User);
                eb.WithDescription(Weekly.FirstOrDefault().Content);
            }

            await dmChannel.SendMessageAsync("", false, eb);

        }

        [Command("deleteentry")]
        [Remarks("Deletes your entry to weekly competitions.")]
        [Alias("delentry")]
        [MinPermissions(AccessLevel.User)]
        public async Task delentry(string answer = null)
        {
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();

            var eb = new EmbedBuilder();

            if (Configuration.Load().Signupphase == 0) // Signups only Monday -> Thursday. 
            {
                eb.WithColor(0xFF0000);
                eb.WithDescription("You can only edit sign up from Monday to Thrusday. *+- 30 min*");
                await dmChannel.SendMessageAsync("", false, eb);
                return;
            }

            var Weekly = Database.GetWeekly(Context.User);

            if(Weekly.Count() <= 0)
            {
                eb.WithColor(0xFF0000);
                eb.WithDescription($"You aren't signed up with any entry.\n**!signup [content]** to sign up.");
                await dmChannel.SendMessageAsync("", false, eb);
                return;
            }

            eb.WithColor(0xFF69B4);

            if(answer != null)
                answer.ToLower();

            if (answer == "yes")  
            {
                Database.DelWeekly(Context.User);
                eb.WithDescription($"Your entry has been deleted.");
                await dmChannel.SendMessageAsync("", false, eb);
                return;
            }
            else
            {
                eb.WithDescription($"Are you sure you want to delete your entry?\n**!deleteentry yes** to delete your entry.\n\n__Your current entry:__\n\n{Weekly.FirstOrDefault().Content}");
                await dmChannel.SendMessageAsync("", false, eb);
            }

        }

        [Command("mysignup")]
        [Remarks("Displays your entry.")]
        [Alias("myentry")]
        [MinPermissions(AccessLevel.User)]
        public async Task showentry()
        {
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();

            var eb = new EmbedBuilder();

            var Weekly = Database.GetWeekly(Context.User);

            if (Weekly.Count() <= 0)
            {
                eb.WithColor(0xFF0000);
                eb.WithDescription($"You aren't signed up.\n**!signup [content]** to sign up. *Only Monday -> Thrusday.*");
                await dmChannel.SendMessageAsync("", false, eb);
                return;
            }

            eb.WithColor(0xFF69B4);

            eb.WithAuthor("Your entry:");
            eb.WithDescription($"{Weekly.FirstOrDefault().Content}");
            await dmChannel.SendMessageAsync("", false, eb);

        }

        [Command("rewardsignups")]
        [Remarks("Rewards all users in database who are signed up with [amount] of Lolos.")]
        [MinPermissions(AccessLevel.User)] // REMEMBER THIS -------------------------------
        public async Task rewardusers(int amount)
        {
            var eb = new EmbedBuilder();

            var AllWeekly = Database.GetAllWeekly();

            int awardedUsers = 0;

            foreach (var weekly in AllWeekly)
            {
                Database.AddLolos(Program.client.GetUser(weekly.UserId), amount);
                awardedUsers++;
            }

            eb.WithColor(0xFF69B4);
            if(awardedUsers <= 0)
            {
                eb.WithDescription("No users in are signed up.");
            }
            else if(awardedUsers == 1)
            {
                eb.WithDescription($"**{awardedUsers}** user has been awarded **{amount}** Lolos :lollipop:");
            }
            else
            {
                eb.WithDescription($"**{awardedUsers}** users have been awarded **{amount}** Lolos :lollipop: each!");
            }

            await ReplyAsync("", false, eb);

        }

        [Command("maketop5")]
        [Remarks("Makes and starts voting for top 5 picks. Takes 5 database ID's as parameter. Will send vote options to specified text channel.")]
        [MinPermissions(AccessLevel.User)] // REMEMBER THIS -------------------------------
        public async Task maketopfive(int entry1, int entry2, int entry3, int entry4, int entry5)
        {
            var eb = new EmbedBuilder();
            var eb2 = new EmbedBuilder();

            var AllWeekly = Database.MakeTop5(entry1, entry2, entry3, entry4, entry5);

            if (AllWeekly.Count() < 5)
            {
                eb.WithColor(0xFF0000);
                eb.WithDescription("Didn't find 5 entries with given IDs.");
                await ReplyAsync("", false, eb);
                return;
            }

            eb.WithColor(0xFF69B4);
            eb2.WithColor(0xFF69B4);

            ITextChannel channel = (ITextChannel)Program.client.GetChannel(345245940401045515); // REMEMBER THIS

            eb2.WithAuthor("New vote for week (add week number from DB)");
            eb2.WithDescription("Vote started etc… Weekly theme was… These are the five… !vote [1-5] to vote...\nVote started etc… Weekly theme was… These are the five… !vote [1-5] to vote... \nVote started etc… Weekly theme was… These are the five… !vote [1-5] to vote...  ");
            await channel.SendMessageAsync("", false, eb2);

            int entryNr = 1;

            foreach (var weekly in AllWeekly)
            {
                string s = weekly.Content;

                foreach (string URL in s.Split(' '))
                {
                    if (Uri.IsWellFormedUriString(URL, UriKind.Absolute))
                    {
                        eb.WithImageUrl(URL);
                        eb.WithAuthor($"Entry #{entryNr}");
                        eb.WithDescription(weekly.Content);
                        eb.WithFooter($"{Program.client.GetUser(weekly.UserId)}");
                        break;
                    }
                    else
                    {
                        eb.WithAuthor($"Entry #{entryNr}");
                        eb.WithDescription(weekly.Content);
                    }

                }

                entryNr++;

                //eb.WithDescription($"**{Program.client.GetUser(weekly.UserId)}** Had content:\n{weekly.Content}\n\nWith **{weekly.Votes}** votes");
                await channel.SendMessageAsync("", false, eb);
            }

            Configuration.SetVotephase(1);

        }

        [Command("vote")]
        [Remarks("Vote for one of the 5 entries.")]
        [MinPermissions(AccessLevel.User)]
        public async Task vote(int vote)
        {
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
            var eb = new EmbedBuilder();
            eb.WithColor(0xFF69B4);
            if(Configuration.Load().Votephase == 0)
            {
                eb.WithColor(0xFF000);
                eb.WithDescription("Voting isn't open at the moment.");
                await dmChannel.SendMessageAsync("", false, eb);
                return;
            }

            if (vote < 1 || vote > 5)
            {
                eb.WithColor(0xFF000);
                eb.WithDescription("!vote [1-5]\nYou have to vote for one of the 5 enties.");
                await dmChannel.SendMessageAsync("", false, eb);
                return;
            }

            var top5 = Database.ShowTop5();

            var votedfor = Database.GetVotedfor(Context.User);
            if (votedfor.Count() > 0)
            {
                Database.ChangeVote(Context.User, vote);
                eb.WithDescription($"You've already voted for entry **{votedfor.FirstOrDefault().Voted}**\nYour vote has been changed to entry **{vote}**.");
            }
            else
            {
                Database.AddVote(Context.User, vote);
                eb.WithDescription($"Your vote for entry **{vote}** has been registered. Thanks for voting!");
            }


            await dmChannel.SendMessageAsync("", false, eb);

        }

    }
}