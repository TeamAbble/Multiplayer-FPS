using FishNet.Object;
using FishNet.Object.Synchronizing;
using Unity.Cinemachine;
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
    public int TeamNumber;
    [SerializeField] internal Vector2 moveInput;
    [SerializeField] internal Vector2 lookInput;

    [SerializeField] CinemachineCamera currentCamera;


    public readonly SyncVar<bool> isAlive = new(true, new(WritePermission.ServerOnly));
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (GameModeController.instance)
        {
            if (IsServerInitialized)
            {
                GameModeController.instance.JoinTeam(Owner, this);
            }
        }
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        if (IsOwner)
        {
            for (int i = 0; i < disableOnLocal.Length; i++)
            {
                disableOnLocal[i].enabled = false;
            }

            spec.cam.gameObject.layer = LayerMask.NameToLayer("LocalPlayer");
            phys.cam.gameObject.layer = LayerMask.NameToLayer("LocalPlayer");

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            for (int i = 0; i < disableOnRemote.Length; i++)
            {
                disableOnRemote[i].enabled = false;
            }
            spec.cam.Priority.Value = -99;
            phys.cam.Priority.Value = -99;
        }
        teamNumber.OnChange += TeamNumber_OnChange;
    }

    private void TeamNumber_OnChange(int prev, int next, bool asServer)
    {
        if (spec)
            spec.SetColour();
        if (phys)
            phys.SetColour();
    }

    private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
    {
        if (GameModeController.instance)
        {
            if (IsServerInitialized)
            {
                GameModeController.instance.JoinTeam(Owner, this);
            }
        }
        else
        {
            if (IsOwner)
            {
                print("Cannot find Game Mode Controller??");
            }
            else
            {
                print("This isn't the local client, i think");
            }
        }
    }
    [ObserversRpc()]
    public void InitialisePlayer()
    {
        if (IsOwner)
        {
            print("Getting spawn point!");

            GameModeController.Spawnpoints spawnpoints = GameModeController.instance.GetSpawnPoint(Owner);
            if(spawnpoints != null)
            {
                Transform spawnpoint = spawnpoints.spawnpoints[Random.Range(0, spawnpoints.spawnpoints.Length)];
                Vector2 randomPoint = Random.insideUnitCircle * spawnpoints.randomSpawnRadius;
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
                Debug.LogWarning("The spawnpoint was null!");
            }
        }
        if (spec)
            spec.SetColour();
        if (phys)
            phys.SetColour();
    }
    public override void OnStopClient()
    {
        base.OnStopClient();

        if (IsOwner)
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
    public void PauseInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GameManager.Instance.SetPause(!GameManager.Instance.paused);
        }
    }

    private void FixedUpdate()
    {
        TeamNumber = teamNumber.Value;

        if (!IsOwner)
        {
            return;
        }
        if (!GameModeController.instance)
        {
            print("GAME MODE CONTROLLER NOT FOUND!");
            return;
        }
        if (GameModeController.instance.gameWaitingToStart.Value && !GameManager.Instance.paused)
        {

            if(currentCamera != spec.cam)
            {
                currentCamera = spec.cam;
                currentCamera.Prioritize();
            }

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
        else if (GameModeController.instance.gameInProgress.Value && !GameManager.Instance.paused)
        {

            if(currentCamera != phys.cam)
            {
                currentCamera = phys.cam;
                currentCamera.Prioritize();
            }

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
