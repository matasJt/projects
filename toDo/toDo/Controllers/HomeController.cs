using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using toDo.Models;
using toDo.WebDbContext;

namespace toDo.Controllers
{
    public class HomeController : Controller
    {

        private readonly AppDbContext dbContext;

        public HomeController(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var tasks = dbContext.tasks.ToList();
            TodoTaskViewModel model = new TodoTaskViewModel
            {
                Tasks = tasks
            };
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(TodoTaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                dbContext.tasks.Add(new TodoTasks { Name = model.NewTask.Name, IsDone = false, dateTime = DateTime.Now });
                dbContext.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CheckAsDone(TodoTaskViewModel model)
        {

            foreach (var item in model.Tasks)
            {
                dbContext.tasks.Update(new TodoTasks { Id = item.Id, Name = item.Name, IsDone = item.IsDone, dateTime = item.dateTime });
            }
            dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult DeleteTasks()
        {
            dbContext.tasks.Where(t => t.IsDone).ToList().ForEach(t => dbContext.tasks.Remove(t));
            dbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        [Route("Home/Test", Name = "Custom")]
        public string Test()
        {
            return "Test";
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
