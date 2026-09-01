namespace ExperienceHost.DataAccess.SQL.Entities;

public class EntityTabItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public List<EntityApplication> Applications { get; set; } = [];
}