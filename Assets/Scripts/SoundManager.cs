using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioClip rotateClip;
    public AudioClip ExplosionClip;

    private void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        
    }

    public void TriggerRotateSound()
    {
        gameObject.GetComponent<AudioSource>().clip = rotateClip;
        gameObject.GetComponent<AudioSource>().Play();
    }

    public void TriggerExplosionSound()
    {
        gameObject.GetComponent<AudioSource>().clip = ExplosionClip;
        gameObject.GetComponent<AudioSource>().Play();
    }
}
