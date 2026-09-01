using System.ComponentModel.DataAnnotations.Schema;
using ExperienceHost.DataAccess.SQL.Structures;

namespace ExperienceHost.DataAccess.SQL.Entities;

public class EntityApplication : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Disk { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;

    public string Arguments { get; set; } = string.Empty;

    public string ImagePath { get; set; } = string.Empty;

    public int PositionX { get; set; }
    public int PositionY { get; set; }
    
    
    public Guid TabItemId { get; set; }
    public EntityTabItem TabItem { get; set; } = null!;
    
    [NotMapped]
    public Point2D Position
    {
        get => new(PositionX, PositionY);
        set
        {
            PositionX = value.X;
            PositionY = value.Y;
        }
    }
}