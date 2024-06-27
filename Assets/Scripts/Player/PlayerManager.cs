using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player Managers handle input for both the physical player and the spectator camera.
/// </summary>
public class PlayerManager : NetworkBehaviour
{
    [SerializeField] internal PhysicalPlayer phys;
    [SerializeField] internal SpectatorPlayer spec;

    public Behaviour[] disableOnRemote;
    public Behaviour[] disableOnLocal;

    public readonly SyncVar<int> teamNumber = new(new(WritePermission.ServerOnly));

    [SerializeField] internal Vector2 moveInput;
    [SerializeField] internal Vector2 lookInput;



    public override void OnStartClient()
    {
        base.OnStartClient();
        if (LocalConnection.IsLocalClient)
        {
            for (int i = 0; i < disableOnLocal.Length; i++)
            {
                disableOnLocal[i].enabled = false;
            }
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            for (int i = 0; i < disableOnLocal.Length; i++)
            {
                disableOnLocal[i].enabled = false;
            }
        }
    }

    private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
    {
        if (GameModeController.instance && LocalConnection.IsLocalClient)
        {
            print("Getting spawn point!");
            GameModeController.instance.teamMembers.Add(LocalConnection, GameModeController.instance.TeamToJoin());

            Transform spawnpoint = GameModeController.instance.teamSpawnAreas[teamNumber.Value].RandomSpawnPoint();
            Vector2 randomPoint = Random.insideUnitCircle * GameModeController.instance.teamSpawnAreas[teamNumber.Value].randomSpawnRadius;
            if (phys)
            {
                phys.transform.SetPositionAndRotation(spawnpoint.position + new Vector3(randomPoint.x, 0, randomPoint.y), spawnpoint.rotation);
            }
            if (spec)
            {
                spec.transform.SetPositionAndRotation(spawnpoint.position + new Vector3(randomPoint.x, 3, randomPoint.y), spawnpoint.rotation);
            }
        }
        else
        {
            if (LocalConnection.IsLocalClient)
            {
                print("Cannot find Game Mode Controller??");
            }
            else
            {
                print("This isn't the local client, i think");
            }
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (LocalConnection.IsLocalClient)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            GameManager.Instance.LoadScene(GameManager.Instance.menuScene);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public override void OnStartServer()
    {
        print("Starting player manager");
        base.OnStartServer();
    }

    public override void OnStopServer()
    {
        print("stopping player manager");
        base.OnStopServer();
        if (GameModeController.instance)
        {
            GameModeController.instance.teamMembers.Remove(LocalConnection);
        }
    }


    public void MoveInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void LookInput(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (!LocalConnection.IsLocalClient)
        {
            return;
        }
        if (!GameModeController.instance)
        {
            print("GAME MODE CONTROLLER NOT FOUND!");
            return;
        }
        if (GameModeController.instance.gameWaitingToStart.Value)
        {
            print("Spectator Player");
            if (spec)
            {
                spec.moveInput = moveInput;
                spec.lookInput = lookInput;
            }
            if (phys)
            {
                phys.moveInput = Vector2.zero;
                phys.lookInput = Vector2.zero;
            }

        }
        else if (GameModeController.instance.gameInProgress.Value)
        {
            print("Physical Player");
            if (spec)
            {
                spec.moveInput = Vector2.zero;
                spec.lookInput = Vector2.zero;
            }
            if (phys)
            {
                phys.moveInput = moveInput;
                phys.lookInput = lookInput;
            }
        }
        else
        {
            print("NULL PLAYER");
            if (spec)
            {
                spec.moveInput = Vector2.zero;
                spec.lookInput = Vector2.zero;
            }
            if (phys)
            {
                phys.moveInput = Vector2.zero;
                phys.lookInput = Vector2.zero;
            }
        }
    }
}
