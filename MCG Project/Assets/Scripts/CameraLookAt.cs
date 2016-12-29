using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLookAt : MonoBehaviour {

	public GameObject target;
	public bool RotateAround = false;
	public float Speed = 2.0f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (target != null) {
			transform.LookAt(target.transform);
			if (RotateAround) {
				transform.RotateAround (target.transform.position, transform.up, Speed * Time.deltaTime);
			
			}
		}
	}
}
