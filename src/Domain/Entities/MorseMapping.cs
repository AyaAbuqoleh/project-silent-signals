namespace Domain.Entities;

public class MorseMapping
{
    public int Id { get; set; }
    
    public string Char { get; set; } = default!;
    
    public string Pattern { get; set; } = default!;
}
