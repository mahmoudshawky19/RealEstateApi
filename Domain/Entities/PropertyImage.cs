namespace Domain.Entities;

public class PropertyImage
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public int PropertyId { get; set; }

    public Property Property { get; set; } = null!;
}
