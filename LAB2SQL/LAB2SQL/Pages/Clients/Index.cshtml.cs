using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace LAB2SQL.Pages.Clients
{
    public class IndexModel(IConfiguration config) : PageModel
    {
        private string connectionString = config.GetConnectionString("MySqlConnection");
        public List<Client> clientsList = new List<Client>();
        public int PageSize = 12;
        public int TotalPages;
        public int CurrentPage;
        public void OnGet(int current_page = 1)
        {  
            int offset = (current_page -1) * PageSize;
            try
            {
				using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string countQuery = "SELECT COUNT(*) FROM klientai";
                    using (MySqlCommand countCommand = new MySqlCommand(countQuery, connection))
                    {
                        int totalRecords = Convert.ToInt32(countCommand.ExecuteScalar());
                        TotalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);
                    }
                    string query = "select * from klientai limit @pageSize offset @offset";
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@pageSize", PageSize);
                        command.Parameters.AddWithValue("@offset", offset);
                        using(MySqlDataReader reader = command.ExecuteReader())
                        {
                            while(reader.Read())
                            {
                                Client client = new Client();
                                client.id = reader.GetInt32(0);
                                client.name = reader.GetString(1);
                                client.surname = reader.GetString(2);
                                client.phone = reader.GetString(3);
                                client.email = reader.GetString(4);
                                client.fkDestination = reader.GetInt32(5);

                                clientsList.Add(client);
                            }
                        }
                    }
                }
                CurrentPage = current_page;
			}
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
        }
    }
    public class Client
    {
        public int id;
        public string? name;
        public string? surname;
        public string? phone;
        public string? email;
        public int fkDestination;
	}
}
