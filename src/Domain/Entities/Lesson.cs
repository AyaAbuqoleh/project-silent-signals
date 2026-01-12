namespace Domain.Entities;

public class Lesson
{
    public int Id { get; set; }
   
    public string Topic { get; set; } = default!;
    public string TitleAR { get; set; } = default!;
    public string TitleEN { get; set; } = default!;
    public string BodyAR { get; set; } = default!;
    public string BodyEN { get; set; } = default!;
    public string? MediaUrl { get; set; }
}
