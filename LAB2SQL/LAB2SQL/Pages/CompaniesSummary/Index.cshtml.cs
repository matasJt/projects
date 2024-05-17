using LAB2SQL.Pages.Employees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace LAB2SQL.Pages.CompaniesSummary
{
    public class Filter
    {
        public string Company { get; set; }
        public string Profession { get; set; }
        public string dateFrom { get; set; }
        public string dateTo { get; set; }
    }
    public class Company
    {
        public string Name { get; set; }
        public int Revenue { get; set; }
        public int MaxSalary { get; set; }
        public List<Employee> Employees = new List<Employee>();
    }
    [IgnoreAntiforgeryToken]
    public class IndexModel(IConfiguration config) : PageModel
    {
        public List<Company> queryCompanies = new List<Company>();
        public List<string> companies = new List<string>();
        public List<string> professions = new List<string>();
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
                            professions.Add(reader.GetString(0));
                        }
                    }
                }
                using (MySqlCommand command = new MySqlCommand(allCompanies, connect))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            companies.Add(reader.GetString(0));
                        }
                    }
                }
            }
        }
        public IActionResult OnPostFilterData()
        {
            string companyFrom = Request.Form["company"];
            string profession = Request.Form["profession"];
            string dateFrom = Request.Form["dateFrom"];
            string dateTo = Request.Form["dateTo"];
            ViewData["Company"] = companyFrom;
            ViewData["Profession"] = profession;
            ViewData["dateTo"] = dateTo;
            ViewData["dateFrom"] = dateFrom;
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string filterQuery = "SELECT d.vardas, d.`pavardė`, " +
                        "(SELECT pavadinimas FROM profesijos p WHERE p.id = d.fk_profesija) AS profesija, " +
                        "COUNT(IF(MONTH(u.`užsakymo_data`)>= @dateFrom AND MONTH(u.`užsakymo_data`) <= @dateTo, 1, NULL)) AS uzsakymu_kiekis, " +
                        "d.atlyginimas, " +
                        "ROUND(AVG(d.atlyginimas) OVER (PARTITION BY t.pavadinimas, d.fk_profesija), 0) AS vidutinis_atlyginimas_pagal_profesija, " +
                        "MAX(d.atlyginimas) OVER (PARTITION BY t.pavadinimas, d.fk_profesija) AS didziausias_atlyginimas_pagal_profesija, " +
                        "MAX(d.atlyginimas) OVER (PARTITION BY t.pavadinimas) AS didziausias_atlyginimas_kompanijoje, " +
                        "t.pavadinimas AS kompanija, " +
                        "(SELECT SUM(m.suma) " +
                        "FROM klientai k " +
                        "JOIN mokėjimai m ON k.id = m.fk_KLIENTAS " +
                        "JOIN užsakymai u ON m.id = u.`fk_MOKĖJIMAS` " +
                        "WHERE u.fk_KOMPANIJA = t.id) AS pajamos_is_uzsakymu " +
                        "FROM darbuotojai d " +
                        "left JOIN užsakymai u ON u.fk_VAIRUOTOJAS = d.id " +
                        "join transportavimo_kompanijos t ON d.fk_KOMPANIJA = t.id " +
                        "GROUP BY t.pavadinimas, d.id ";

                    List<string> valuesForFilter = new List<string>();


                    if (!string.IsNullOrEmpty(companyFrom))
                    {
                        valuesForFilter.Add(" kompanija = @company ");
                    }
                    if (!string.IsNullOrEmpty(profession))
                    {
                        valuesForFilter.Add(" profesija = @profession ");
                    }
                    if (valuesForFilter.Count > 0)
                    {
                        filterQuery += " HAVING " + string.Join(" AND ", valuesForFilter);
                    }

                    using (MySqlCommand command = new MySqlCommand(filterQuery, connection))
                    {

                        command.Parameters.AddWithValue("@company", companyFrom);
                        command.Parameters.AddWithValue("@profession",profession);
                        command.Parameters.AddWithValue("@dateFrom", dateFrom.Substring(6,1));
                        command.Parameters.AddWithValue("@dateTo", dateTo.Substring(6,1));
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Employee employee = new Employee();
                                employee.Name = reader.GetString(0);
                                employee.Surname = reader.GetString(1);
                                employee.Profession = reader.GetString(2);
                                employee.OrdersCount = reader.GetInt32(3);
                                employee.Salary = reader.GetInt32(4);
                                employee.AverageSalaryByProfession = reader.GetInt32(5);
                                employee.MaxSalaryByProfession = reader.GetInt32(6);

                                string companyName = reader.GetString(8);
                                int maxSalary = reader.GetInt32(7);
                                int revenue = reader.GetInt32(9);


                                Company company = queryCompanies.Find(x => x.Name == companyName);
                                if (company == null)
                                {
                                    Company newCompany = new Company();
                                    newCompany.Name = companyName;
                                    newCompany.MaxSalary = maxSalary;
                                    newCompany.Revenue = revenue;
                                    queryCompanies.Add(newCompany);
                                    newCompany.Employees.Add(employee);
                                }
                                else
                                {
                                    company.Employees.Add(employee);
                                }

                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }
            OnGet();
            return Page();
        }
    }
}
