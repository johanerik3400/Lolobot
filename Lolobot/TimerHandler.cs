using Discord;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lolobot
{
    class TimerHandler
    {
        private Timer _tm = null;

        private AutoResetEvent _autoEvent = null;

        private int _counter = 0;


        public void StartTimer()
        {
            _autoEvent = new AutoResetEvent(false);
            _tm = new Timer(Execute, _autoEvent, 10000, 10000);    
            Console.Read();
        }

        public void Execute(Object stateInfo)
        {
            Console.WriteLine("Call #" + _counter);
            _counter++;

            int day = ((int)DateTime.Now.DayOfWeek == 0) ? 7 : (int)DateTime.Now.DayOfWeek; // Set first day of the week to Monday with integer value 1 and Sunday with integer value 7

            if (day > 4 && Configuration.Load().Signupphase == 1) // When signups end on Friday morning -> Stop signups
            {
                // Stop signups
                Configuration.SetSignupphase(0);
            }
            else if(Configuration.Load().Votephase == 1 && day >= 1 && day < 5) // When Sunday ends start signups if there is one prepared
            {
                // Turn off Vote phase
                Configuration.SetVotephase(0);
                // Show winners
                var winners = Database.GetWinners();

                ITextChannel channel = (ITextChannel)Program.client.GetChannel(345245940401045515); // REMEMBER THIS
                var eb = new EmbedBuilder();
                eb.WithColor(0xFF69B4);

                string allwinners = "";

                int place = 1;
                int points = 3;
                int lastVotes = 0;
                foreach (var winner in winners)
                {
                    Console.WriteLine($"ID: {winner.ID} | Votes: {winner.Votes}");
                    var winnerInfo = Database.GetTop5ByID(winner.ID);

                    if(place == 1 && winner.Votes != lastVotes)
                    {
                        allwinners = allwinners + $"**{place}**st place. Gained **{points}** points.\n";
                    }
                    else if(place == 2 && winner.Votes != lastVotes)
                    {
                        allwinners = allwinners + $"\n**{place}**nd place. Gained **{points}** points.\n";
                    }
                    else if(place == 3 && winner.Votes != lastVotes)
                    {
                        allwinners = allwinners + $"\n**{place}**rd place. Gained **{points}** point.\n";
                    }
                    
                    allwinners = allwinners + $"**{Program.client.GetUser(winnerInfo.FirstOrDefault().UserId)}**\n";
                    if (winner.Votes != lastVotes)
                    {
                        place++;
                        if (points > 1)
                            points--;

                    }
                    else
                    {   
                        if(place < 4)
                            eb.WithFooter("Same amount of votes for some entries.");
                    }

                    lastVotes = winner.Votes;
                   
                }
                eb.WithTitle($"Vote has ended for competition Nr. {Configuration.Load().WeeklyID}!");
                eb.WithDescription(allwinners);
                channel.SendMessageAsync("", false, eb);

                // Award points 
                // Update leaderboard

                // WeeklyID ++
                int ID = Configuration.Load().WeeklyID + 1;

                Configuration.SetWeeklyID(ID);

                // Check if exists in DB
                if(Database.CheckExistingTheme(ID))
                {
                    // Announce new weekly

                    ITextChannel channel2 = (ITextChannel)Program.client.GetChannel(345913036105842700); // REMEMBER THIS
                    var Weeklytheme = Database.GetWeeklytheme(ID);
                    var eb2 = new EmbedBuilder();
                    eb2.WithColor(0xFF69B4);
                    eb2.WithTitle($"New weekly theme! Competition Nr. {ID}");
                    eb2.WithDescription(Weeklytheme.FirstOrDefault().Theme);
                    var url = Weeklytheme.FirstOrDefault().URL;
                    if(url != "none")
                    {
                        eb2.WithUrl(url);
                    }
                    channel2.SendMessageAsync("", false, eb2);
                    // Delete top 5s
                    // Delete old weekly entries
                    // Start Signups
                    Configuration.SetSignupphase(1);
                }

            }

            return;

        }
    }
}
