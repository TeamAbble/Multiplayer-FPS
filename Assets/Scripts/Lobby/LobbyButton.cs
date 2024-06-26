using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyButton : MonoBehaviour
{
    public TMP_Text playerNumberText, mapNameText, lobbyNameText;
    public Button button;
    public Lobby targetLobby;
    public string joinCode;
    public void AssignButton(Lobby targetLobby)
    {
        this.targetLobby = targetLobby;
        joinCode = targetLobby.LobbyCode;
        UpdateText();
    }
    public void UpdateText()
    {
        if (!string.IsNullOrEmpty(targetLobby.Id))
        {
            playerNumberText.text = $"Players:\n{targetLobby.Players.Count}/{targetLobby.MaxPlayers}";
            mapNameText.text = targetLobby.Data[ConnectionManager.MAPNAMEKEY].Value;
            lobbyNameText.text = targetLobby.Name;
        }
    }
}
