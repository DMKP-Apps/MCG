using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopographicBulletController : MonoBehaviour {

    public GameObject target;
    public Vector3 positionOffset;
    public Vector3 rotationOffset;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (target != null)
        {
            transform.position = target.transform.position + positionOffset;
            /*var newRotation = target.transform.rotation.eulerAngles + rotationOffset;
            newRotation.x = 90f;
            newRotation.z = 0f;
            transform.rotation = Quaternion.Euler(newRotation);*/
        }
        else {
            Destroy(this.gameObject);
        }

	}
}
