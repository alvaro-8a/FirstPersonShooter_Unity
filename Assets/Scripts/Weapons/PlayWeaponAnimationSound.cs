using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayWeaponAnimationSound : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private AudioClip footstepsSound;
    [SerializeField] private AudioClip equip01Sound;
    [SerializeField] private AudioClip equip02Sound;
    [SerializeField] private AudioClip equip03Sound;


    public void PlaySteps()
    {
        playerController.PlaySound(footstepsSound, 1f);
    }

    public void PlayWeaponEquip01()
    {
        weaponController.PlaySound(equip01Sound, 0.4f);
    }                                           
                                                
    public void PlayWeaponEquip02()             
    {                                           
        weaponController.PlaySound(equip02Sound, 0.45f);
    }                                              
                                                   
    public void PlayWeaponEquip03()                
    {                                              
        weaponController.PlaySound(equip03Sound, 0.3f);
    }
}
