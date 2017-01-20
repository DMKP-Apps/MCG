using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour {

	public float Speed = 5f;
	public bool Enabled = true;
	public bool isClockWise = true;

	
	// Update is called once per frame
	void Update () {
		if (!Enabled) {
			return;
		}
		var step = Speed * Time.deltaTime;
		transform.Rotate(new Vector3(0, 0,(Time.deltaTime * Speed) * (isClockWise ? 1 : -1)));
	}
}
