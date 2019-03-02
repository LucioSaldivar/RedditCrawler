using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Data.SqlClient;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace RedditCrawler
{
    class Program
    {
        public static void Main()
        {            
            MySql.Data.MySqlClient.MySqlConnection conn;
            string myConnectionString;

            myConnectionString = "server=127.0.0.1;uid=redditcrawler;" +                            // string is used to connect to MySQL
                "pwd=Lucio420;database=redditcra";

            try
            {
                conn = new MySqlConnection();
                conn.ConnectionString = myConnectionString;
                Console.WriteLine("database connection open");
                conn.Open();
                string[] subs = {"MuayThai","computerscience","botany" };                   //subreddit subjects crawler will be looking for 

                foreach (string sub in subs)
                {

                    Console.WriteLine(sub);
                    Task<string> task = getResponse(sub);
                    dynamic link = JsonConvert.DeserializeObject(task.Result);              // Must deserialize in order to make response readable.
                    foreach (var d in link.data.children)
                    {

                        var id = d.data.id;
                        var subject = d.data.subreddit;
                        var title = d.data.title;
                        var uLink = d.data.url;
                        var utc = d.data.created_utc;
                        var subId = d.data.subreddit_id;

                        var cmd = new MySqlCommand();
                        cmd.Connection = conn;                                      // used to open connection to MySQL

                        // Parameterized queries - prevents string from breaking off dude to string compromise. 
                        cmd.CommandText = "INSERT INTO articles(article_id,subject,subid,title) VALUES(?article_id,?subject,?subid,?title)";
                        cmd.Parameters.Add("?article_id", MySqlDbType.VarChar).Value = id;
                        cmd.Parameters.Add("?subject", MySqlDbType.VarChar).Value = subject;
                        cmd.Parameters.Add("?subid", MySqlDbType.VarChar).Value = subId;
                        cmd.Parameters.Add("?title", MySqlDbType.VarChar).Value = title;
                        cmd.ExecuteNonQuery();

                        if(title == "new")
                        {
                            Console.WriteLine("post" + title);
                        }
                    }
                }              
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
                Console.WriteLine(ex.Message);
            }                  
        }                                                        // End of Main

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
        }                                                               // This Method Calls for HTTP link
    }
}

