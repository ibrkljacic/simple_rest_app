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
    public class PhotosController : ApiController
    {

        /// <summary>
        ///     Get all photos
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Photos")]
        public HttpResponseMessage Get()
        {
            writeToLog("/Photos", getBrowser());

            List<Photo> allPhotos = new List<Photo>();
            Photo p = new Photo();
            string myConnectionString = "Server=localhost;Port=5432;User ID=postgres;Password=58596878;Database=RZNU_rest";
            using (var conn = new NpgsqlConnection(myConnectionString))
            {
                conn.Open();
                string query = String.Format("SELECT id, userid, url FROM photos;");
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            p = new Photo();
                            p.ID = reader.GetInt32(0);
                            p.UserID = reader.GetInt32(1);
                            p.Url = reader.GetString(2);
                            allPhotos.Add(p);
                        }
                    }
                }
                conn.Close();
            }
            if (allPhotos.Capacity == 0)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, allPhotos);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, allPhotos);
            }
        }


        /// <summary>
        ///     Get all photos from specific user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("User/{userID}/Photos")]
        public HttpResponseMessage Get(int userID)
        {
            writeToLog("/User/" + userID + "/Photos", getBrowser());

            List<Photo> usersPhotos = new List<Photo>();
            Photo p = new Photo();
            string myConnectionString = "Server=localhost;Port=5432;User ID=postgres;Password=58596878;Database=RZNU_rest";
            using (var conn = new NpgsqlConnection(myConnectionString))
            {
                conn.Open();
                string query = String.Format("SELECT id, userid, url FROM photos WHERE userid = '{0}';", userID);
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            p = new Photo();
                            p.ID = reader.GetInt32(0);
                            p.UserID = reader.GetInt32(1);
                            p.Url = reader.GetString(2);
                            usersPhotos.Add(p);
                        }
                    }
                }
                conn.Close();
            }
            if (usersPhotos.Capacity == 0)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            return Request.CreateResponse(HttpStatusCode.OK, usersPhotos);
        }

        /// <summary>
        ///     Get specific photo from specific user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("User/{userID}/Photos/{photoID}")]
        public HttpResponseMessage Get(int userID, int photoID)
        {
            writeToLog("/User/" + userID + "/Photos/" + photoID, getBrowser());

            Photo p = new Photo();
            string myConnectionString = "Server=localhost;Port=5432;User ID=postgres;Password=58596878;Database=RZNU_rest";
            using (var conn = new NpgsqlConnection(myConnectionString))
            {
                conn.Open();
                string query = String.Format("SELECT id, userid, url FROM photos WHERE userid = '{0}' AND id = '{1}';", userID, photoID);
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            p = new Photo();
                            p.ID = reader.GetInt32(0);
                            p.UserID = reader.GetInt32(1);
                            p.Url = reader.GetString(2);
                            conn.Close();
                            return Request.CreateResponse(HttpStatusCode.OK, p);
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

        /// <summary>
        ///     Edit url of specific photo
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [BasicAuthentication]
        [Route("User/{userID}/Photos/{photoID}")]
        public HttpResponseMessage Put(int userID, int photoID, [FromBody]Photo p)
        {
            writeToLog("/User/" + userID + "/Photos/" + photoID, getBrowser());

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
                    conn.Close();
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                else
                {
                    string searchQuery = String.Format("SELECT * FROM photos WHERE id = '{0}';", photoID);
                    using (cmd = new NpgsqlCommand(searchQuery, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                reader.Close();
                                string updateQuery = String.Format("UPDATE photos SET url = '{0}' WHERE id = '{1}' AND userid = '{2}';", p.Url, photoID, userID);
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
        ///     Add a new photo to user
        /// </summary>
        /// <returns></returns>
        [HttpPost]       
        [BasicAuthentication]
        [Route("User/{userID}/Photos")]
        public HttpResponseMessage Post([FromBody]Photo p, int userID)
        {
            writeToLog("/User/" + userID + "/Photos", getBrowser());

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
                    conn.Close();
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                else
                {
                    query = String.Format("INSERT INTO photos (userid, url) VALUES ('{0}', '{1}');", userId, p.Url);
                    cmd = new NpgsqlCommand(query, conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    return Request.CreateResponse(HttpStatusCode.Created);
                }
            }
        }

        /// <summary>
        ///     Delete a specific photo of a user 
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [BasicAuthentication]
        [Route("User/{userID}/Photos/{photoID}")]
        public HttpResponseMessage Delete(int userID, int photoID)
        {
            writeToLog("/User/" + userID + "/Photos/" + photoID, getBrowser());

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
                    conn.Close();
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                else
                {
                    string searchQuery = String.Format("SELECT * FROM photos WHERE userid = '{0}' AND id = '{1}';", userID, photoID);
                    using (cmd = new NpgsqlCommand(searchQuery, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                reader.Close();
                                string deleteQuery = String.Format("DELETE FROM photos WHERE userid = '{0}' AND id = '{1}';", userID, photoID);
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
