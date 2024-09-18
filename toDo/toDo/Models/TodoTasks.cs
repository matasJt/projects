using System.ComponentModel.DataAnnotations;

namespace toDo.Models
{
    public class TodoTasks
    {
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public bool IsDone { get; set; }
        public DateTime dateTime { get; set; }
    }
    public class TodoTaskViewModel
    {
        public List<TodoTasks> Tasks { get; set; } = new List<TodoTasks>();
        public TodoTasks NewTask { get; set; } = new TodoTasks();
    }
}
