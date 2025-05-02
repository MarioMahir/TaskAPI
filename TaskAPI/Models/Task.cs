namespace TaskAPI.Models
{
    public class Task<T>
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public T ExtraData { get; set; }
    }
}
