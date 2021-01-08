using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance;

    public GameObject explosionParticle;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ShowParticle(Vector2 location, Color color)
    {
        explosionParticle.transform.position = location;
        ParticleSystem.MainModule settings  = explosionParticle.GetComponent<ParticleSystem>().main;
        settings.startColor = new ParticleSystem.MinMaxGradient(color);
       
    }

    private IEnumerator particleCreateDestroy(Vector2 location, Action DestroyParticle)
    {
        Instantiate(explosionParticle, location, Quaternion.identity);
        explosionParticle.GetComponent<ParticleSystem>().Emit(1);
        yield return new WaitForSeconds(1f);
    }

    public void DestroyParticle()
    {
        Destroy(explosionParticle);
    }
}
