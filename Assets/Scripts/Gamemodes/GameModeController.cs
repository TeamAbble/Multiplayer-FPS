using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Unity.Collections;
using Unity.Services.Relay;
public class GameModeController : NetworkBehaviour
{
    public static GameModeController instance;

    public readonly SyncVar<int> numberOfTeams = new(new(WritePermission.ServerOnly));

    public string[] teamNames;
    public float gameTimeMax;

    public readonly SyncVar<int[]> scores = new(new() { WritePermission = WritePermission.ServerOnly});
    public readonly SyncTimer inGameTimer = new();
    public readonly SyncVar<bool> gameInProgress = new(new(WritePermission.ServerOnly));
    public readonly SyncVar<bool> gameWaitingToStart = new(new(WritePermission.ServerOnly));
    public readonly SyncTimer pregameTimer = new(new(WritePermission.ServerOnly, ReadPermission.Observers));
    public float startTimer;

    private void Start()
    {
        
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(PreGameStartTimer());
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
