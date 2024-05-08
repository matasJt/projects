using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Mozilla;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using static Org.BouncyCastle.Math.EC.ECCurve;


namespace LAB2SQL.Pages.Cargo
{
    public class SelectedValues
    {
        public string GoodsNames { get; set; }
        public string SellersName { get; set; }
    }
    [IgnoreAntiforgeryToken]
    public class IndexModel(IConfiguration config) : PageModel
    {
        [BindProperty]
        public string SelectedOrder { get; set; }
        [BindProperty]
        public int Count { get; set; }

        public List<string> goodsNames = new List<string>();
        public List<string> goodsSellers = new List<string>();
        public List<string> orderList = new List<string>();
        public List<Goods> goodsList = new List<Goods>();
        public Goods goodsCreate = new Goods();
        public List<SelectedValues> selectedValues = new List<SelectedValues>();

        private string connectionString = config.GetConnectionString("MySqlConnection");
        public string ErrorMessage = "";
        public string SuccessMessage = "";
        public Cargo cargo = new Cargo();
        public void OnGet()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "select užsakymo_nr, užsakymo_data from užsakymai";
                    string getNames = "select pavadinimas from pavadinimai";
                    string getSellers = "select įmonės_pavadinimas from pardavėjai";


                    using (MySqlCommand command1 = new MySqlCommand(getNames, connection))
                    using (MySqlCommand command2 = new MySqlCommand(getSellers, connection))
                    using (MySqlCommand command3 = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command3.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string orderString = "ID " + reader.GetInt32(0) + " " + reader.GetDateTime(1).ToString("yyyy-MM-dd");
                                orderList.Add(orderString);
                            }

                        }
                        using (MySqlDataReader reader = command1.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                goodsNames.Add(reader.GetString(0));

                            }

                        }
                        using (MySqlDataReader reader = command2.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                goodsSellers.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        public IActionResult OnPostUpdateData([FromBody] List<Goods> list)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {


                    connection.Open();
                    foreach (Goods goods in list)
                    {
                        int nameId, sellerId;
                        string sellerQuery = "select id from pardavėjai where įmonės_pavadinimas = @sellerName";
                        string goodQuery = "select id from pavadinimai where pavadinimas = @goodName";
                        string updateQuery = "update prekės " +
                                             "set pavadinimas=@name, " +
                                             "kaina_už_vnt=@pcount, " +
                                             "vnt=@count, " +
                                             "vnt_svoris=@weight, " +
                                             "fk_PARDAVĖJAS=@sellerId " +
                                             "where id=@id;";

                        using (MySqlCommand cmd = new MySqlCommand(sellerQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@sellerName", goods.seller);
                            sellerId = Convert.ToInt32(cmd.ExecuteScalar());
                            cmd.Parameters.Clear();
                        }
                        using (MySqlCommand cmd1 = new MySqlCommand(goodQuery, connection))
                        {
                            cmd1.Parameters.AddWithValue("@goodName", goods.name);
                            nameId = Convert.ToInt32(cmd1.ExecuteScalar());
                            cmd1.Parameters.Clear();
                        }

                        using (MySqlCommand update = new MySqlCommand(updateQuery, connection))
                        {
                            update.Parameters.AddWithValue("@name", nameId);
                            update.Parameters.AddWithValue("@pcount", goods.pricePerCount);
                            update.Parameters.AddWithValue("@count", goods.count);
                            update.Parameters.AddWithValue("@weight", goods.weightPerUnit);
                            update.Parameters.AddWithValue("@sellerId", sellerId);
                            update.Parameters.AddWithValue("@id", goods.id);
                            update.ExecuteNonQuery();
                        }

                    }

                    SuccessMessage = "Duomenys buvo išsaugoti";
                    OnGet();
                    OnPost();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }

        }

        public IActionResult OnPostAddToCargo([FromBody] List<Goods> list)
        {

            try
            {
                foreach(var goods in list)
                {
                    if (goods.pricePerCount == 0 || goods.count == 0 || goods.weightPerUnit == 0 || goods.name == null || goods.seller == null)
                    {
                        ErrorMessage = "Visi laukai turi buti uzpildyti";
                        return Redirect("/Index");
                    }
                }
                using (MySqlConnection connect = new MySqlConnection(connectionString))
                {
                    connect.Open();
                    foreach (var goods in list)
                    {
                        string maxIDSql = "select MAX(id) from prekės";
                        int maxId = 1;
                        using (MySqlCommand cmd = new MySqlCommand(maxIDSql, connect))
                        {
                            maxId = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        int newId = maxId + 1;
                        int nameId, sellerId, categoryId;
                        string sellerQuery = "select id from pardavėjai where įmonės_pavadinimas = @sellerName";
                        string goodQuery = "select id from pavadinimai where pavadinimas = @goodName";
                        string categoryQuery = "SELECT * FROM kategorijos " +
                                                "JOIN pavadinimai ON pavadinimai.fk_kategorija = kategorijos.id " +
                                                "WHERE pavadinimai.id = @nameId";


                        using (MySqlCommand cmd = new MySqlCommand(sellerQuery, connect))
                        {
                            cmd.Parameters.AddWithValue("@sellerName", goods.seller);
                            sellerId = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        using (MySqlCommand cmd1 = new MySqlCommand(goodQuery, connect))
                        {
                            cmd1.Parameters.AddWithValue("@goodName", goods.name);
                            nameId = Convert.ToInt32(cmd1.ExecuteScalar());
                        }
                        using (MySqlCommand cmd2 = new MySqlCommand(categoryQuery, connect))
                        {
                            cmd2.Parameters.AddWithValue("nameId", nameId);
                            categoryId = Convert.ToInt32(cmd2.ExecuteScalar());
                        }


                        string sql = "INSERT INTO prekės " +
                                      "(id, pavadinimas, kaina_už_vnt, vnt, vnt_svoris, kategorija, fk_KROVINYS, fk_PARDAVĖJAS) " +
                                      "VALUES " +
                                      "(@id, @nameId, @pricePerCount, @count, @weightPerUnit, @categoryId, @cargoId, @sellerId)";
                        using (MySqlCommand cmd = new MySqlCommand(sql, connect))
                        {
                            cmd.Parameters.AddWithValue("@id", newId);
                            cmd.Parameters.AddWithValue("@nameId", nameId);
                            cmd.Parameters.AddWithValue("@pricePerCount", goods.pricePerCount);
                            cmd.Parameters.AddWithValue("@count", goods.count);
                            cmd.Parameters.AddWithValue("@weightPerUnit", goods.weightPerUnit);
                            cmd.Parameters.AddWithValue("@categoryId", categoryId);
                            cmd.Parameters.AddWithValue("@cargoId", goods.id);
                            cmd.Parameters.AddWithValue("@sellerId", sellerId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
            return Page();
        }
        public void OnPost()
        {
            if (!string.IsNullOrEmpty(SelectedOrder))
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string selectGoods = "SELECT k.id, k.matmenys, k.tipas, k.`fk_UŽSAKYMAS`, p.id, pav.pavadinimas, p.`kaina_už_vnt`, p.vnt, p.vnt_svoris, par.`įmonės_pavadinimas` FROM prekės p " +
                        "join kroviniai k ON p.fk_KROVINYS = k.id " +
                        "JOIN pavadinimai pav ON p.pavadinimas = pav.id " +
                        "JOIN pardavėjai par ON p.`fk_PARDAVĖJAS` = par.id " +
                        "WHERE p.fk_KROVINYS = @orderId";
                    using (MySqlCommand command1 = new MySqlCommand(selectGoods, connection))
                    {
                        command1.Parameters.AddWithValue("@orderId", SelectedOrder);
                        using (MySqlDataReader reader = command1.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                cargo.id = reader.GetInt32(0);
                                cargo.dimensions = reader.GetString(1);
                                cargo.type = reader.GetString(2);
                                cargo.orderId = reader.GetInt32(3);
                            }
                        }
                        using (MySqlDataReader reader2 = command1.ExecuteReader())
                        {
                            while (reader2.Read())
                            {
                                Goods goods = new Goods();
                                goods.id = reader2.GetInt32(4);
                                goods.name = reader2.GetString(5);
                                goods.pricePerCount = reader2.GetInt32(6);
                                goods.count = reader2.GetInt32(7);
                                goods.weightPerUnit = reader2.GetInt32(8);
                                goods.seller = reader2.GetString(9);
                                var selectedValue = new SelectedValues
                                {
                                    GoodsNames = goods.name,
                                    SellersName = goods.seller
                                };
                                selectedValues.Add(selectedValue);
                                goodsList.Add(goods);

                            }
                        }
                    }
                }

            }
            OnGet();
        }
    }
    public class Cargo
    {
        public int id;
        public string? dimensions;
        public string? type;
        public int orderId;
    }
    public class Goods
    {
        public int id { get; set; }
        public string? name { get; set; }
        public int pricePerCount { get; set; }
        public int count { get; set; }
        public int weightPerUnit { get; set; }
        public string? seller { get; set; }

    }
}
