using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothLookAt : MonoBehaviour {

	//values that will be set in the Inspector
	public Transform Target;
	public float RotationSpeed;

	//values for internal use
	private Quaternion _lookRotation;
	private Vector3 _direction;

	// Update is called once per frame
	void Update()
	{
		if (Target == null) {
			return;
		}

		//find the vector pointing from our position to the target
		_direction = (Target.position - transform.position).normalized;

		//create the rotation we need to be in to look at the target
		_lookRotation = Quaternion.LookRotation(_direction);

		/*var pos1 = levelVector(transform.rotation.eulerAngles);
		var pos2 = levelVector(_lookRotation.eulerAngles);


		var distance = Vector3.Distance (pos1, pos2);
		if (distance > RotationSpeed) {
			distance = RotationSpeed;
		}
		else if (distance < 2.5f) {
			distance = 2.5f;
		}
		//RotationSpeed = distance;*/

		//rotate us over time according to speed until we are in the required rotation
		transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * RotationSpeed);
	}

	private Vector3 levelVector(Vector3 point)
	{
		if (point.y > 180)
		{
			point.y -= 360;
		}
		if (point.x > 180)
		{
			point.x -= 360;
		}
		if (point.z > 180)
		{
			point.z -= 360;
		}
		if (point.y < -180)
		{
			point.y += 360;
		}
		if (point.x < -180)
		{
			point.x += 360;
		}
		if (point.z < -180)
		{
			point.z += 360;
		}

		return point;
	}

}
