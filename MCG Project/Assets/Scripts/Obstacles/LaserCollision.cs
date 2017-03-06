using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserCollision : MonoBehaviour {
    LineRenderer lineRenderer;
    Vector3 offset;
    public float laserWidth = 1.0f;
    int length;
    Vector3[] position;

    public float noise = 1.0f;
    public float maxLength = 50.0f;
    
    // Use this for initialization
    void Start () {
        lineRenderer = GetComponent<LineRenderer>();
        offset = new Vector3(0, 0, 0);
        //lineRenderer.SetWidth(laserWidth, laserWidth);
    }
	
	// Update is called once per frame
	void Update () {
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
            offset.z = transform.position.z + i * transform.forward.z + Random.Range(-noise, noise);
            offset.x = i * transform.forward.x + Random.Range(-noise, noise) + transform.position.x;
            //offset.z = transform.position.z + i + Random.Range(-noise, noise);
            //offset.x = i  + Random.Range(-noise, noise) + transform.position.x;
            offset.y = transform.position.y;
            position[i] = offset;
            //position[0] = transform.position;

            lineRenderer.SetPosition(i, position[i]);

        }
        
    }

    void UpdateLength()
    {
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

                return;
            }
            i++;
        }
        
        length = (int)maxLength;
        position = new Vector3[length];
        lineRenderer.numPositions = length;


    }
}
