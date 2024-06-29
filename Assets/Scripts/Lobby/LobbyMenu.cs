using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using TMPro;
public class LobbyMenu : MonoBehaviour
{
    private List<LobbyButton> lobbyButtons = new();
    public GameObject lobbyButtonPrefab;
    public Transform buttonRoot;
    public float buttonUpdateTime = 30;
    public Button joinLobbyButton;
    public GameObject joiningScreen;
    LobbyButton currentLobbyButton;
    private void Start()
    {
        StartCoroutine(UpdateButtons());
    }
    IEnumerator UpdateButtons()
    {
        RefreshLobbies();
        while (true)
        {
            yield return new WaitForSeconds(buttonUpdateTime);
            RefreshLobbies();
        }
    }
    public async void RefreshLobbies()
    {
        try
        {
            if (lobbyButtons.Count > 0)
            {
                for (int i = 0; i < lobbyButtons.Count; i++)
                {
                    Destroy(lobbyButtons[i].gameObject);
                }
                lobbyButtons.Clear();
            }

            var l = await Lobbies.Instance.QueryLobbiesAsync();
            if(l.Results.Count > 0)
            {
                for (int i = 0; i < l.Results.Count; i++)
                {
                    Lobby lobby = l.Results[i];
                    var lb = Instantiate(lobbyButtonPrefab, buttonRoot).GetComponent<LobbyButton>();
                    lb.AssignButton(lobby);
                    lb.button.onClick.AddListener(() => SelectLobby(lb));
                    lobbyButtons.Add(lb);
                }
            }
            currentLobbyButton = null;
            joinLobbyButton.interactable = false;

        }
        catch (System.Exception)
        {
            throw;
        }
    }
    public void SelectLobby(LobbyButton targetedButton)
    {
        if (targetedButton == currentLobbyButton)
        {
            currentLobbyButton = null;
            joinLobbyButton.interactable = false;
            joinLobbyButton.GetComponentInChildren<TMP_Text>().text = "None Selected";
        }
        else
        {
            currentLobbyButton = targetedButton;
            joinLobbyButton.GetComponentInChildren<TMP_Text>().text = "Join " + targetedButton.targetLobby.Name;
            joinLobbyButton.interactable = true;
        }
    }
    public async void JoinTargetedLobby()
    {
        try
        {
            joiningScreen.SetActive(true);
            await ConnectionManager.Instance.JoinGameLobbyViaCode(currentLobbyButton.targetLobby.LobbyCode);
        }
        catch (System.Exception)
        {
            joiningScreen.SetActive(false);
            throw;
        }
    }

    public async void JoinRandomLobby()
    {

            joiningScreen.SetActive(true);
            QueryResponse lobbies;
            try
            {
                lobbies = await Lobbies.Instance.QueryLobbiesAsync();
            }
            catch (System.Exception)
            {

                throw;
            }
            int randomLobby = Random.Range(0, lobbies.Results.Count);

            try
            {
                await ConnectionManager.Instance.JoinGameLobbyViaCode(lobbies.Results[randomLobby].LobbyCode);
            }
            catch (System.Exception)
            {
                joiningScreen.SetActive(false);
                throw;
            }
    }
    public async void CreateLobby()
    {
        print("starting lobby...");
        joiningScreen.SetActive(true);
        joiningScreen.GetComponentInChildren<TMP_Text>().text = "Creating Lobby!";
        try
        {
            await ConnectionManager.Instance.StartGameLobby();
        }
        catch (System.Exception)
        {
            joiningScreen.SetActive(false);
            throw;
        } 
    }
}
