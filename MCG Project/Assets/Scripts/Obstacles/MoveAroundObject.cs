using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAroundObject : MonoBehaviour {

	public float Speed = 5f;
	public bool Enabled = true;
	public bool isClockWise = true;

	public Transform target;

	
	// Update is called once per frame
	void Update () {
        if (target == null) {
            return;
        }
		if (!Enabled) {
			return;
		}
		var step = Speed * Time.deltaTime;
		transform.RotateAround (target.position, Vector3.up, Speed * Time.deltaTime * (isClockWise ? 1 : -1));
	}
}
