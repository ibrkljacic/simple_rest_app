using Npgsql;
using RZNU_SimpleRest.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;

namespace RZNU_SimpleRest.Controllers
{
    public class UserController : ApiController
    {
        /// <summary>
        ///     Get all users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [BasicAuthentication]
        [Route("User")]
        public HttpResponseMessage Get()
        {
            writeToLog("/User", getBrowser());

            List<User> allUsers = new List<User>();
            User u = new User();
            int adminID = 1;
            string username = Thread.CurrentPrincipal.Identity.Name;
            string myConnectionString = "Server=localhost;Port=5432;User ID=postgres;Password=58596878;Database=RZNU_rest";
            using (var conn = new NpgsqlConnection(myConnectionString))
            {
                conn.Open();
                string query = String.Format("SELECT id FROM users WHERE username = '{0}';", username);
                var cmd = new NpgsqlCommand(query, conn);
                int userId = int.Parse(cmd.ExecuteScalar().ToString());
                if (userId != adminID)
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                else
                {
                    query = String.Format("SELECT * FROM users;");
                    using (cmd = new NpgsqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                u = new User();
                                u.Id = reader.GetInt32(0);
                                u.UserName = reader.GetString(1);
                                u.Password = reader.GetString(2);
                                allUsers.Add(u);
                            }
                        }
                    }
                    conn.Close();
                    if (allUsers.Capacity == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, allUsers);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, allUsers);
                    }
                }
            }
        }

        /// <summary>
        ///     Get a specific user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [BasicAuthentication]
        [Route("User/{userID}")]
        public HttpResponseMessage Get(int userID)
        {
            writeToLog("/User/"+userID ,getBrowser());

            User u = new User();
            int adminID = 1;
            string username = Thread.CurrentPrincipal.Identity.Name;
            string myConnectionString = "Server=localhost;Port=5432;User ID=postgres;Password=58596878;Database=RZNU_rest";
            using (var conn = new NpgsqlConnection(myConnectionString))
            {
                conn.Open();
                string query = String.Format("SELECT id FROM users WHERE username = '{0}';", username);
                var cmd = new NpgsqlCommand(query, conn);
                int userId = int.Parse(cmd.ExecuteScalar().ToString());
                if (userId != userID && userId != adminID)
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                else
                {
                    query = String.Format("SELECT * FROM users WHERE id = '{0}';", userID);
                    using (cmd = new NpgsqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                u = new User();
                                u.Id = reader.GetInt32(0);
                                u.UserName = reader.GetString(1);
                                u.Password = reader.GetString(2);
                            }
                        }
                    }
                    conn.Close();
                    if (u.Id == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, u);
                    }
                }
            }
        }

        /// <summary>
        ///     Add a new user
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("User")]
        public HttpResponseMessage Post([FromBody]User u)
        {
            writeToLog("/User/", getBrowser());

            int lastInsertedID;

            string myConnectionString = "Server=localhost;Port=5432;User ID=postgres;Password=58596878;Database=RZNU_rest";
            using (var conn = new NpgsqlConnection(myConnectionString))
            {
                conn.Open();
                string query = String.Format("INSERT INTO users (username, password) VALUES ('{0}', '{1}') RETURNING id;", u.UserName, u.Password);
                var cmd = new NpgsqlCommand(query, conn);
                lastInsertedID = int.Parse(cmd.ExecuteScalar().ToString());
                conn.Close();
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);
            response.Headers.Location = new Uri(Request.RequestUri, lastInsertedID.ToString());
            return response;
        }

        /// <summary>
        ///     Edit a specific user
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="u"></param>
        /// <returns></returns>
        [HttpPut]
        [BasicAuthentication]
        [Route("User/{userID}")]
        public HttpResponseMessage Put(int userID, [FromBody]User u)
        {
            writeToLog("/User/" + userID, getBrowser());

            int adminID = 1;
            string username = Thread.CurrentPrincipal.Identity.Name;
            string myConnectionString = "Server=localhost;Port=5432;User ID=postgres;Password=58596878;Database=RZNU_rest";
            using (var conn = new NpgsqlConnection(myConnectionString))
            {
                conn.Open();
                string query = String.Format("SELECT id FROM users WHERE username = '{0}';", username);
                var cmd = new NpgsqlCommand(query, conn);
                int userId = int.Parse(cmd.ExecuteScalar().ToString());
                if (userId != userID && userId != adminID)
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                else
                {
                    string searchQuery = String.Format("SELECT * FROM users WHERE id = '{0}';", userID);
                    using (cmd = new NpgsqlCommand(searchQuery, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                reader.Close();
                                string updateQuery = String.Format("UPDATE users SET username = '{0}', password = '{1}' WHERE id = '{2}';", u.UserName, u.Password, userID);
                                var cmdDelete = new NpgsqlCommand(updateQuery, conn);
                                cmdDelete.ExecuteNonQuery();
                                conn.Close();
                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                conn.Close();
                                return Request.CreateResponse(HttpStatusCode.NotFound);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Delete a specific user
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [HttpDelete]
        [BasicAuthentication]
        [Route("User/{userID}")]
        public HttpResponseMessage Delete(int userID)
        {
            writeToLog("/User/" + userID, getBrowser());

            int adminID = 1;
            string username = Thread.CurrentPrincipal.Identity.Name;
            string myConnectionString = "Server=localhost;Port=5432;User ID=postgres;Password=58596878;Database=RZNU_rest";
            using (var conn = new NpgsqlConnection(myConnectionString))
            {
                conn.Open();
                string query = String.Format("SELECT id FROM users WHERE username = '{0}';", username);
                var cmd = new NpgsqlCommand(query, conn);
                int userId = int.Parse(cmd.ExecuteScalar().ToString());
                if (userId != userID && userId != adminID)
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                else
                {
                    string searchQuery = String.Format("SELECT * FROM users WHERE id = '{0}';", userID);
                    using (cmd = new NpgsqlCommand(searchQuery, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                reader.Close();
                                string deleteQuery = String.Format("DELETE FROM users WHERE id = '{0}';", userID);
                                var cmdDelete = new NpgsqlCommand(deleteQuery, conn);
                                cmdDelete.ExecuteNonQuery();
                                conn.Close();
                                return Request.CreateResponse(HttpStatusCode.NoContent);
                            }
                            else
                            {
                                conn.Close();
                                return Request.CreateResponse(HttpStatusCode.NotFound);
                            }
                        }
                    }
                }
            }
        }

        public static string getBrowser()
        {
            var userAgent = HttpContext.Current.Request.UserAgent;
            var userBrowser = new HttpBrowserCapabilities { Capabilities = new Hashtable { { string.Empty, userAgent } } };
            var factory = new BrowserCapabilitiesFactory();
            factory.ConfigureBrowserCapabilities(new NameValueCollection(), userBrowser);
            var bb = userBrowser.Browser;
            return bb.ToLower();
        }

        private void writeToLog(string command, string browser)
        {
            string line = command + "\t" + browser;
            StreamWriter writeFS = new StreamWriter(@"C:\Users\Brklja\Desktop\logREST.txt", true);
            writeFS.WriteLine(line);
            writeFS.Close();

        }

    }
}
