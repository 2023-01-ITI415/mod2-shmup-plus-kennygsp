using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(BoundsCheck))]
public class Enemy : MonoBehaviour
{
    public ScoreCounter scoreCounter;

    [Header("Inscribed: Enemy")]
    public float speed = 10f; // The speed in m/s
    public float fireRate = 0.3f; // Seconds/shot (Unused)
    public float health = 10;
    public float showDamageDuration = 0.1f; // # seconds to show damage
    public float powerUpDropChance = 1f; // Chance to drop a power-up
    public GameObject[] prefabEnemies;


    [Header("Set Dynamically: Enemy")]
    public Color[] originalColors;
    public Material[] materials;// All the Materials of this & its children
    public bool showingDamage = false;
    public float damageDoneTime; // Time to stop showing damage
    public bool notifiedOfDestruction = false; // Will be used later

    protected BoundsCheck bndCheck;
    protected bool calledShipDestroyed = false;

    // This is a property: A method that acts like a field
    public Vector3 pos
    {
        get
        {
            return (this.transform.position);
        }
        set
        {
            this.transform.position = value;
        }
    }

    private void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        // Get materials and colors for this GameObject and its children
        materials = Utils.GetAllMaterials(gameObject);
        originalColors = new Color[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            originalColors[i] = materials[i].color;
        }
    }

    void Start() {
        
        //Find a GameObject named ScoreCounter in the Scene Hierarchy
        GameObject scoreGO = GameObject.Find( "Score" );

        //Get the ScoreCounter script component of scoreGO
        scoreCounter = scoreGO.GetComponent<ScoreCounter>();
    }


    void Update()
    {
        Move();

        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offDown))
        {
            // We're off the bottom, so destroy this GameObject
            Destroy(gameObject);
        }

    }

    public virtual void Move()
    {
        Vector3 tempPos = pos;
        tempPos.y -= speed * Time.deltaTime;
        pos = tempPos;
    }

    private void OnCollisionEnter(Collision coll)
    {
        GameObject otherGO = coll.gameObject;

        ProjectileHero p = otherGO.GetComponent<ProjectileHero>();

        if (p != null) //it's a Projectile Hero
        {

            //If this Enemy is on screen,  damage it.
            if (bndCheck.isOnScreen)
            {
                // Get the damage amount from the Main WEAP_DICT
                health -= Main.GET_WEAPON_DEFINITION(p.type).damageOnHit;
                if (health <= 0)
                {
                    if (!calledShipDestroyed)
                    {
                        calledShipDestroyed = true;
                        Main.SHIP_DESTROYED(this);

                        int prefabIndex = -1;
                        for (int i = 0; i < prefabEnemies.Length; i++)
                        {
                            if (this.gameObject == prefabEnemies[i])
                            {
                                prefabIndex = i;
                                break;
                            }
                        }

                        switch (prefabIndex)
                        {
                            case 0:
                                scoreCounter.score += 50;
                                HighScore.TRY_SET_HIGH_SCORE(scoreCounter.score);
                                break;
                            case 1:
                                scoreCounter.score += 100;
                                HighScore.TRY_SET_HIGH_SCORE(scoreCounter.score);
                                break;
                            case 2:
                                scoreCounter.score += 150;
                                HighScore.TRY_SET_HIGH_SCORE(scoreCounter.score);
                                break;
                            case 3:
                                scoreCounter.score += 200;
                                  HighScore.TRY_SET_HIGH_SCORE(scoreCounter.score);
                                break;
                            case 4:
                                scoreCounter.score += 300;
                                HighScore.TRY_SET_HIGH_SCORE(scoreCounter.score);
                                break;
                        }
                    }
                    Destroy(this.gameObject);
                }
            }
            // Destroy the projectile regardless
            Destroy(otherGO);

        }
        else
        {
            print("Enemy hit by non-ProjectileHero: " + otherGO.name);
        }
    }
    
}
