using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace LAB2SQL.Pages.Employees
{
    public class DeleteModel : PageModel
    {
        public void OnGet()
        {
            string id;
            string page = Request.Query["current_page"];
            try
            {
                id = Request.Query["id"];
                using (MySqlConnection connection = new MySqlConnection("Server=localhost;Database=kroviniai;Uid=root;Pwd=ripsas;Charset=utf8;"))
                {
                    connection.Open();
                    string deleteFromMultiple = "delete from vairuoja where fk_VAIRUOTOJAS = @id ";
                    string deleteFromOrders = "update užsakymai set fk_VAIRUOTOJAS = NULL where fk_VAIRUOTOJAS = @id";
                    string deleteFromEmployees = "delete from darbuotojai where id=@id";

                    using (MySqlCommand command1 = new MySqlCommand(deleteFromMultiple, connection))
                    using (MySqlCommand command2 = new MySqlCommand(deleteFromOrders, connection))
                    using (MySqlCommand finalDelete = new MySqlCommand(deleteFromEmployees, connection))
                    {
                        command1.Parameters.AddWithValue("@id", id);
                        command2.Parameters.AddWithValue("@id", id);
                        finalDelete.Parameters.AddWithValue("@id", id);
                        command1.ExecuteNonQuery();
                        command2.ExecuteNonQuery();
                        finalDelete.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
            TempData["deleted"] = $"Darbuotojas {id} buvo pašalintas";
            Response.Redirect($"/Employees?current_page={page}");
        }
    }
}
