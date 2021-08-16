﻿using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

public class Player : NetworkBehaviour
{
    public float baseSpeed;
    public bool hasPowerupEffect;
    public int powerupType;
    public float powerupTimer;
    [SyncVar]
    public bool isAlive;
    [SyncVar]
    public bool isInvis;
    public Vector3 glitchPosition;
    public int glitchMarksToPlace;
    public GameObject glitchMarkPrefab;
    public GameObject glitchModule;
    [SyncVar]
    public bool hasShield;
    public Material material;
    public ParticleSystem ps;
    public ParticleSystemRenderer psr;
    public int pIndex;
    //Private 
    public float speed;
    

    public Vector3 direction;

    void Start()
    {
        direction = new Vector3(0f,0f,0f);
        speed = baseSpeed;
        hasPowerupEffect = false;
        gameObject.GetComponentInChildren<Renderer>().material = material;
        ps = gameObject.transform.GetChild(1).gameObject.GetComponent<ParticleSystem>();
        psr = gameObject.transform.GetChild(1).gameObject.GetComponentInChildren<ParticleSystemRenderer>();
        psr.material = material;
        psr.trailMaterial = material;

        isAlive = true;
    }

    void Update()
    {
        if (hasPowerupEffect)
        {
            if (powerupType == 4)
            {
                if (powerupTimer <= 2.5 && glitchMarksToPlace >= 5)
                {
                    Instantiate(glitchMarkPrefab, transform.position, Quaternion.identity, glitchModule.transform);
                    glitchMarksToPlace--;
                }
                if (powerupTimer <= 2 && glitchMarksToPlace >= 4)
                {
                    Instantiate(glitchMarkPrefab, transform.position, Quaternion.identity, glitchModule.transform);
                    glitchMarksToPlace--;
                }
                if (powerupTimer <= 1.5 && glitchMarksToPlace >= 3)
                {
                    Instantiate(glitchMarkPrefab, transform.position, Quaternion.identity, glitchModule.transform);
                    glitchMarksToPlace--;
                }
                if (powerupTimer <= 1 && glitchMarksToPlace >= 2)
                {
                    Instantiate(glitchMarkPrefab, transform.position, Quaternion.identity, glitchModule.transform);
                    glitchMarksToPlace--;
                }
                if (powerupTimer <= 0.5 && glitchMarksToPlace >= 1)
                {
                    Instantiate(glitchMarkPrefab, transform.position, Quaternion.identity, glitchModule.transform);
                    glitchMarksToPlace--;
                }


            }
            if (powerupTimer <= 0)
            {
                //Remove powerup effect
                if (powerupType == 1)
                {
                    speed = baseSpeed;
                    transform.GetChild(4).gameObject.SetActive(false);
                    transform.GetChild(2).gameObject.SetActive(true);
                } else if (powerupType == 3)
                {
                    isInvis = false;
                    gameObject.GetComponentInChildren<Renderer>().material = material;
                } else if (powerupType == 4)
                {
                    transform.position = glitchPosition;
                    Destroy(glitchModule);
                } else if (powerupType == 5)
                {
                    hasShield = false;
                    transform.GetChild(3).gameObject.SetActive(false);
                }
                hasPowerupEffect = false;
            } else
            {
                powerupTimer -= Time.deltaTime;
            }
        }
    }

    void FixedUpdate()
    {
        if (direction.magnitude >= 0.1f && isAlive)
        {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }
    }

    [ClientRpc]
    public void Die()
    {
        ps.Play();
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        isAlive = false;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Powerup")
        {
            if (hasPowerupEffect == false)
            {
                collider.GetComponent<Powerup>().ActivatePowerup(gameObject);
            }
        }
    }
}