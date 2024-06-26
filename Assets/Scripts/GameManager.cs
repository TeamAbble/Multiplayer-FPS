using Eflatun.SceneReference;
using FishNet.Managing;
using FishNet.Managing.Scened;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] internal CanvasGroup loadscreenGroup;
    [SerializeField] internal bool loadingScene;
    [SerializeField] internal AnimationCurve loadScreenAlphaCurve;
    [SerializeField] internal float fakeLoadScreenTime;
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
        yield return new WaitForSeconds(fakeLoadScreenTime);
        while (a < 1)
        {
            a += Time.deltaTime;
            loadscreenGroup.alpha = loadScreenAlphaCurve.Evaluate(a);
            yield return new WaitForEndOfFrame();
        }
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
            yield return new WaitForSeconds(fakeLoadScreenTime);
        }

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
    }
}
