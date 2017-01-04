using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public Vector3 PositionOffset = new Vector3 (0.0f, 0.0f, 0.0f);
	public GameObject target;
	// Use this for initialization
	void Start () {
		//PositionOffset = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (target != null) {
			transform.position = target.transform.position + PositionOffset;
			transform.LookAt(target.transform);
			//transform.RotateAround (target.transform.position, transform.up, 50);

		}
	}

	public void Reset()
	{
		transform.position = PositionOffset;

	}
}
