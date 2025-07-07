namespace LunchRush.API.Models;

public class LunchSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "open"; // "open" or "locked"
    public Restaurant? Restaurant { get; set; } // Deprecated
    public List<Restaurant> Restaurants { get; set; } = new();
    public List<Participant> Participants { get; set; } = new();
    public string? OrderPlacer { get; set; }
    public DateTime LockTime { get; set; } = DateTime.UtcNow.AddMinutes(30);
    public bool Locked { get; set; } = false;
}