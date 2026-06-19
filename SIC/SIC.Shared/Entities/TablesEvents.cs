namespace SIC.Shared.Entities;

public class TablesEvents
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; } = null!;
    public ICollection<Invitation> Invitation { get; set; } = null!;
    public int Number { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Seats { get; set; }
    public int OccupiedSeats { get; set; }
    
}
