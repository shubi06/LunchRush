using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using LunchRush.API.Models;
using LunchRush.API.Services;

namespace LunchRush.API.Controllers;

[ApiController]
[Route("sessions")]
public class SessionsController : ControllerBase
{
    private readonly DaprClient _dapr;
    private readonly SessionService _sessionService;
    private const string PubSub = "pubsub";

    public SessionsController(DaprClient daprClient)
    {
        _dapr = daprClient;
        _sessionService = new SessionService(_dapr);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSession()
    {
        var session = new LunchSession();

        await _sessionService.SaveSessionAsync(session);
        await _dapr.PublishEventAsync(PubSub, "session.created", session);
        await _dapr.SaveStateAsync("statestore", "today-session", session);

        return Ok(session);
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetTodaySession()
    {
        var session = await _dapr.GetStateAsync<LunchSession>("statestore", "today-session");
        return session is not null ? Ok(session) : NotFound();
    }

    [HttpPost("{id}/lock")]
    public async Task<IActionResult> LockSession(string id)
    {
        var session = await _sessionService.GetSessionAsync(id);
        if (session is null) return NotFound();

        session.Status = "locked";
        session.Locked = true;
        await _sessionService.SaveSessionAsync(session);
        await _dapr.PublishEventAsync(PubSub, "session.locked", session);

        return Ok(session);
    }
}