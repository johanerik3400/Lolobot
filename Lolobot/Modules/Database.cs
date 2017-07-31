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

    }

}
