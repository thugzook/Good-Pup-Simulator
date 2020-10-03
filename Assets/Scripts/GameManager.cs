﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject attackBird;
    public int attackBirdLimit = 2;
    public static List<GameObject> attackBirdList = new List<GameObject>();

    private static Vector3 dogPosition;
    private static int level = -1;
    private static int health = 3;
    private static int spawnRate = 4;

    System.Random rand = new System.Random();

    // Defines what the player lost to
    public enum lossState
    {
        OWNER,
        BIRD
    }

    void LevelLose(lossState loss)
    {
        // Pause all functionality
        Time.timeScale = 0;
        GameObject.Find("Hand").GetComponent<HandShake>().enabled = false;
        GameObject.Find("Click Manager").GetComponent<ClickManager>().enabled = false;
        switch (loss)
        {
            case lossState.OWNER:
                Debug.Log("Owner Loss");
                break;
            case lossState.BIRD:
                Debug.Log("Bird Loss");
                break;
        }
    }

    public void LevelWin()
    {
        // increment level and call level manager
        level++;
        levelManager();

        // Reload scene
        GameObject.Find("LevelText").GetComponent<UnityEngine.UI.Text>().text = "Level " + (level + 1);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void loseHealth(int amt, lossState loss)
    {
        health -= amt;
        if (health <= 0)
        {
            GameObject.Find("Health").GetComponent<UnityEngine.UI.Text>().text = "0";
            LevelLose(loss);
        }
        else
        {
            //GameObject.Find("Click Manager").GetComponent<ClickManager>().handRetract.Invoke();
            GameObject.Find("Health").GetComponent<UnityEngine.UI.Text>().text = health.ToString();
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        // initialize game elements
        ProgressBar.onProgressComplete.AddListener(LevelWin);
        OwnerLook.OwnerCaughtYou.AddListener(delegate { loseHealth(3, lossState.OWNER); }) ; // create a delegate for health loss when owner looks at you
        dogPosition = GameObject.Find("Dog").GetComponent<Transform>().position;
        // Manage levels
        levelManager();
        GameObject.Find("LevelText").GetComponent<UnityEngine.UI.Text>().text = "Level " + (level + 1);
    }

    void levelManager()
    {
        switch (level)
        {
            // Level 0: Main Menu
            case -1:
                // hide all game objects
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                // disable click manager
                GameObject.Find("Click Manager").GetComponent<ClickManager>().enabled = false;
                foreach (GameObject go in allObjects)
                {
                    if (go.GetComponent<Renderer>() && !(go.name == "Background"))
                    {
                        float alpha = go.GetComponent<Renderer>().material.color.a;
                        Color newColor = new Color(1, 1, 1, 0);
                        go.GetComponent<Renderer>().material.color = newColor;
                    }
                }
                break;
            // Level 1: No Grandpa
            case 0:
                // disable unneeded features
                OwnerLook.enabled = false;
                GameObject.Find("Health").SetActive(false);
                BirdMove.enabled = false;
                break;
            // Level 2: Grandpa looking
            case 1:
                OwnerLook.enabled = true;
                break;
            // Level 3: Decreasing pet level
            case 2:
                OwnerLook.enabled = true;
                StartCoroutine("DecreaseProgress");
                break;
            // Level 4: Kid comes to pet the dog
            case 3:
                // SKIP THIS LEVEL
                level++;
                levelManager();
                break;
            // Level 5: Birds attack (hurts dog)
            case 4:
                OwnerLook.enabled = true;
                BirdMove.enabled = true;
                StartCoroutine("DecreaseProgress");
                StartCoroutine("SpawnAttackBirds");
                break;
            // Level 6: More birds attack
            case 5:
                OwnerLook.enabled = true;
                BirdMove.enabled = true;
                attackBirdList.Clear();
                StartCoroutine("DecreaseProgress");
                Debug.Log(attackBirdList.Count);
                StartCoroutine("SpawnAttackBirds");
                break;
            // Level 7: Birds take over grandpa's body
            case 6:
                break;
            // level 8: INSANE bird level
            case 7:
                break;
            // level 9: NUKE. Time limit
            case 8:
                break;
        }
    }

private IEnumerator DecreaseProgress()
    {
        while (true)
        {
            if (GameObject.Find("Slider").GetComponent<ProgressBar>().CurrentValue > 0)
                GameObject.Find("Slider").GetComponent<ProgressBar>().CurrentValue-= 0.003f;
            yield return new WaitForSeconds(0.01f);
        }
    }

private IEnumerator KidPetDecreaseProgress()
    {
        while (true)
        {
            if (GameObject.Find("Slider").GetComponent<ProgressBar>().CurrentValue > 0)
                GameObject.Find("Slider").GetComponent<ProgressBar>().CurrentValue -= 0.004f;
            yield return new WaitForSeconds(0.01f);
        }
    }

// Spawns attack birds
private IEnumerator SpawnAttackBirds()
    {
        float timer = 0f;
        while (true)
        {
            if (attackBirdList.Count < attackBirdLimit)
            {
                timer -= Time.deltaTime;
                if (timer < 0)
                {
                    // spawn an attackbird with velocity towards Dog
                    float x_pos = (float)(rand.NextDouble() * (13f + 1f + 13f) - 13f);
                    float y_pos = 6.5f;


                    Vector3 birdPosition = new Vector3(x_pos, y_pos, 0);
                    Vector3 vectorToDog = dogPosition - birdPosition;
                    float angle = Mathf.Atan2(vectorToDog.y, vectorToDog.x) * Mathf.Rad2Deg;
                    Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);

                    GameObject birdInstance = Instantiate(attackBird, new Vector2(x_pos, y_pos), q);

                    vectorToDog.Normalize();
                    float randSpeed = (float)(rand.NextDouble() * (5f + 1f) - 1f);
                    birdInstance.GetComponent<Rigidbody2D>().velocity = new Vector2(vectorToDog.x * randSpeed, vectorToDog.y * randSpeed);
                    attackBirdList.Add(birdInstance);

                    // flip attack bird if spawned on right side
                    Vector3 lTemp = birdInstance.GetComponent<Transform>().localScale; 
                    lTemp.y *= System.Math.Sign(-x_pos);
                    birdInstance.GetComponent<Transform>().localScale = lTemp;

                    // reset timer
                    timer = (float)(rand.NextDouble() * (spawnRate - 0f) + 0f);
                    Debug.Log("Timer " + timer);
                }
            }
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
