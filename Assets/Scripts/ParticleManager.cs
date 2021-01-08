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
        Instantiate(explosionParticle, new Vector3(location.x, location.y, -2), Quaternion.identity);
        explosionParticle.GetComponent<ParticleSystem>().Emit(1);
    }

}
