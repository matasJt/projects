using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace LAB2SQL.Pages.Employees
{
    public class IndexModel(IConfiguration config) : PageModel
    {
        private string connectionString = config.GetConnectionString("MySqlConnection");
        public List<Employee> emplList = new List<Employee>();
        public int PageSize = 12;
        public int TotalPages;
        public int CurrentPage;
        public void OnGet(int current_page = 1)
        {
            int offset = (current_page - 1) * PageSize;
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string countQuery = "SELECT COUNT(*) FROM darbuotojai";
                    using (MySqlCommand countCommand = new MySqlCommand(countQuery, connection))
                    {
                        int totalRecords = Convert.ToInt32(countCommand.ExecuteScalar());
                        TotalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);
                    }

                    string query = "SELECT d.*, t.pavadinimas, p.pavadinimas FROM darbuotojai d " +
                        "JOIN profesijos p ON d.fk_profesija = p.id " +
                        "JOIN transportavimo_kompanijos t ON t.id = d.fk_KOMPANIJA " +
                        "limit @pageSize offset @offset";
                       
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@pageSize", PageSize);
                        command.Parameters.AddWithValue("@offset", offset);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                Employee employee = new Employee();
                                employee.id = reader.GetInt32(0);
                                employee.name = reader.GetString(1);
                                employee.surname = reader.GetString(2);
                                employee.salary = reader.GetDecimal(3);
                                employee.start_date = reader.GetDateTime(4).ToString("yyyy-MM-dd");
                                employee.phone = reader.GetString(5);
                                employee.email = reader.GetString(6);
                                employee.birthday = reader.GetDateTime(7).ToString("yyyy-MM-dd");
                                employee.company = reader.GetString(10);
                                employee.profession = reader.GetString(11);
                                emplList.Add(employee);
                            }

                        }
                        CurrentPage = current_page;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}
public class Employee
{
    public int id;
    public string? name;
    public string? surname;
    public decimal salary;
    public string? start_date;
    public string? phone;
    public string? email;
    public string? birthday;
    public string? company;
    public string? profession;

}