using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace LAB2SQL.Pages.Clients
{
    public class EditModel(IConfiguration config) : PageModel
    {
        public Client Client = new Client();
        public string ErrorMessage = "";
        public string SuccessMessage = "";
        private string connectionString = config.GetConnectionString("MySqlConnection");

        public void OnGet()
        {
            string id = Request.Query["id"];

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "select * from klientai where id=@id";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if(reader.Read())
                            {
                                Client.id = reader.GetInt32(0);
                                Client.name = reader.GetString(1);
                                Client.surname = reader.GetString(2);
                                Client.phone = reader.GetString(3);
                                Client.email = reader.GetString(4);
                                Client.fkDestination = reader.GetInt32(5);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return;
            }
        }
        public void OnPost() {

            string id = Request.Query["id"];
            Client.id = int.Parse(id);
            Client.name = Request.Form["name"];
            Client.surname = Request.Form["surname"];
            Client.phone = Request.Form["phone"];
            Client.email = Request.Form["email"];

            if (Client.name.Length == 0 || Client.surname.Length == 0 ||
               Client.phone.Length == 0 || Client.email.Length == 0)
            {
                ErrorMessage = "Visi laukai privalo būti užpildyti";
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "update klientai " +
                        "set vardas=@name, pavardė=@surname, tel_nr=@phone, email=@email " +
                        "where id=@id";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@name", Client.name);
                        cmd.Parameters.AddWithValue("@surname", Client.surname);
                        cmd.Parameters.AddWithValue("@phone", Client.phone);
                        cmd.Parameters.AddWithValue("@email", Client.email);
                        cmd.Parameters.AddWithValue("@id", Client.id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return;
            }
            SuccessMessage = "Kliento duomenys buvo atnaujinti";
        }

    }
}