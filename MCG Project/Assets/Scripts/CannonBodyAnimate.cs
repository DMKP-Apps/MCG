using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CannonBodyAnimate : MonoBehaviour {

    public int currentState = 0;
    private Vector3 originalScale;
    //private Vector3 scaleToUp = new Vector3(0.11f, 0.1f, 0.06f) * 10;
    //private Vector3 speedUp = new Vector3(0.0001f, 0, 0.0001f) * 10;

    private Vector3 scaleToUp = new Vector3(0.11f, 0.1f, 0.06f);
    private Vector3 speedUp = new Vector3(0.0001f, 0, 0.0001f) * 10;

    public bool animate = false;
    private DateTime startTime = DateTime.MaxValue;

    //private float speed 
	// Use this for initialization
	void Start () {
        originalScale = transform.localScale;

    }

    public void PlayAnimation()
    {
        startTime = DateTime.Now;
        animate = true;
        transform.localScale = originalScale;
        currentState = 0;
    }

    public void StopAnimation()
    {
        startTime = DateTime.MaxValue;
        animate = false;
        transform.localScale = originalScale;
        currentState = 1;
    }

    // Update is called once per frame
    void Update () {
        if (animate)
        {
            if (currentState == 0)
            {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z - (0.15f * Time.deltaTime));
                if (transform.localScale.z <= 0.7f)
                {
                    currentState = 1;
                }
            }
        }
	}
}
