using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using System;
using System.ComponentModel.Design;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace LAB2SQL.Pages.Employees
{
    public class DropDownList
    {
        public string Text { get; set; }
    }
    public class CreateModel(IConfiguration config) : PageModel
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
           
        }
        public void OnPost()
        {
            Employee.Name = Request.Form["name"];
            Employee.Surname = Request.Form["surname"];
            Employee.Salary = Convert.ToDecimal(Request.Form["salary"]);
            Employee.Start_date = Request.Form["start_date"];
            Employee.Phone = Request.Form["phone"];
            Employee.Email = Request.Form["email"];
            Employee.Birthday = Request.Form["birthday"];
            string company = Request.Form["company"];
            string profession = Request.Form["profession"];


            if (Employee.Name.Length == 0 || Employee.Surname.Length == 0 ||
                Employee.Phone.Length == 0 || Employee.Email.Length == 0
                || Employee.Salary == 0 || Employee.Start_date.Length == 0)
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

                    string maxIDSql = "select MAX(id) from darbuotojai";
                    int maxId = 1;
                    using (MySqlCommand cmd = new MySqlCommand(maxIDSql, connect))
                    {
                        maxId = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    int newId = maxId + 1;
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


                    string sql = "INSERT INTO darbuotojai " +
                                 "(id, vardas, pavardė, atlyginimas, įsidarbinimo_data, tel_nr, email, gimimo_data, fk_KOMPANIJA, fk_profesija) " +
                                 "VALUES " +
                                 "(@id, @name, @surname, @salary, @start_date, @phone, @email, @birthday, @companyId, @professionId)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, connect))
                    {
                        cmd.Parameters.AddWithValue("@id", newId);
                        cmd.Parameters.AddWithValue("@name", Employee.Name);
                        cmd.Parameters.AddWithValue("@surname", Employee.Surname);
                        cmd.Parameters.AddWithValue("@salary", Employee.Salary);
                        cmd.Parameters.AddWithValue("@start_date", Employee.Start_date);
                        cmd.Parameters.AddWithValue("@phone", Employee.Phone);
                        cmd.Parameters.AddWithValue("@email", Employee.Email);
                        cmd.Parameters.AddWithValue("@birthday", Employee.Birthday);
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
            Employee.Surname = "";
            Employee.Salary = 0;
            Employee.Start_date = "";
            Employee.Phone = "";
            Employee.Email = "";
            Employee.Birthday = "";
            SuccessMessage = "Naujas darbuotojas buvo pridėtas";

        }
    }
}
