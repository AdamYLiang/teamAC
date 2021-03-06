﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeFeedback : MonoBehaviour {

    public float detectRange = 5, maxSize;

    public bool print = false;
    public bool react = true;

    //The object that we will measure distance to.
    public GameObject sensorObject;
    public GameObject controller1, controller2;
    public float growTime = 1f;

    protected GameObject gameManager;

    public Color closeColor;
    protected Color originalColor;

    protected bool onColor = false;
    protected Vector3 originalScale;

    protected float colorStep = 0f;
    protected Material ourMat;

	// Use this for initialization
	void Start ()
    {
        gameManager = GameObject.Find("GameManager");

        if(gameManager.GetComponent<GameManager>().hand1 != null)
        {
            controller1 = gameManager.GetComponent<GameManager>().hand1.gameObject;
        }
        if (gameManager.GetComponent<GameManager>().hand2 != null)
        {
            controller2 = gameManager.GetComponent<GameManager>().hand2.gameObject;
        }

        detectRange = 0.2f;

        originalScale = transform.GetChild(0).localScale;
        ourMat = transform.GetChild(0).GetComponent<Renderer>().material;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (gameManager.GetComponent<GameManager>().hand1 != null)
        {
            controller1 = gameManager.GetComponent<GameManager>().hand1.gameObject;
        }
        if (gameManager.GetComponent<GameManager>().hand2 != null)
        {
            controller2 = gameManager.GetComponent<GameManager>().hand2.gameObject;
        }
        //Old if statement. Now used to get the sensorObject.
        if (controller1 != null && controller2 != null && react)
        {
            if(controller1 != null)
            {
                if(controller1.transform.GetChild(3).GetComponent<DrawFromController>() != null)
                {
                    sensorObject = controller1;
                }
            }
            if (controller2 != null)
            {
                if (controller2.transform.GetChild(3).GetComponent<DrawFromController>() != null)
                {
                    sensorObject = controller2;
                }
            }
        }
        if(react && sensorObject != null)
        {
            //Calculate the distance from controller to node
            //float distance = (controller1.transform.position - transform.position).magnitude;
            //float distance2 = (controller2.transform.position - transform.position).magnitude;
            float distance = (sensorObject.transform.position - transform.position).magnitude;
            float chosen = distance;

            //Pick the smallest distance to use as our measurement. i.e. we want the closest controller to affect our size
            /*
            float chosen = 0;
            if (distance > distance2)
            {
                chosen = distance2;
            }
            else // Instead of checking for less than, just an else is fine. If it is equal it won't matter which one we pick anyway.
            {
                chosen = distance;
            }*/

            //Default 1 will keep it the normal size. We want hte min to be 1, and the max to be maxSize.
            //We want it to scale bigger the closer it gets to 0. Our max range of default scaling should be detectRange
            float scaling = 1;
            //We only operate if our detectRange is greater than chosen. If we are equal, no math required.
            if (detectRange > chosen)
            {
                //Render it
                transform.GetChild(0).GetComponent<Renderer>().enabled = true;

                //DetectRange - chosen will be greater the smaller chosen is, i.e. closer to the node
                //Thus, with a detect range of 5, and a chosen distance of 3, our scale modifier will be 0.4f.
                //At a range of 5 with chosen distance of 1, our scale modifeier will be 0.8f;
                //Note: Issues because we are dividing decimals, which actually makes it bigger.
                scaling = ((detectRange - chosen) / detectRange) * 6;
                //Set a min and max.
                if(scaling < 0.5f)
                {
                    scaling = 0.5f;
                }
                else if(scaling > 2f)
                {
                    scaling = 2f;
                }
                onColor = true;
            }
            else
            {
                //transform.GetChild(0).GetComponent<Renderer>().enabled = false;
                scaling = 0.5f;
                onColor = false;
            }

            if (onColor)
            {
                colorStep = Mathf.Clamp01(colorStep + Time.deltaTime * 2);
            }
            else
            {
                colorStep = Mathf.Clamp01(colorStep - Time.deltaTime * 2);
            }

            ourMat.color = Color.Lerp(originalColor, closeColor, colorStep);
            transform.GetChild(0).localScale = originalScale * scaling;
        }
        else if(!react)
        {
            TurnOn();
        }
    }

    public void TurnOn()
    {
        ourMat.color = closeColor;
        transform.GetChild(0).localScale = originalScale * 2.5f;
    }
}
