using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LaserCollision : MonoBehaviour {
    LineRenderer lineRenderer;
    Vector3 offset;
    public float laserWidth = 1.0f;
    int length;
    Vector3[] position;

    public float noise = 1.0f;
    public float maxLength = 50.0f;

    public GameObject explosion;

    public bool Enabled = true;

    public List<double> toggleEnabledMillisecs = new List<double>();
    private DateTime _time = DateTime.Now;
    private int toggleIndex = 0;
    public double initialWaitMillisecs = 0;

    private bool hasHit = false;
    private bool reset = false;

    // Use this for initialization
    void Start () {
        lineRenderer = GetComponent<LineRenderer>();
        offset = new Vector3(0, 0, 0);
        _time = DateTime.Now.AddMilliseconds(initialWaitMillisecs);
    }
	
	// Update is called once per frame
	void Update () {

        if (toggleEnabledMillisecs.Count > 0)
        {
            if (DateTime.Now.Subtract(_time).TotalMilliseconds > toggleEnabledMillisecs[toggleIndex])
            {
                toggleIndex++;
                if (toggleIndex >= toggleEnabledMillisecs.Count)
                {
                    toggleIndex = 0;
                }
                _time = DateTime.Now;
                Enabled = hasHit || reset ? Enabled : !Enabled;
                if (hasHit)
                {
                    reset = true;
                }
                else if (reset)
                {
                    reset = false;
                }
                hasHit = false;
            }
        }

        RenderLaser();

    }

    void RenderLaser()
    {

        //Shoot our laserbeam forwards!
        UpdateLength();

        //lineRenderer.SetColors(color, color);
        //Move through the Array
        for (int i = 0; i < length; i++)
        {
            //Set the position here to the current location and project it in the forward direction of the object it is attached to
            offset.z = transform.position.z + i * transform.forward.z + UnityEngine.Random.Range(-noise, noise);
            offset.x = i * transform.forward.x + UnityEngine.Random.Range(-noise, noise) + transform.position.x;
            offset.y = transform.position.y;
            position[i] = offset;

            lineRenderer.SetPosition(i, position[i]);

        }
        
    }

    void UpdateLength()
    {
        if (!Enabled)
        {
            length = 0;
            position = new Vector3[length];
            lineRenderer.numPositions = length;
            return;
        }

        //Raycast from the location of the cube forwards
        RaycastHit[] hit;
        hit = Physics.RaycastAll(transform.position, transform.forward, maxLength);
        int i = 0;
        while (i < hit.Length)
        {
            if (!hit[i].collider.isTrigger)
            {
                length = (int)Mathf.Round(hit[i].distance) + 2;
                position = new Vector3[length];
                
                lineRenderer.numPositions = length;

                Debug.Log(hit[i].collider.gameObject.name);

                if (hit[i].collider.gameObject.tag == "Bullet")
                {
                    hasHit = true;
                    // get the bullet hit controller if available
                    var hitController = hit[i].collider.gameObject.GetComponent<BulletHitController>();
                    if (hitController != null)
                    {
                        hitController.HitHazard("Laser", this.gameObject, hit[i].point, explosion);
                    }
                }

                return;
            }
            i++;
        }
        
        length = (int)maxLength;
        position = new Vector3[length];
        lineRenderer.numPositions = length;


    }
}
