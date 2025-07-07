namespace LunchRush.API.Models;

public class Restaurant
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public int Votes { get; set; } = 0;
    public string ProposedBy { get; set; }
    public List<string> Voters { get; set; } = new();
    
}