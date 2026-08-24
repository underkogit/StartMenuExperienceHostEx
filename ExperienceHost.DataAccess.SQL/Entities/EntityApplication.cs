namespace ExperienceHost.DataAccess.SQL.Entities;

public class EntityApplication : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Disk { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    
    public string Arguments { get; set; } = string.Empty;
    
    public string ImagePath { get; set; } = string.Empty;
}