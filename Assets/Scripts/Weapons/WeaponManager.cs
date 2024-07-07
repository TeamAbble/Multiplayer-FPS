using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using System.Collections.Generic;
public class WeaponManager : NetworkBehaviour
{

    public bool primaryInput;
    public bool secondaryInput;
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
        CurrentWeapon.fireInput = primaryInput;
        CurrentWeapon.secondaryInput = secondaryInput;
    }
    public void SetFireInput(bool input)
    {
        
        primaryInput = input;
    }

    public void SetSecondaryInput(bool input)
    {
        secondaryInput= input;
    }

}
