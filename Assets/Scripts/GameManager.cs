using Eflatun.SceneReference;
using FishNet.Managing;
using FishNet.Managing.Scened;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] internal CanvasGroup loadscreenGroup;
    [SerializeField] internal bool loadingScene;
    [SerializeField] internal AnimationCurve loadScreenAlphaCurve;
    [SerializeField] internal float fakeLoadScreenTime;
    [SerializeField] GameModeController gmc;
    [SerializeField] GameObject gmcPrefab;

    [SerializeField] internal SceneReference menuScene;
    [SerializeField] internal TMP_Text joinCodeText;
    [SerializeField] internal GameObject joinCodePanel;


    [SerializeField] internal bool paused;
    [SerializeField] internal GameObject pauseMenu;

    [SerializeField] internal LayerMask attackLayer;

    private void Awake()
    {
        if(!Instance)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }


        DontDestroyOnLoad(gameObject);
        joinCodePanel.SetActive(false);
        pauseMenu.SetActive(false);

    }
    private void Start()
    {
        ConnectionManager.Instance.networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
    }
    private void ClientManager_OnClientConnectionState(FishNet.Transporting.ClientConnectionStateArgs obj)
    {
        if(obj.ConnectionState == FishNet.Transporting.LocalConnectionState.Stopping)
        {
            LoadScene(menuScene);
            joinCodePanel.SetActive(false);

            SetPause(false);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;            
        }
        if(obj.ConnectionState == FishNet.Transporting.LocalConnectionState.Starting)
        {
            joinCodePanel.SetActive(true);
            joinCodeText.text = ConnectionManager.Instance.currentLobbyJoinCode;

            
        }
    }
    public void SetPause(bool paused)
    {
        this.paused = paused;

        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;

        pauseMenu.SetActive(paused);
    }

    public void LoadScene(SceneReference scene)
    {
        if(loadscreenGroup && !loadingScene)
            StartCoroutine(LoadSceneAsync(scene, false));
    }
    public void LoadSceneOnNetwork(SceneReference scene)
    {
        StartCoroutine(LoadSceneAsync(scene, true));
    }
    IEnumerator LoadSceneAsync(SceneReference scene, bool useNetworkLoad)
    {
        loadscreenGroup.gameObject.SetActive(true);
        loadingScene = true;
        loadscreenGroup.blocksRaycasts = true;
        loadscreenGroup.alpha = 0;
        yield return null;
        float a = 0;
        while (a < 1)
        {
            a += Time.deltaTime;
            loadscreenGroup.alpha = loadScreenAlphaCurve.Evaluate(a);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(fakeLoadScreenTime);

        if (useNetworkLoad)
        {
            SceneLoadData sld = new(scene.Name)
            {
                ReplaceScenes = ReplaceOption.All,
                
            };
            ConnectionManager.Instance.networkManager.SceneManager.LoadGlobalScenes(sld);
        }
        else
        {
            var c = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene.BuildIndex);
            while (!c.isDone)
            {
                yield return null;
            }
            c.allowSceneActivation = true;
        }
        yield return new WaitForSeconds(fakeLoadScreenTime);

        while (a > 0)
        {
            a -= Time.deltaTime;
            loadscreenGroup.alpha = loadScreenAlphaCurve.Evaluate(a);
            yield return new WaitForEndOfFrame();
        }
        loadscreenGroup.interactable = false;
        loadscreenGroup.blocksRaycasts = false;
        loadscreenGroup.alpha = 0;
        loadscreenGroup.gameObject.SetActive(false);
        loadingScene = false;
        yield break;
    }
    public void QuitMPCallback()
    {
        ConnectionManager.Instance.QuitMPGame();
    }
}
