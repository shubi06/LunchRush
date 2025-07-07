using Dapr.Client;
using LunchRush.API.Models;

namespace LunchRush.API.Services;

public class SessionService
{
    private const string StoreName = "statestore";
    private readonly DaprClient _dapr;

    public SessionService(DaprClient daprClient)
    {
        _dapr = daprClient;
    }

    public async Task<LunchSession?> GetSessionAsync(string id)
        => await _dapr.GetStateAsync<LunchSession>(StoreName, id);

    public async Task SaveSessionAsync(LunchSession session)
        => await _dapr.SaveStateAsync(StoreName, session.Id, session);

    public async Task DeleteSessionAsync(string id)
        => await _dapr.DeleteStateAsync(StoreName, id);
}