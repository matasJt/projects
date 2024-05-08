using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace LAB2SQL.Pages.Cargo
{
    public class DeleteModel : PageModel
    {
        public void OnGet()
        {
            string id;
            try
            {
                id = Request.Query["id"];
                using (MySqlConnection connection = new MySqlConnection("Server=localhost;Database=kroviniai;Uid=root;Pwd=ripsas;Charset=utf8;"))
                {
                    connection.Open();
                    string deleteGoods = "delete from prekės where id = @id";
                    using (MySqlCommand command = new MySqlCommand(deleteGoods, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
            TempData["deleted"] = $"Prekė {id} buvo pašalinta";
            Response.Redirect("/Cargo/Index");
        }
    }
}
