using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFootsteps : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private AudioClip footstepsSound;


    public void PlaySteps()
    {
        playerController.PlaySound(footstepsSound);
    }
}
