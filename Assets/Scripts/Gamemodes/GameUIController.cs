using TMPro;
using UnityEngine;

public class GameUIController : MonoBehaviour
{
    public static GameUIController Instance;

    public TMP_Text timerText;
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
    }
}
