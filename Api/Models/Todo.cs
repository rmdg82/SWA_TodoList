namespace Api.Models;

public class Todo
{
    public string Id { get; set; }
    public string Text { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}