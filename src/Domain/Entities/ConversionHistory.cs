namespace Domain.Entities;

public class ConversionHistory
{
    public int Id { get; set; }
    
    public Guid? UserId { get; set; }

    public string SourceText { get; set; } = default!;
   
    public string TargetSystem { get; set; } = "Morse";
  
    public string ResultPayload { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
