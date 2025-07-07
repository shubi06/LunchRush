using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using LunchRush.API.Models;
using LunchRush.API.Services;

namespace LunchRush.API.Controllers;

[ApiController]
[Route("sessions/{sessionId}/[controller]")]
public class RestaurantsController : ControllerBase
{
    private readonly DaprClient _dapr;
    private readonly SessionService _sessionService;
    private const string PubSub = "pubsub";

    public RestaurantsController(DaprClient daprClient)
    {
        _dapr = daprClient;
        _sessionService = new SessionService(_dapr);
    }

    [HttpGet]
    public async Task<IActionResult> GetRestaurants(string sessionId)
    {
        var session = await _sessionService.GetSessionAsync(sessionId);
        return session is null ? NotFound() : Ok(session.Restaurants);
    }

    [HttpPost]
    public async Task<IActionResult> Propose(string sessionId, [FromBody] RestaurantProposal request)
    {
        var session = await _sessionService.GetSessionAsync(sessionId);
        if (session is null) return NotFound();

        var restaurant = new Restaurant
        {
            Name = request.Name,
            ProposedBy = request.ProposedBy,
            Voters = new List<string> { request.ProposedBy },
            Votes = 1
        };

        session.Restaurants.Add(restaurant);
        await _sessionService.SaveSessionAsync(session);
        await _dapr.PublishEventAsync(PubSub, "restaurant.proposed", restaurant);

        return Ok(restaurant);
    }

    [HttpPost("{restaurantId}/vote")]
    public async Task<IActionResult> Vote(string sessionId, string restaurantId, [FromBody] VoteRequest request)
    {
        var session = await _sessionService.GetSessionAsync(sessionId);
        if (session is null) return NotFound();

        var restaurant = session.Restaurants.FirstOrDefault(r => r.Id == restaurantId);
        if (restaurant is null) return NotFound();

        if (restaurant.Voters.Contains(request.Username))
        {
            restaurant.Voters.Remove(request.Username);
            restaurant.Votes--;
        }
        else
        {
            restaurant.Voters.Add(request.Username);
            restaurant.Votes++;
        }

        await _sessionService.SaveSessionAsync(session);
        await _dapr.PublishEventAsync(PubSub, "restaurant.voted", restaurant);

        return Ok(restaurant);
    }
}
