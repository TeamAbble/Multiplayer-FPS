using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Unity.Collections;
using Unity.Services.Relay;
using FishNet.Connection;
using System.Linq;
using Unity.VisualScripting;
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
    public struct TeamPlayer
    {
        public NetworkConnection conn;
        public int teamNumber;
    }

    public string[] teamNames;
    public float gameTimeMax;

    public readonly SyncVar<int[]> scores = new(new() { WritePermission = WritePermission.ServerOnly});
    public readonly SyncTimer inGameTimer = new();
    public readonly SyncVar<bool> gameInProgress = new(new(WritePermission.ServerOnly));
    public readonly SyncVar<bool> gameWaitingToStart = new(new(WritePermission.ServerOnly));
    public readonly SyncTimer pregameTimer = new(new(WritePermission.ServerOnly, ReadPermission.Observers));
    public float startTime;

    public Color[] teamColours;

    public Spawnpoints[] teamSpawnAreas;

    public readonly SyncList<NetworkConnection> blueTeamMembers = new(new(WritePermission.ServerOnly)), redTeamMembers = new(new(WritePermission.ServerOnly));
    public Spawnpoints redSpawnpoints, blueSpawnpoints;
    public Color redTeamColour, blueTeamColour;

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
        gameWaitingToStart.Value = true;
        pregameTimer.StartTimer(startTime);
    }
    public void JoinTeam(NetworkConnection connection)
    {
        //Figure out which team to join
        if(blueTeamMembers.Count > redTeamMembers.Count)
        {
            redTeamMembers.Add(connection);
        }
        else
        {
            blueTeamMembers.Add(connection);
        }
    }
    public void LeaveTeam(NetworkConnection connection)
    {
        if (redTeamMembers.Contains(connection))
            redTeamMembers.Remove(connection);
        else if(blueTeamMembers.Contains(connection))
            blueTeamMembers.Remove(connection);
    }
    public Spawnpoints GetSpawnPoint(NetworkConnection connection)
    {
        if (redTeamMembers.Contains(connection))
        {
            return redSpawnpoints;
        }
        if (blueTeamMembers.Contains(connection))
        {
            return blueSpawnpoints;
        }
        return null;
    }
    public Color TeamColour(NetworkConnection connection)
    {
        if (redTeamMembers.Contains(connection))
            return redTeamColour;
        else
            return blueTeamColour;
    }
    public override void OnStopServer()
    {
        base.OnStopServer();
        
    }
    public override void OnStopClient()
    {
        print("This probably means the host has disappeared, or the player has been removed from the game");
        base.OnStopClient();
    }


    IEnumerator PreGameStartTimer()
    {
        gameWaitingToStart.Value = true;
        pregameTimer.StartTimer(startTime);
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
        gameWaitingToStart.Value = false;
        inGameTimer.StartTimer(gameTimeMax);
        gameInProgress.Value = true;
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
                pregameTimer.Update(Time.fixedDeltaTime);
                GameUIController.Instance.timerText.text = $"{(pregameTimer.Remaining / 60) % 60:00}:{pregameTimer.Remaining % 60:00}";

            }
        }
        if (IsServerInitialized)
        {
            if (gameInProgress.Value && inGameTimer.Remaining < 0 && inGameTimer.Elapsed > 0)
            {
                gameInProgress.Value = false;
            }
            if (pregameTimer.Remaining <= 0 && pregameTimer.Elapsed != 0)
            {
                StartGame();
            }
        }
    }
    [ServerRpc]
    public void AddScoreToTeam(int teamIndex, int score)
    {
        scores.Value[teamIndex] += score;
    }
}
