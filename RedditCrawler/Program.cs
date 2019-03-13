using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace RedditCrawler
{
    class Program
    {
        public static void Main()
        {
            MySql.Data.MySqlClient.MySqlConnection conn;
            string myConnectionString;
            // string is used to connect to MySQL
            myConnectionString = "server=127.0.0.1;uid=redditcrawler;" +
                "pwd=Lucio420;database=redditcra";

            try
            {
                conn = new MySqlConnection();
                conn.ConnectionString = myConnectionString;
                Console.WriteLine("database connection open");
                conn.Open();


                string query = "SELECT * FROM subreddit";
             

                var comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                var reader = comm.ExecuteReader();


                while (reader.Read())
                {
                    string[] subs = { reader["name"].ToString() };
                    Console.WriteLine("\t{0}",
                    reader[1]);

                    foreach (string sub in subs)
                    {

                        Console.WriteLine(sub);
                        //Asynchronous oporation. This will return request.
                        Task<string> task = getResponse(sub);
                        // Must deserialize in order to make response readable.
                        Console.WriteLine(task.Result);
                        dynamic link = JsonConvert.DeserializeObject(task.Result);
                        foreach (var d in link.data.children)
                        {

                            var id = d.data.id;
                            var subject = d.data.subreddit;
                            var title = d.data.title;
                            var subId = d.data.subreddit_id;

                            MySqlConnection connection = new MySqlConnection(myConnectionString);
                            MySqlCommand cmd;
                            connection.Open();
                            cmd = connection.CreateCommand();

                            // Parameterized queries - prevents string from breaking off dude to string compromise. 
                            cmd.CommandText = "INSERT INTO articles(article_id,subject,subid,title) VALUES(?article_id,?subject,?subid,?title)";
                            cmd.Parameters.Add("?article_id", MySqlDbType.VarChar).Value = id;
                            cmd.Parameters.Add("?subject", MySqlDbType.VarChar).Value = subject;
                            cmd.Parameters.Add("?subid", MySqlDbType.VarChar).Value = subId;
                            cmd.Parameters.Add("?title", MySqlDbType.VarChar).Value = title;
                            cmd.ExecuteNonQuery();

                            if (title == "new")
                            {
                                Console.WriteLine("post" + title);
                            }
                        }
                    }

                }
                reader.Close();
                //subreddit subjects crawler will be looking for




            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
                Console.WriteLine(ex.Message);
            }
        }// End of 



        // This Request for the Http site.
        static async Task<string> getResponse(string sub)
        {
            using (HttpClient client = new HttpClient())
            {
                string subreddit = "https://www.reddit.com/r/" + sub + "/new/.json";
                try
                {
                    HttpResponseMessage response = await client.GetAsync(subreddit);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
                catch (HttpRequestException e)
                {
                    return e.Message;
                }
            }
        }
    }
}

