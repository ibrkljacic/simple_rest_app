using Npgsql;
using RZNU_SimpleRest.Models;
using System;


namespace RZNU_SimpleRest
{
    public class UserSecurity
    {
        public static bool Login(string username, string password)
        {
            User u = new User();
            string myConnectionString = "Server=localhost;Port=5432;User ID=postgres;Password=58596878;Database=RZNU_rest";
            using (var conn = new NpgsqlConnection(myConnectionString))
            {
                conn.Open();
                string query = String.Format("SELECT * FROM users WHERE username = '{0}' AND password = '{1}';", username, password);
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            conn.Close();
                            return true;
                        }
                        else
                        {
                            conn.Close();
                            return false;
                        }
                    }
                }
            }
        }
    }
}