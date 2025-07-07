using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using LunchRush.API.Models;
using LunchRush.API.Services;

namespace LunchRush.API.Controllers;

[ApiController]
[Route("sessions/{sessionId}/[controller]")]
public class ParticipantsController : ControllerBase
{
    private readonly DaprClient _dapr;
    private readonly SessionService _sessionService;
    private const string Store = "statestore";
    private const string PubSub = "pubsub";

    public ParticipantsController(DaprClient daprClient)
    {
        _dapr = daprClient;
        _sessionService = new SessionService(_dapr);
    }

    [HttpPost("join")]
    public async Task<IActionResult> Join(string sessionId, [FromBody] JoinRequest request)
    {
        var session = await _sessionService.GetSessionAsync(sessionId);
        if (session is null) return NotFound();

        if (session.Participants.Any(p => p.UserId == request.UserId))
            return Ok(session);

        session.Participants.Add(new Participant
        {
            UserId = request.UserId,
            Username = request.Username,
            JoinedAt = DateTime.UtcNow,
            LastSeen = DateTime.UtcNow,
            IsActive = true
        });

        await _sessionService.SaveSessionAsync(session);
        await _dapr.PublishEventAsync(PubSub, "participant.joined", request);

        return Ok(session);
    }

    [HttpPut("meal")]
    public async Task<IActionResult> UpdateMeal(string sessionId, [FromBody] MealUpdateRequest request)
    {
        var session = await _sessionService.GetSessionAsync(sessionId);
        if (session is null) return NotFound();

        var participant = session.Participants.FirstOrDefault(p => p.UserId == request.UserId);
        if (participant is null) return NotFound();

        participant.MealChoice = request.Meal ?? request.MealChoice ?? "";
        participant.Meal = participant.MealChoice;
        await _sessionService.SaveSessionAsync(session);
        await _dapr.PublishEventAsync(PubSub, "meal.updated", participant);

        return Ok(session);
    }

    [HttpPut("order-placer")]
    public async Task<IActionResult> SetOrderPlacer(string sessionId, [FromBody] OrderPlacerRequest request)
    {
        var session = await _sessionService.GetSessionAsync(sessionId);
        if (session is null) return NotFound();

        foreach (var p in session.Participants)
            p.IsOrderPlacer = p.Username == request.Username;

        session.OrderPlacer = request.Username;
        await _sessionService.SaveSessionAsync(session);
        await _dapr.PublishEventAsync(PubSub, "orderplacer.set", request);

        return Ok(session);
    }

    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat(string sessionId, [FromBody] HeartbeatRequest request)
    {
        var session = await _sessionService.GetSessionAsync(sessionId);
        if (session is null) return NotFound();

        var user = session.Participants.FirstOrDefault(p => p.UserId == request.UserId);
        if (user is not null)
        {
            user.LastSeen = DateTime.UtcNow;
            user.IsActive = true;
            await _sessionService.SaveSessionAsync(session);
        }

        return Ok();
    }

    [HttpDelete("{username}")]
    public async Task<IActionResult> Remove(string sessionId, string username)
    {
        var session = await _sessionService.GetSessionAsync(sessionId);
        if (session is null) return NotFound();

        session.Participants.RemoveAll(p => p.Username == username);
        await _sessionService.SaveSessionAsync(session);
        await _dapr.PublishEventAsync(PubSub, "participant.left", username);

        return Ok(session);
    }
}
