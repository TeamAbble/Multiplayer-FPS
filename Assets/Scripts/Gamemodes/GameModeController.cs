using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Unity.Collections;
using Unity.Services.Relay;
using FishNet.Connection;
public class GameModeController : NetworkBehaviour
{
    public static GameModeController instance;
    [System.Serializable]
    public class Spawnpoints
    {
        public Transform[] spawnpoints;
        public Transform RandomSpawnPoint()
        {
            return spawnpoints[Random.Range(0, spawnpoints.Length)];
        }
        public float randomSpawnRadius;
    }
    public readonly SyncVar<int> numberOfTeams = new(new(WritePermission.ServerOnly));

    public string[] teamNames;
    public float gameTimeMax;

    public readonly SyncVar<int[]> scores = new(new() { WritePermission = WritePermission.ServerOnly});
    public readonly SyncTimer inGameTimer = new();
    public readonly SyncVar<bool> gameInProgress = new(new(WritePermission.ServerOnly));
    public readonly SyncVar<bool> gameWaitingToStart = new(new(WritePermission.ServerOnly));
    public readonly SyncTimer pregameTimer = new(new(WritePermission.ServerOnly, ReadPermission.Observers));
    public float startTimer;

    public Color[] teamColours;

    public readonly SyncDictionary<NetworkConnection, int> teamMembers = new(new(WritePermission.ServerOnly));
    public Spawnpoints[] teamSpawnAreas;
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        var players = FindObjectsByType<BasePlayer>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
        if(players.Length > 0)
        {
            foreach( var player in players)
            {
                player.SetColour();
            }
        }
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(PreGameStartTimer());
    }
    public int TeamToJoin()
    {
        int teamToJoin = -1;
        //We need to figure out how many teams there are, which is indicated by how many team names there are.
        //I know its stupid, but it works for now.
        //I don't think I need to write anything for that bit

        if(teamMembers.Count > 0)
        {
            //We now need to figure out how many players are on each team.
            int[] playersOnEachTeam = new int[teamNames.Length];
            foreach (var item in teamMembers)
            {
                playersOnEachTeam[item.Value]++;
            }

            //Now we need to figure out which has the least players.
            int smallestTeamNumber = 100;
            int smallestTeamIndex = -1;
            for (int i = 0; i < playersOnEachTeam.Length; i++)
            {
                if (playersOnEachTeam[i] < smallestTeamNumber)
                {
                    smallestTeamIndex = i;
                }
            }
            teamToJoin = smallestTeamIndex;
        }
        else
        {
            //We have no existing teams, so we'll just join team 0.
            teamToJoin = 0;
        }

        return teamToJoin;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        teamMembers.Clear();
        
    }
    public override void OnStopClient()
    {
        print("This probably means the host has disappeared, or the player has been removed from the game");
        base.OnStopClient();
    }


    IEnumerator PreGameStartTimer()
    {
        gameWaitingToStart.Value = true;
        pregameTimer.StartTimer(startTimer);
        while (pregameTimer.Remaining > 0)
        {
            yield return new WaitForSeconds(1);
            pregameTimer.Update(1);
        }
        StartGame();
        yield break;
    }
    public void StartGame()
    {
        gameInProgress.Value = true;
        gameWaitingToStart.Value = false;
        inGameTimer.StartTimer(gameTimeMax);
    }
    private void FixedUpdate()
    {
        if (GameUIController.Instance)
        {
            if (gameInProgress.Value)
            {
                inGameTimer.Update(Time.fixedDeltaTime);
                GameUIController.Instance.timerText.text = $"{(inGameTimer.Remaining / 60) % 60:00}:{inGameTimer.Remaining % 60:00}";
            }
            else if (gameWaitingToStart.Value)
            {
                GameUIController.Instance.timerText.text = $"{(pregameTimer.Remaining / 60) % 60:00}:{pregameTimer.Remaining % 60:00}";
            }
        }
        if (IsServerInitialized)
        {
            if (gameInProgress.Value && inGameTimer.Remaining <= 0)
            {
                gameInProgress.Value = false;
            }
        }
    }
    [ServerRpc]
    public void AddScoreToTeam(int teamIndex, int score)
    {
        scores.Value[teamIndex] += score;
    }
}
