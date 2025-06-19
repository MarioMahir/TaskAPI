using TaskAPI.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskAPI.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }

        public string ExtraData { get; set; }
    }
}
