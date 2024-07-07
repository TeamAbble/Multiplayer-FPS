using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponManager : WeaponManager
{
    PlayerManager pm;
    public override void OnStartClient()
    {
        base.OnStartClient();
        pm = GetComponentInParent<PlayerManager>();
    }
    private void FixedUpdate()
    {
        if (pm == null || !IsOwner)
        {
            print("pm null or not owner");
            return;
        }
        CurrentWeapon.fireInput = primaryInput && GameModeController.instance.gameInProgress.Value && !GameManager.Instance.paused;
        CurrentWeapon.secondaryInput = secondaryInput && GameModeController.instance.gameInProgress.Value && !GameManager.Instance.paused;
        

    }
    public void GetFireInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SetFireInput(true);

        }
        if (context.canceled)
        {
            SetFireInput(false);

        }
    }
}
