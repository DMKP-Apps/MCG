using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour {

	public float Speed = 5f;
	public bool Enabled = true;
	public bool isClockWise = true;

    public enum RotateAxis
    {
        x,
        y,
        z
    }

    public RotateAxis rotateOn = RotateAxis.z;

	// Update is called once per frame
	void Update () {
		if (!Enabled) {
			return;
		}
        
		var step = Speed * Time.deltaTime;
        if (rotateOn == RotateAxis.x)
        {
            transform.Rotate(new Vector3((Time.deltaTime * Speed) * (isClockWise ? 1 : -1), 0, 0));
        }
        else if (rotateOn == RotateAxis.y)
        {
            transform.Rotate(new Vector3(0, (Time.deltaTime * Speed) * (isClockWise ? 1 : -1), 0));
        }
        else if (rotateOn == RotateAxis.z)
        {
            transform.Rotate(new Vector3(0, 0, (Time.deltaTime * Speed) * (isClockWise ? 1 : -1)));
        }
    }
}
