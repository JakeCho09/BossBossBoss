using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    [Header("장착된 무기")]
    public WeaponCollider equippedWeapon;

    public void OpenHitBox()
    {
        if(equippedWeapon != null)
        {
            equippedWeapon.EnabledWeapon();
            Debug.Log("타격 판정 ON!");
        }
    }

    public void CloseHitBox()
    {
        if (equippedWeapon != null)
        {
            equippedWeapon.DisableWeapon();
            Debug.Log("타격 판정 OFF!");
        }
    }
}
