﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleManager : MonoBehaviour {

    public GameObject lightRack;
    public List<bool> receiverCompletion;
    public bool play = false;
    public bool finished = false;
    public bool editor = false;
    public enum PuzzleType { Cube, Wall, Other };
    public PuzzleType type;
    public bool hidden = false;
    public Color uncompleteColor, completeColor;
    public float colorChangeSpeed = 3f;
    public MeshRenderer targetRenderer;
    Vector3 randomRotation = new Vector3(1, 1, 1);
    public UnityEvent OnPuzzleComplete = new UnityEvent();
    public GameObject keyCube;

    private bool allActive = true;
    private Light glow;
    private bool changing = false, color1 = false, color2 = true;
    private bool finishInvoked = false;
    private float colorChangeTimer = 0f; 
    private Transform playTiles;
    private Color targetColor, currentColor;

    private float lerpTimer = 0f;
 

    void Start()
    {
        //glow = transform.FindChild("Glow").GetComponent<Light>();
        //glow.color = uncompleteColor;
        playTiles = transform.FindChild("PlayTiles");
        
        SetAllColor(uncompleteColor);
    }

	// Update is called once per frame
	void Update () {
        if (hidden)
        {
            HideCube();
        }
        else
        {
            ShowPuzzle();
            if (targetRenderer == null)
            {
                targetRenderer = playTiles.GetChild(0).GetComponent<MeshRenderer>();
                SetAllColor(uncompleteColor);
            }
            else
            {
                //Debug.Log(targetRenderer);
                if (finished && targetRenderer.material.color == uncompleteColor && !changing)
                {
                    targetColor = completeColor;
                    colorChangeTimer = 0;
                    changing = true;
                }

                if (!finished && targetRenderer.material.color == completeColor && !changing)
                {
                    targetColor = uncompleteColor;
                    colorChangeTimer = 0;
                    changing = true;
                }

                if (changing)
                {
                    float step = Time.deltaTime / colorChangeSpeed;
                    Color originalColor;
                    if (targetColor == completeColor)
                    {
                        originalColor = uncompleteColor;
                    }
                    else
                    {
                        originalColor = completeColor;
                    }
                    colorChangeTimer += step;
                    SetAllColor(Color.Lerp(originalColor, targetColor, colorChangeTimer));
                    //glow.color = Color.Lerp(originalColor, targetColor, colorChangeTimer);
                    //currentColor = glow.color;
                    if (colorChangeTimer >= 1.0f)
                    {
                        changing = false;
                    }
                }
            }
            if (play)
            {
                finished = CheckAllReceiver();
            }
            else if(!play && !editor)
            {
                
                randomRotation.x = Mathf.Clamp(randomRotation.x + Random.Range(0, 10f), -20, 20f);
                randomRotation.y = Mathf.Clamp(randomRotation.y + Random.Range(0, 10f), -20, 20f);
                randomRotation.z = Mathf.Clamp(randomRotation.z + Random.Range(0, 10f), -20, 20f);
                transform.Rotate(randomRotation * Time.deltaTime);
            }
        }

        if (!finishInvoked && finished)
        {
            OnPuzzleComplete.Invoke();
            finishInvoked = true;
        }
        /*
        if (finished)
        {
            if(lerpTimer <= 1.5f)
            {
                lerpTimer += Time.deltaTime * 2f;
				this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(this.transform.position.x, -1, this.transform.position.z), lerpTimer);
            }
        }*/

	}

    public IEnumerator HidePuzzle()
    {
        float timer = 0f;
        GetComponent<AudioSource>().Play();
        while (!hidden)
        {
            timer += Time.deltaTime;
            //Debug.Log(timer);
            if(timer >= 3f)
            {
                hidden = true;
            }
            else
            {
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
       
    }

    //Hides the cube so that performance can be increased.
    public void HideCube()
    {
        if (allActive)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).transform.gameObject.SetActive(false);
            }
        }
        GetComponent<Collider>().enabled = false;
        allActive = false;
        hidden = true;
      
    }

    //Show the cube
    public void ShowPuzzle()
    {
       
        if (!allActive)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).transform.gameObject.SetActive(true);
            }
        }
        if (type == PuzzleType.Cube)
        {
            GetComponent<Collider>().enabled = true;
        }    
        allActive = true;
        hidden = false;

    
    }

    //Sets the colors properly
    public void SetAllColor(Color targetColor)
    {
        for (int i = 0; i < playTiles.childCount; i++)
        {
            if (playTiles.GetChild(i).GetComponent<MeshRenderer>() != null)
            {
                targetRenderer = playTiles.GetChild(i).GetComponent<MeshRenderer>();
                targetRenderer.material.color = targetColor;
            }
        }
    }

    //Colors: Red, Blue, Yellow, Pink,  Magenta, Green, Grey, Cyan, Brown, Purple, Orange
    public Color GetLaserPigment(EmitterScript.LaserColor laserColor)
    {
        Color targetColor;
        switch ((int)laserColor)
        {
            //Red
            case 0:
                targetColor = Color.red;
                break;
            //Blue
            case 1:
                targetColor = Color.blue;
                break;
            //Yellow
            case 2:
                targetColor = Color.yellow;
                break;
            //Pink
            case 3:
                targetColor = new Color32(255, 105, 180, 255);
                break;
            //Magenta
            case 4:
                targetColor = Color.magenta;
                break;
            //Green
            case 5:
                targetColor = Color.green;
                break;
            //Grey
            case 6:
                targetColor = Color.grey;
                break;
            //Cyan
            case 7:
                targetColor = Color.cyan;
                break;
            //Brown
            case 8:
                targetColor = new Color32(97, 65, 38, 255);
                break;
            //Purple
            case 9:
                targetColor = new Color32(76, 23, 125, 255);
                break;
            //Orange
            case 10:
                targetColor = new Color32(255, 165, 0, 255);
                break;
            default:
                targetColor = Color.white;
                Debug.Log("Invalid color attempted.");
                break;
        }
        return targetColor;
    }

    bool CheckAllReceiver()
    {
        bool completion = true;
        for(int i = 0; i < receiverCompletion.Count; i++)
        {
            if(receiverCompletion[i] == false)
            {
                completion = false;
            }

        }
        return completion;
    }

    public void EmptyList()
    {
        receiverCompletion = new List<bool>();
    }

    public void TurnOn(int index)
    {
        lightRack.GetComponent<AllHexLightsController>().TurnOn(index);
    }

    public void TurnOff(int index)
    {
        lightRack.GetComponent<AllHexLightsController>().TurnOff(index);
    }

    //Coroutine that Shrinks all the elements on the face of the cube till they disappear
    public IEnumerator PushInAll()
    {
        //The play tiles.
        Transform playTiles = transform.FindChild("PlayTiles");

        //The edge connections.
        Transform edgeConnections = transform.FindChild("EdgeConnections");

        float timer = 0f;

        while(timer < 5f)
        {
            //Push all the playTiles in.
            playTiles.localScale = Vector3.Max(playTiles.localScale - new Vector3(0.01f, 0.01f, 0.01f), new Vector3(0.01f, 0.01f, 0.01f));
            for(int i = 0; i < playTiles.childCount; i++)
            {
                Transform tile = playTiles.GetChild(i).transform;
                tile.transform.position += -tile.transform.up * 0.015f * Time.deltaTime;
                /*
                tile.localPosition = new Vector3(
                    Mathf.Lerp(originalPositions[i].x, originalPositions[i].x - 2f, timer / 10f),
                    tile.localPosition.y,
                    tile.localPosition.z);  */
            }

            //Push all the edgeConnections in.
            for(int i = 0; i < edgeConnections.childCount; i++)
            {
                Transform edge = edgeConnections.GetChild(i).transform;
                edge.transform.position += -edge.transform.up * 0.1f * Time.deltaTime;
            }

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        //Destroy(edgeConnections.gameObject);
        //Destroy(playTiles.gameObject);
    }

    //Coroutine that Replaces the transformed cube with a new KeyCube
    public void ReplaceCube()
    {
        //Offset was -0.065 last check. Only the Y.
        GameObject key = Instantiate(keyCube, transform.position, Quaternion.identity);

    }

    //Transform the cube.
    public void TransformPuzzle()
    {
        play = false;
        StartCoroutine(PushInAll());
    }
}
