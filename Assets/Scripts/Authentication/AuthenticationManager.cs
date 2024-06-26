using Eflatun.SceneReference;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    public SceneReference menuScene;
    public static AuthenticationManager Instance;
    public bool ForceSignInOnBrowser;
    private async void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }



        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                InitializationOptions o = new();
                await UnityServices.InitializeAsync(o);
            }
        }
        catch (System.Exception e)
        {
            throw e;
        }

        if (ForceSignInOnBrowser)
            AuthenticationService.Instance.ClearSessionToken();
        PlayerAccountService.Instance.SignedIn += Instance_SignedIn1;
        AuthenticationService.Instance.SignedIn += Instance_SignedIn;
        await PlayerSignIn();
    }

    private void Instance_SignedIn1()
    {
        SignInWithUnity();
    }

    async Task PlayerSignIn()
    {
        try
        {
            if (!AuthenticationService.Instance.SessionTokenExists)
            {
                try
                {
                    await PlayerAccountService.Instance.StartSignInAsync();
                }
                catch (System.Exception)
                {

                    throw;
                }
                
            }
            else
            {
                try
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
                catch (System.Exception)
                {

                    throw;
                }
            }

        }
        catch (System.Exception)
        {

            throw;
        }
    }
    async void SignInWithUnity()
    {
        print(PlayerAccountService.Instance.AccessToken);
        try
        {
            await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
        }
        catch (System.Exception)
        {

            throw;
        }
    }

    private void Instance_SignedIn()
    {
        print("Successfully signed in!");
        GameManager.Instance.LoadScene(menuScene);
    }
    private void OnApplicationQuit()
    {
        AuthenticationService.Instance.SignOut();
    }
}
