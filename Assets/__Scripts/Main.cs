using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour {

    public ScoreCounter scoreCounter;

    static public Main S; // A singleton for Main
    static Dictionary<eWeaponType, WeaponDefinition> WEAP_DICT;

    [Header("Inscribed")]
    public bool spawnEnemies = true;
    public GameObject[] prefabEnemies; // Array of Enemy prefabs
    public float enemySpawnPerSecond = 0.5f; // # Enemies/second
    public float enemyInsetDefault = 1.5f; // Padding for position
    public float gameRestartDelay = 2;
    public float WaveRound = 0;

    //Pause Menu
    public static bool gameIsPaused;
    public GameObject PauseMenu;
    private bool waveTextActive = false;

    int num = 0;

    //Waves
    public GameObject[] WavesText;
    float timer = 0.0f;

    // Enemy GameObjects
    public GameObject enemy1;
    public GameObject enemy2;
    public GameObject enemy3;
    public GameObject enemy4;


    public WeaponDefinition[] weaponDefinitions;
    public GameObject prefabPowerUp;
    public eWeaponType[] powerUpFrequency = new eWeaponType[]
    {
        eWeaponType.blaster, eWeaponType.blaster, eWeaponType.spread, eWeaponType.shield
    };

    private BoundsCheck bndCheck;


    /// <summary>
    /// Called by an Enemy ship whenever it is destroyed. It sometimes
    /// creates a powerup in place of the ship destroyed
    /// </summary>
    /// <param name="e">The enemy that is destroyed</param>
    static public void SHIP_DESTROYED( Enemy e)
    {
        // Potentially generate a PowerUp
        if (Random.value <= e.powerUpDropChance)
        {
            // Choose which PowerUp to pick
            // Pick one from the possibilities in powerUpFrequency
            int ndx = Random.Range(0, S.powerUpFrequency.Length);
            eWeaponType puType = S.powerUpFrequency[ndx];
            // Spawn a PowerUp
            GameObject go = Instantiate(S.prefabPowerUp) as GameObject;
            PowerUp pu = go.GetComponent<PowerUp>();
            // Set it to the proper WeaponType
            pu.SetType(puType);

            // Set it to the position of the destroyed ship
            pu.transform.position = e.transform.position;
        }
    }

    void Start()
    {

        PauseMenu.SetActive(false);
        Time.timeScale = 1;

        GameObject scoreGO = GameObject.Find( "Score" );
        scoreCounter = scoreGO.GetComponent<ScoreCounter>();

        GameObject enemy1 = GameObject.Find("Enemy_1");
        GameObject enemy2 = GameObject.Find("Enemy_2");
        GameObject enemy3 = GameObject.Find("Enemy_3");
        GameObject enemy4 = GameObject.Find("Enemy_4");
        
    }

    void Update()
    {
        timer += Time.deltaTime;
        Waves();

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(gameIsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }      
    }

    public void Waves()
    {

        if ( waveTextActive && timer >= 2f)
        {
            WavesText[num].SetActive(false);
            waveTextActive = false;
            num += 1;
        }

        if ( scoreCounter.score >= 1000 && !waveTextActive && num <= 0)
        {
            waveTextActive = true;
            WavesText[num].SetActive(true);
            timer = 0.0f;
            enemy1.SetActive(true);
            
        }

        else if ( scoreCounter.score >= 2000 && !waveTextActive && num <= 1 )
        {
            waveTextActive = true;
            WavesText[num].SetActive(true);
            timer = 0.0f;
            enemy2.SetActive(true);
            
        }

        else if ( scoreCounter.score >= 3000 && !waveTextActive && num <= 2 )
        {
            waveTextActive = true;
            WavesText[num].SetActive(true);
            timer = 0.0f;
            enemy3.SetActive(true);
            
        }

        else if ( scoreCounter.score >= 4000 && !waveTextActive && num <= 3 )
        {
            waveTextActive = true;
            WavesText[num].SetActive(true);
            timer = 0.0f;
            enemy4.SetActive(true);
            
        }

    }

    private void Awake()
    {
        enemy1.SetActive(false);
        enemy2.SetActive(false);
        enemy3.SetActive(false);
        enemy4.SetActive(false);

        S = this;
        // Set bndCheck to reference the BoundsCheck component on this GameObject
        bndCheck = GetComponent<BoundsCheck>();

        // Invoke SpawnEnemy() once (in 2 seconds, based on default values)
        Invoke(nameof(SpawnEnemy), 1f / enemySpawnPerSecond);

        // A generic Dictionary with WeaponType as the key
        WEAP_DICT = new Dictionary<eWeaponType, WeaponDefinition>();
        foreach(WeaponDefinition def in weaponDefinitions)
        {
            WEAP_DICT[def.type] = def;
        }
    }

    public void SpawnEnemy()
    {
        if(!spawnEnemies)
        {
            Invoke(nameof(SpawnEnemy), 1f / enemySpawnPerSecond);
            return;
        }


        // Pick a random Enemy prefab to instantiate
        int ndx = Random.Range(0, prefabEnemies.Length);
        GameObject go = Instantiate<GameObject>(prefabEnemies[ndx]);

        // Position the Enemy above the screen with a random x position
        float enemyInset = enemyInsetDefault;
        if (go.GetComponent<BoundsCheck>() != null)
        {
            enemyInset = Mathf.Abs(go.GetComponent<BoundsCheck>().radius);
        }

        // Set the initial position for the spawned Enemy
        Vector3 pos = Vector3.zero;
        float xMin = -bndCheck.camWidth + enemyInset;
        float xMax = bndCheck.camWidth - enemyInset;
        pos.x = Random.Range(xMin, xMax);
        pos.y = bndCheck.camHeight + enemyInset;
        go.transform.position = pos;

        // Invoke SpawnEnemy() again
        Invoke(nameof(SpawnEnemy), 1f / enemySpawnPerSecond);

    }

    public void DelayedRestart()
    {
        // Invoke the Restart() method in delay seconds
        Invoke(nameof(Restart), gameRestartDelay);
    }

    public void Restart()
    {
        // Reload _Scene_0 to restart the game
        SceneManager.LoadScene("__Scene_0");
    }

    static public void HERO_DIED()
    {
        S.DelayedRestart();
    }

    public void PauseGame()
    {
        PauseMenu.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }

    public void ResumeGame()
    {
        PauseMenu.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    ///<summary>
    ///Static function that gets a WeaponDefinition from the WEAP_DICT static
    ///protected field of the main class.
    /// </summary>
    /// <returns>The WeaponDefinition or, if there is no WeaponDefinition with
    /// the WeaponType passed in, returns a new WeaponDefinition with a
    /// WeaponType of none..</returns>
    /// <param name="wt">The WeaponType of the desired WeaponDefinition</param>
    static public WeaponDefinition GET_WEAPON_DEFINITION(eWeaponType wt)
    {
        // Check to make sure that the key exists in the Dictionary
        // Attempting to retrieve a key that didn't exist would throw an error,
        // so the following if statement is important.
        if (WEAP_DICT.ContainsKey(wt))
        {
            return (WEAP_DICT[wt]);
        }
        // This returns a new WeaponDefinition with a type of WeaponType.none,
        // which means it has failed to find the right WeaponDefinition
        return new WeaponDefinition();
    }
}
