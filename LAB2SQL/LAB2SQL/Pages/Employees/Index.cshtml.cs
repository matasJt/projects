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
                                employee.Id = reader.GetInt32(0);
                                employee.Name = reader.GetString(1);
                                employee.Surname = reader.GetString(2);
                                employee.Salary = reader.GetDecimal(3);
                                employee.Start_date = reader.GetDateTime(4).ToString("yyyy-MM-dd");
                                employee.Phone = reader.GetString(5);
                                employee.Email = reader.GetString(6);
                                employee.Birthday = reader.GetDateTime(7).ToString("yyyy-MM-dd");
                                employee.Company = reader.GetString(10);
                                employee.Profession = reader.GetString(11);
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
    public int Id {  get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public decimal Salary { get; set; }
    public string? Start_date { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Birthday { get; set; }
    public string? Company { get; set; }
    public string? Profession { get; set; }
    public int OrdersCount { get; set; }
    public int AverageSalaryByProfession { get; set; }
    public int MaxSalaryByProfession { get; set; }

}