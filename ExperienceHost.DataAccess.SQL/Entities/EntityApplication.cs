using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using ExperienceHost.DataAccess.SQL.Structures;

namespace ExperienceHost.DataAccess.SQL.Entities;

public class EntityApplication : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    
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

    public override string ToString()
    {
#if DEBUG
        var builder = new StringBuilder();

        builder.AppendLine($"{nameof(EntityApplication)}:");
        builder.AppendLine($"  {nameof(Name)}: {Name}");
        
        builder.AppendLine($"  {nameof(FilePath)}: {FilePath}");
        builder.AppendLine($"  {nameof(Arguments)}: {Arguments}");
        builder.AppendLine($"  {nameof(ImagePath)}: {ImagePath}");
        builder.AppendLine($"  {nameof(PositionX)}: {PositionX}");
        builder.AppendLine($"  {nameof(PositionY)}: {PositionY}");
        builder.AppendLine(
            $"  {nameof(Position)}: X={Position.X}, Y={Position.Y}");
        builder.AppendLine($"  {nameof(TabItemId)}: {TabItemId}");
        builder.AppendLine(
            $"  {nameof(TabItem)}: {TabItem?.GetType().Name ?? "null"}");

        return builder.ToString().TrimEnd();
#else
        return Name;
#endif
    }
}