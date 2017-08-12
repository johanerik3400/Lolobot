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
        public async Task addfaq(string keyword, string URL = null, [Remainder] string description = null)
        {
            var eb = new EmbedBuilder();
            eb.WithColor(0xFF69B4);

            eb.WithAuthor("New FAQ added! Preview:");
            eb.WithTitle(keyword);

            if(description == null)
            {
                description = "";
            }

            keyword = keyword.Replace("'", "''");
            description = description.Replace("'", "''");

            Console.WriteLine(keyword + description);

            bool faqadd;

            if (Uri.IsWellFormedUriString(URL, UriKind.Absolute))
            {
                eb.WithImageUrl(URL);
                eb.WithDescription(description);
                faqadd = Database.AddFaq(keyword, URL, description, Context.User);
            }
            else
            {
                URL = URL.Replace("'", "''");
                eb.WithDescription($"{URL} {description}");
                faqadd = Database.AddFaq(keyword, null, $"{URL} {description} ", Context.User);
                eb.WithFooter("Either there was no URL given, or it was incorrect. If you meant to add an URL to this FAQ, check that the URL is a proper URL, delete the old FAQ and remake it.");
            }

            if(faqadd)
            {
                await ReplyAsync("", false, eb);
                return;
            }
            else
            {
                var eb2 = new EmbedBuilder();
                eb2.WithColor(0xFF0000);
                eb2.WithDescription($"The keyword **{keyword}** already exists in the database. Either delete the old one or make a new one with a different keyword.");
                await ReplyAsync("", false, eb2);
            }
        }

        [Command("faq")]
        [Remarks("Look up FAQ with keyword and print if found.")]
        [Alias("?")]
        [MinPermissions(AccessLevel.User)]
        public async Task faqprint(string keyword)
        {
            var eb = new EmbedBuilder();
            eb.WithColor(0xFF69B4);

            keyword = keyword.Replace("'", "''");

            var result = Database.CheckExistingFAQ(keyword);
            if (result.Count() <= 0)
            {
                eb.WithDescription("The keyword was not found.");
                await ReplyAsync("", false, eb);
                return;
            }

            var FAQ = Database.GetFaq(keyword);

            Database.AddOneToTimesUsed(keyword);

            eb.WithTitle(FAQ.FirstOrDefault().Keyword);
            eb.WithDescription(FAQ.FirstOrDefault().Description);
            eb.WithFooter($"({FAQ.FirstOrDefault().Timesused + 1}) - Author: {Program.client.GetUser(FAQ.FirstOrDefault().AuthorId)}");

            if (FAQ.FirstOrDefault().Url != null)
            {
                eb.WithImageUrl(FAQ.FirstOrDefault().Url);
            }

            await ReplyAsync("", false, eb);
        }

        [Command("delfaq")]
        [Remarks("Delete FAQ with given keyword.")]
        [MinPermissions(AccessLevel.User)] // REMEMBER THIS, MOD OR ADMIN (MCHELPER)
        public async Task delfaq(string keyword)
        {
            var eb = new EmbedBuilder();
            eb.WithColor(0xFF69B4);

            keyword = keyword.Replace("'", "''");

            var result = Database.CheckExistingFAQ(keyword);
            if (result.Count() <= 0)
            {
                eb.WithDescription("Could not delete since the keyword was not found.");
                await ReplyAsync("", false, eb);
                return;
            }

            Database.DelFaq(keyword);

            eb.WithDescription($"The FAQ with keyword **{keyword}** was deleted.");

            await ReplyAsync("", false, eb);
        }

        [Command("listfaq")]
        [Remarks("Lists all FAQ keywords available.")]
        [MinPermissions(AccessLevel.User)] 
        public async Task listafq()
        {
            var eb = new EmbedBuilder();
            eb.WithColor(0xFF69B4);

            var result = Database.GetAllKeywords();


            if (result.Count() <= 0)
            {
                eb.WithDescription("There are no FAQs in the database.");
                await ReplyAsync("", false, eb);
                return;
            }

            string keywords = result.FirstOrDefault();

            foreach (var faq in result.Skip(1))
            {
                keywords = keywords + $", {faq}";
            }

            eb.WithAuthor("Keywords found:");
            eb.WithDescription($"{keywords}");

            await ReplyAsync("", false, eb);
        }
    }


}