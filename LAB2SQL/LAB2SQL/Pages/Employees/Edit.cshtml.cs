using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace LAB2SQL.Pages.Employees
{
    public class EditModel(IConfiguration config) : PageModel
    {
        public Employee Employee = new Employee();
        public List<DropDownList> companies = new List<DropDownList>();
        public List<DropDownList> professions = new List<DropDownList>();
        public string ErrorMessage = "";
        public string SuccessMessage = "";
        private string connectionString = config.GetConnectionString("MySqlConnection");
        public void OnGet()
        {
            string allProfessions = "select pavadinimas from profesijos";
            string allCompanies = "select pavadinimas from transportavimo_kompanijos";

            using (MySqlConnection connect = new MySqlConnection(connectionString))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand(allProfessions, connect))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Add each record as a SelectListItem to the list
                            professions.Add(new DropDownList
                            {
                                Text = reader.GetString(0)
                            });
                        }
                    }
                }
                using (MySqlCommand command = new MySqlCommand(allCompanies, connect))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Add each record as a SelectListItem to the list
                            companies.Add(new DropDownList
                            {
                                Text = reader.GetString(0)
                            });
                        }
                    }
                }
            }
            try
            {
                string id = Request.Query["id"];
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT d.*, t.pavadinimas, p.pavadinimas FROM darbuotojai d " +
                        "JOIN profesijos p ON d.fk_profesija = p.id " +
                        "JOIN transportavimo_kompanijos t ON t.id = d.fk_KOMPANIJA " +
                        "where d.id = @id";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {

                            if (reader.Read())
                            {
                                Employee.id = reader.GetInt32(0);
                                Employee.name = reader.GetString(1);
                                Employee.surname = reader.GetString(2);
                                Employee.salary = reader.GetDecimal(3);
                                Employee.start_date = reader.GetDateTime(4).ToString("yyyy-MM-dd");
                                Employee.phone = reader.GetString(5);
                                Employee.email = reader.GetString(6);
                                Employee.birthday = reader.GetDateTime(7).ToString("yyyy-MM-dd");
                                Employee.company = reader.GetString(10);
                                Employee.profession = reader.GetString(11);
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
        public void OnPost()
        {
            string id = Request.Query["id"];
            Employee.id = int.Parse(id);
            Employee.name = Request.Form["name"];
            Employee.surname = Request.Form["surname"];
            Employee.salary = Convert.ToDecimal(Request.Form["salary"]);
            Employee.start_date = Request.Form["start_date"];
            Employee.phone = Request.Form["phone"];
            Employee.email = Request.Form["email"];
            Employee.birthday = Request.Form["birthday"];
            string company = Request.Form["company"];
            string profession = Request.Form["profession"];


            if (Employee.name.Length == 0 || Employee.surname.Length == 0 ||
                Employee.phone.Length == 0 || Employee.email.Length == 0
                || Employee.salary == 0 || Employee.start_date.Length == 0)
            {
                ErrorMessage = "Visi laukai privalo būti užpildyti";
                OnGet();
                return;
            }

            try
            {
                using (MySqlConnection connect = new MySqlConnection(connectionString))
                {
                    connect.Open();
                    int companyID, professionID;
                    string companiesQuery = "select id from transportavimo_kompanijos where pavadinimas = @company";
                    string professionQuery = "select id from profesijos where pavadinimas = @profession";

                    using (MySqlCommand cmd = new MySqlCommand(professionQuery, connect))
                    {
                        cmd.Parameters.AddWithValue("@profession", profession);
                        professionID = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    using (MySqlCommand cmd1 = new MySqlCommand(companiesQuery, connect))
                    {
                        cmd1.Parameters.AddWithValue("@company", company);
                        companyID = Convert.ToInt32(cmd1.ExecuteScalar());
                    }

                    string sql = "UPDATE darbuotojai " +
                                 "SET id = @id, " +
                                 "vardas = @name, " +
                                 "pavardė = @surname, " +
                                 "atlyginimas = @salary, " +
                                 "įsidarbinimo_data = @start_date, " +
                                 "tel_nr = @phone, " +
                                 "email = @email, " +
                                 "gimimo_data = @birthday, " +
                                 "fk_KOMPANIJA = @companyId, " +
                                 "fk_profesija = @professionId " +
                                 "WHERE id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(sql, connect))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@name", Employee.name);
                        cmd.Parameters.AddWithValue("@surname", Employee.surname);
                        cmd.Parameters.AddWithValue("@salary", Employee.salary);
                        cmd.Parameters.AddWithValue("@start_date", Employee.start_date);
                        cmd.Parameters.AddWithValue("@phone", Employee.phone);
                        cmd.Parameters.AddWithValue("@email", Employee.email);
                        cmd.Parameters.AddWithValue("@birthday", Employee.birthday);
                        cmd.Parameters.AddWithValue("@companyId", companyID);
                        cmd.Parameters.AddWithValue("@professionId", professionID);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return;
            }
            SuccessMessage = "Atnaujinta informacija";
        }
    }
}
