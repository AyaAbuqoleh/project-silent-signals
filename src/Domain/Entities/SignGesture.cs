namespace Domain.Entities;

public class SignGesture
{
    public int Id { get; set; }

    public string TokenType { get; set; } = "Letter";
 
    public string Token { get; set; } = default!;
    
    public string? ImageUrl { get; set; }
    public string? DescriptionAR { get; set; }
    public string? DescriptionEN { get; set; }
}
