namespace LunchRush.API.Models;

public class Participant
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public string MealChoice { get; set; } = "";
    public string Meal { get; set; } = ""; // alias
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsOrderPlacer { get; set; } = false;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}