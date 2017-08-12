using Discord;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lolobot
{
    public class Database
    {
        private string table { get; set; }
        private const string server = "localhost";
        private const string database = "discordbot";
        private const string username = "root";
        private const string password = "test3400";
        private MySqlConnection dbConnection;

        public Database(string table)
        {
            this.table = table;
            MySqlConnectionStringBuilder stringBuilder = new MySqlConnectionStringBuilder();
            stringBuilder.Server = server;
            stringBuilder.UserID = username;
            stringBuilder.Password = password;
            stringBuilder.Database = database;
            stringBuilder.SslMode = MySqlSslMode.None;

            var connectionString = stringBuilder.ToString();

            dbConnection = new MySqlConnection(connectionString);

            dbConnection.Open();
        }

        public MySqlDataReader FireCommand(string query)
        {
            if (dbConnection == null)
            {
                return null;
            }

            MySqlCommand command = new MySqlCommand(query, dbConnection);

            var mySqlReader = command.ExecuteReader();

            return mySqlReader;
        }

        public void CloseConnection()
        {
            if (dbConnection != null)
            {
                dbConnection.Close();
            }
        }

        public static List<ulong> CheckExistingUser(IUser user)
        {
            var result = new List<ulong>();
            var database = new Database("discordbot");

            var str = string.Format("SELECT * FROM users WHERE userid = '{0}'", user.Id);
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var userId = (ulong)tableName["userid"];

                result.Add(userId);
            }

            tableName.Close();

            return result;
        }

        public static string EnterUser(IUser user)
        {
            var database = new Database("discordbot");

            var str = string.Format("INSERT INTO users (userid, username, lolos , changeofheart, divorces, affinity, price) VALUES ('{0}', '{1}', '0', '0', '0', '0', '1')", user.Id, user.Username);
            var table = database.FireCommand(str);
            table.Close();
            database.CloseConnection();

            return null;
        }

        public static void UserCheck(IUser user)
        {
            var result = CheckExistingUser(user);
            if (result.Count() <= 0)
            {
                EnterUser(user);
            }
        }

        public static void AddLolos(IUser user, int lolos)
        {
            var result = CheckExistingUser(user);
            if (result.Count() <= 0)
            {
                EnterUser(user);
            }

            var database = new Database("discordbot");

            try
            {
                var strings = string.Format("UPDATE users SET lolos = lolos + '{1}' WHERE userid = {0}", user.Id, lolos);
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception e)
            {
                database.CloseConnection();
                return;
            }
        }

        public static List<users> GetUserInfo(IUser user)
        {

            var resultExist = CheckExistingUser(user);

            if (resultExist.Count() <= 0)
            {
                EnterUser(user);
            }

            var result = new List<users>();

            var database = new Database("discordbot");

            var str = string.Format("SELECT * FROM users WHERE userid = '{0}'", user.Id);
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var userId = (ulong)tableName["userid"];
                var userName = (string)tableName["username"];
                var currentTokens = (int)tableName["lolos"];
                var changeOfHeart = (int)tableName["changeofheart"];
                var divorces = (int)tableName["divorces"];
                var affinity = (ulong)tableName["affinity"];
                var price = (int)tableName["price"];

                result.Add(new users
                {
                    UserId = userId,
                    Username = userName,
                    Lolos = currentTokens,
                    ChangeOfHeart = changeOfHeart,
                    Divorces = divorces,
                    Affinity = affinity,
                    Price = price
                });
            }
            tableName.Close();
            database.CloseConnection();

            return result;

        }

        public static List<claimedwaifu> GetWaifus(IUser user)
        {
            var result = new List<claimedwaifu>();

            var database = new Database("discordbot");

            var str = string.Format("SELECT * FROM claimedwaifu WHERE userid = '{0}'", user.Id);
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var userId = (ulong)tableName["userid"];
                var waifuId = (ulong)tableName["waifuid"];

                result.Add(new claimedwaifu
                {
                    UserId = userId,
                    WaifuId = waifuId,
                });
            }
            tableName.Close();
            database.CloseConnection();

            return result;
        }

        public static ulong GetClaimerId(IUser user)
        {
            ulong result = 0;

            var database = new Database("discordbot");

            var str = string.Format("SELECT users.userid FROM users, claimedwaifu WHERE waifuid = '{0}' and claimedwaifu.userid = users.userid", user.Id);
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var userid = (ulong)tableName["userid"];

                result = userid;
            }

            tableName.Close();
            database.CloseConnection();

            return result;
        }

        public static void ClaimWaifu(IUser userClaimer, IUser claimedWaifu, int amount)
        {
            var result = CheckExistingUser(claimedWaifu);
            if (result.Count() <= 0)
            {
                EnterUser(claimedWaifu);
            }

            var database = new Database("discordbot");

            var str = string.Format("INSERT INTO claimedwaifu (userid, waifuid) VALUES ('{0}', '{1}')", userClaimer.Id, claimedWaifu.Id);
            var table = database.FireCommand(str);
            table.Close();

            try
            {
                var strings = string.Format("UPDATE users SET price = '{1}' WHERE userid = {0}", claimedWaifu.Id, amount);
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                database.CloseConnection();
                return;
            }
            
        }

        public static void ChangePrice(IUser claimedWaifu, int amount)
        {
            var result = CheckExistingUser(claimedWaifu);
            if (result.Count() <= 0)
            {
                EnterUser(claimedWaifu);
            }

            var database = new Database("discordbot");

            try
            {
                var strings = string.Format("UPDATE users SET price = '{1}' WHERE userid = {0}", claimedWaifu.Id, amount);
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                database.CloseConnection();
                return;
            }

        }

        public static void DeleteClaim(IUser userClaimer, IUser claimedWaifu)
        {
            var database = new Database("discordbot");

            try
            {
                var strings = string.Format("DELETE FROM claimedwaifu WHERE userid = '{0}' and waifuid = '{1}'", userClaimer.Id, claimedWaifu.Id);
                var reader = database.FireCommand(strings);
                reader.Close();
                var strings2 = string.Format("UPDATE users SET divorces = divorces + 1 WHERE userid = '{0}'", userClaimer.Id);
                var reader2 = database.FireCommand(strings2);
                reader2.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                database.CloseConnection();
                return;
            }

        }

        public static void ChangeAffinity(IUser userClaimer, IUser claimedWaifu)
        {
            var database = new Database("discordbot");

            try
            {
                var strings = string.Format("UPDATE users SET affinity = '{1}' WHERE userid = '{0}'", userClaimer.Id, claimedWaifu.Id);
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                database.CloseConnection();
                return;
            }
        }

        public static void ResetAffinity(IUser userClaimer)
        {
            var database = new Database("discordbot");

            try
            {
                var strings = string.Format("UPDATE users SET affinity = '0' WHERE userid = '{0}'", userClaimer.Id);
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                database.CloseConnection();
                return;
            }

        }


        public static void AddOneToChangeOfHeart(IUser user)
        {
            var database = new Database("discordbot");

            try
            {
                var strings = string.Format("UPDATE users SET changeofheart = changeofheart + 1 WHERE userid = '{0}'", user.Id);
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                database.CloseConnection();
                return;
            }
        }

        public static List<faq> GetFaq(string keywordSearch)
        {
            var result = new List<faq>();

            var database = new Database("discordbot");

            var str = string.Format("SELECT * FROM faq WHERE keyword = '{0}'", keywordSearch);
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var genre = (string)tableName["genre"];
                var keyword = (string)tableName["keyword"];
                var url = (string)tableName["url"];
                var description = (string)tableName["description"];
                var authorid = (ulong)tableName["authorid"];
                var timesused = (int)tableName["timesused"];

                result.Add(new faq
                {
                    Genre = genre,
                    Keyword = keyword,
                    Url = url,
                    Description = description,
                    AuthorId = authorid,
                    Timesused = timesused
                });
            }
            tableName.Close();
            database.CloseConnection();

            return result;

        }

        public static bool AddFaq(string keyword, string URL, string description, IUser user)
        {
            var result = CheckExistingFAQ(keyword);
            if (result.Count() > 0)
            {
                return false;
            }

            var database = new Database("discordbot");

            var str = string.Format("INSERT INTO faq (genre, keyword, url , description, authorid, timesused) VALUES ('0', '{0}', '{1}', '{2}', '{3}', '0')", keyword, URL, description, user.Id);
            var table = database.FireCommand(str);
            table.Close();
            database.CloseConnection();

            return true;
        }

        public static List<string> CheckExistingFAQ(string keywordSearch)
        {
            var result = new List<string>();
            var database = new Database("discordbot");

            var str = string.Format("SELECT * FROM faq WHERE keyword = '{0}'", keywordSearch);
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var keyword = (string)tableName["keyword"];

                result.Add(keyword);
            }

            tableName.Close();
            database.CloseConnection();

            return result;
        }

        public static void DelFaq(string keyword)
        {
            var database = new Database("discordbot");

            try
            {
                var strings = string.Format("DELETE FROM faq WHERE keyword = '{0}'", keyword);
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                database.CloseConnection();
                return;
            }
        }

        public static List<string> GetAllKeywords()
        {
            var result = new List<string>();
            var database = new Database("discordbot");

            var str = string.Format("SELECT keyword FROM faq");
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var keyword = (string)tableName["keyword"];

                result.Add(keyword);
            }

            tableName.Close();
            database.CloseConnection();

            return result;
        }


        public static void AddOneToTimesUsed(string keyword)
        {
            var database = new Database("discordbot");

            try
            {
                var strings = string.Format("UPDATE faq SET timesused = timesused + 1 WHERE keyword = '{0}'", keyword);
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                database.CloseConnection();
                return;
            }
        }


        public static bool AddWeekly(IUser user, string content)
        {
            UserCheck(user);

            var result = CheckExistingWeekly(user);
            if (result.Count() > 0)
            {
                return false;
            }

            var database = new Database("discordbot");

            var str = string.Format("INSERT INTO weekly (userid, content) VALUES ('{0}', '{1}')", user.Id, content);
            var table = database.FireCommand(str);
            table.Close();
            database.CloseConnection();

            return true;
        }

        public static List<ulong> CheckExistingWeekly(IUser user)
        {
            var result = new List<ulong>();
            var database = new Database("discordbot");

            var str = string.Format("SELECT * FROM weekly WHERE userid = '{0}'", user.Id);
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var userId = (ulong)tableName["userid"];

                result.Add(userId);
            }

            tableName.Close();
            database.CloseConnection();

            return result;
        }

        public static List<weekly> GetWeekly(IUser user)
        {
            var result = new List<weekly>();

            var database = new Database("discordbot");

            var str = string.Format("SELECT * FROM weekly WHERE userid = '{0}'", user.Id);
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var userid = (ulong)tableName["userid"];
                var content = (string)tableName["content"];

                result.Add(new weekly
                {
                    UserId = userid,
                    Content = content,
                });
            }
            tableName.Close();
            database.CloseConnection();

            return result;
        }

        public static List<weekly> GetAllWeekly()
        {
            var result = new List<weekly>();

            var database = new Database("discordbot");

            var str = string.Format("SELECT * FROM weekly");
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var userid = (ulong)tableName["userid"];
                var content = (string)tableName["content"];

                result.Add(new weekly
                {
                    UserId = userid,
                    Content = content,
                });
            }
            tableName.Close();
            database.CloseConnection();

            return result;

        }

        public static void DelWeekly(IUser user)
        {
            var database = new Database("discordbot");

            try
            {
                var strings = string.Format("DELETE FROM weekly WHERE userid = '{0}'", user.Id);
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                database.CloseConnection();
                return;
            }
        }


        public static List<top5> GetTop5(int entry1, int entry2, int entry3, int entry4, int entry5)
        {
            var result = new List<top5>();

            var database = new Database("discordbot");

            var str = string.Format("SELECT * FROM weekly WHERE id = '{0}' or id = '{1}' or id = '{2}' or id = '{3}' or id = '{4}'", entry1, entry2, entry3, entry4, entry5);
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var userid = (ulong)tableName["userid"];
                var content = (string)tableName["content"];

                result.Add(new top5
                {
                    UserId = userid,
                    Content = content,
                });
            }
            tableName.Close();
            database.CloseConnection();

            return result;

        }


        public static void DelTop5()
        {
            var database = new Database("discordbot");

            try
            {
                var strings = string.Format("DELETE FROM top5; ALTER TABLE top5 AUTO_INCREMENT = 1");
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                database.CloseConnection();
                return;
            }
        }

        public static List<top5> MakeTop5(int entry1, int entry2, int entry3, int entry4, int entry5)
        {
            var top5 = GetTop5(entry1, entry2, entry3, entry4, entry5);

            var database = new Database("discordbot");

            DelVotedfor();
            DelTop5();


            foreach (var entry in top5)
            {
                var replacedContent = entry.Content.Replace("'", "''");

                var str = string.Format("INSERT INTO top5(userid, content) VALUES('{0}', '{1}')", entry.UserId, replacedContent);
                var tableName = database.FireCommand(str);
                tableName.Close();
            }

            database.CloseConnection();

            return top5;

        }

        public static void DelVotedfor()
        {
            var database = new Database("discordbot");

            try
            {
                var strings = string.Format("DELETE FROM votedfor; ALTER TABLE top5 AUTO_INCREMENT = 1");
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                database.CloseConnection();
                return;
            }
        }

        public static void AddVote(IUser user, int vote)
        {
            UserCheck(user);

            var database = new Database("discordbot");

            var str = string.Format("INSERT INTO votedfor (userid, voted) VALUES ('{0}', '{1}')", user.Id, vote);
            var table = database.FireCommand(str);
            table.Close();
            database.CloseConnection();

            return;
        }

        public static void ChangeVote(IUser user, int vote)
        {
            var database = new Database("discordbot");

            try
            {
                var strings = string.Format("UPDATE votedfor SET voted = '{0}' WHERE userid = '{1}'", vote, user.Id);
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                database.CloseConnection();
                return;
            }
        }

        public static List<votedfor> GetVotedfor(IUser user)
        {
            var result = new List<votedfor>();

            var database = new Database("discordbot");

            var str = string.Format("SELECT * FROM votedfor WHERE userid = '{0}'", user.Id);
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var userid = (ulong)tableName["userid"];
                var voted = (int)tableName["voted"];

                result.Add(new votedfor
                {
                    UserId = userid,
                    Voted = voted,
                });
            }
            tableName.Close();
            database.CloseConnection();

            return result;

        }

        public static List<top5> ShowTop5()
        {
            var database = new Database("discordbot");


            var result = new List<top5>();
            var str = string.Format("SELECT * FROM top5");
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var userid = (ulong)tableName["userid"];
                var content = (string)tableName["content"];

                result.Add(new top5
                {
                    UserId = userid,
                    Content = content,
                });
            }
            tableName.Close();
            database.CloseConnection();

            return result;
        }

        public static List<top5> GetTop5ByID(int id)
        {
            var result = new List<top5>();

            var database = new Database("discordbot");

            var str = string.Format("SELECT * FROM top5 WHERE id = '{0}'", id);
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var userid = (ulong)tableName["userid"];
                var content = (string)tableName["content"];

                result.Add(new top5
                {
                    UserId = userid,
                    Content = content,
                });
            }
            tableName.Close();
            database.CloseConnection();

            return result;

        }

        public static List<winners> GetWinners()
        {
            var result = new List<winners>();

            var database = new Database("discordbot");

            for (int i = 1; i < 6; i++)
            {
                var str = string.Format("SELECT * FROM votedfor WHERE voted = '{0}'", i);
                var tableName = database.FireCommand(str);

                var votes = 0;

                while (tableName.Read())
                {
                    votes++;
                }

                result.Add(new winners
                {
                    ID = i,
                    Votes = votes,
                });

                tableName.Close();
            }

            List<winners> SortedList = result.OrderBy(o => o.Votes).ToList();
            SortedList.Reverse();

            database.CloseConnection();

            return SortedList;

        }


        public static bool CheckExistingTheme(int id)
        {
            var database = new Database("discordbot");

            var str = string.Format("SELECT * FROM weeklytheme WHERE ID = '{0}'", id);
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                tableName.Close();
                database.CloseConnection();
                return true;
            }

            tableName.Close();
            database.CloseConnection();

            return false;
        }

        public static List<weeklytheme> GetWeeklytheme(int id)
        {
            var result = new List<weeklytheme>();

            var database = new Database("discordbot");

            var str = string.Format("SELECT * FROM weeklytheme WHERE ID = '{0}'", id);
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var theme = (string)tableName["theme"];
                var url = (string)tableName["URL"];

                result.Add(new weeklytheme
                {
                    Theme = theme,
                    URL = url,
                });
            }
            tableName.Close();
            database.CloseConnection();

            return result;

        }

    }

}
