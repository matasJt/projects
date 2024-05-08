using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.ComponentModel;

namespace LAB2SQL.Pages.Clients
{
    public class CreateModel(IConfiguration config) : PageModel
    {
        public Client Client = new Client();
        public string ErrorMessage = "";
        public string SuccessMessage = "";
        private string connectionString = config.GetConnectionString("MySqlConnection");

        public void OnGet()
        {
        }
        public void OnPost()
        {
            Client.name = Request.Form["name"];
            Client.surname = Request.Form["surname"];
            Client.phone = Request.Form["phone"];
            Client.email = Request.Form["email"];

            if(Client.name.Length == 0 || Client.surname.Length == 0 ||
                Client.phone.Length == 0 || Client.email.Length == 0)
            {
                ErrorMessage = "Visi laukai privalo būti užpildyti";
                return;
            }

            try
            {
                using(MySqlConnection connect = new MySqlConnection(connectionString))
                {
                    connect.Open();

                    string maxIDSql = "select MAX(id) from klientai";
                    int maxId;
                    using(MySqlCommand cmd = new MySqlCommand(maxIDSql, connect))
                    {
                        maxId = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    int newId = maxId + 1;

                    string sql = "insert into klientai" +
                        "(id, vardas, pavardė, tel_nr, email, fk_PRISTATYMAS) VALUES " +
                        "(@id, @name, @surname, @phone, @email, 20)";
                    using(MySqlCommand cmd = new MySqlCommand(sql, connect))
                    {
                        cmd.Parameters.AddWithValue("@id", newId);
                        cmd.Parameters.AddWithValue("@name", Client.name);
                        cmd.Parameters.AddWithValue("@surname", Client.surname);
                        cmd.Parameters.AddWithValue("@phone", Client.phone);
                        cmd.Parameters.AddWithValue("@email", Client.email);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return;
            }
            Client.name = "";
            Client.surname = "";
            Client.email = "";
            Client.phone = "";
            SuccessMessage = "Naujas klientas buvo sukurtas";

        }
    }
}
