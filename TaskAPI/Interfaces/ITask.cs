using Microsoft.VisualBasic;

namespace TaskAPI.Interfaces
{
    public interface ITask
    {
        string Description { get; set; }
        DateTime DueDate { get; set; }
        bool IsCompleted { get; set; }
        object ExtraData { get; set; }
    }
}
