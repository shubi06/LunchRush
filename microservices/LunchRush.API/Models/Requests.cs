namespace LunchRush.API.Models;

public class VoteRequest
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public string RestaurantId { get; set; }
}

public class JoinRequest
{
    public string UserId { get; set; }
    public string Username { get; set; }
}

public class MealUpdateRequest
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public string? MealChoice { get; set; }
    public string? Meal { get; set; }
}

public class RestaurantProposal
{
    public string Name { get; set; }
    public string ProposedBy { get; set; }
    public string? UserId { get; set; }
}

public class OrderPlacerRequest
{
    public string? UserId { get; set; }
    public string Username { get; set; }
}

public class HeartbeatRequest
{
    public string UserId { get; set; }
    public string Username { get; set; }
}