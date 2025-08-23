using System.Threading.Tasks;
using Unity.Services.Multiplayer;
using UnityEngine;

public static class LobbyManager
{
    public static async Task CreateLobby()
    {
        var lobby = await MultiplayerService.Instance.CreateOrJoinSessionAsync("MyLobby", new SessionOptions());
        Debug.Log("Lobby created with ID: " + lobby.Id);
    }

    public static async Task JoinLobby(string lobbyId)
    {
        var lobby = await MultiplayerService.Instance.JoinSessionByIdAsync(lobbyId);
        Debug.Log("Joined lobby: " + lobby.Id);
    }

    public static async Task ListLobbies()
    {
        var lobbies = await MultiplayerService.Instance.QuerySessionsAsync(new QuerySessionsOptions());
        Debug.Log($"Found {lobbies.Sessions.Count} lobbies.");
        foreach (var lobby in lobbies.Sessions)
            Debug.Log($"Lobby: {lobby.Id}, Players: {lobby.MaxPlayers - lobby.AvailableSlots}/{lobby.MaxPlayers}");
    }
}