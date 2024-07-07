using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using System.Collections.Generic;
public class WeaponManager : NetworkBehaviour
{

    public readonly SyncVar<bool> primaryInput = new SyncVar<bool>();
    public readonly SyncVar<bool> secondaryInput = new SyncVar<bool>(new(WritePermission.ClientUnsynchronized));
    public readonly SyncVar<bool> fireBlocked = new SyncVar<bool>();
    public readonly SyncVar<int> weaponIndex = new();
    public readonly SyncList<BaseWeapon> weapons = new();
    public List<BaseWeapon> weaponsList = new();
    public BaseWeapon CurrentWeapon => weaponsList[weaponIndex.Value];

    public Transform fireOrigin;
    public override void OnStartClient()
    {
        base.OnStartClient();
        UpdateWeaponList();
    }
    void UpdateWeaponList()
    {
        weapons.Clear();
        weapons.AddRange(weaponsList);
    }
    private void FixedUpdate()
    {
        CurrentWeapon.fireInput = primaryInput.Value;
        CurrentWeapon.secondaryInput = secondaryInput.Value;
    }
    public void SetFireInput(bool input)
    {
        
        primaryInput.Value = input;
        print($"fire input {primaryInput.Value}");
    }

    public void SetSecondaryInput(bool input)
    {
        secondaryInput.Value = input;
        print($"secondary input {secondaryInput.Value}");
    }

}
