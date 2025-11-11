namespace geo_auth.Models;

internal record MachineData
{
    public Guid MachineId { get; set; }
    public string? Secret { get; set; }
}
