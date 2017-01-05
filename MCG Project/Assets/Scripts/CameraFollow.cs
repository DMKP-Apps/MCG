using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public Vector3 PositionOffset = new Vector3 (0.0f, 0.0f, 0.0f);
	public Vector3 OriginalPositionOffset = new Vector3 (0.0f, 0.0f, 0.0f);
	public GameObject target;
	private System.DateTime startTime = System.DateTime.MaxValue;

	// Use this for initialization
	void Start () {
		//PositionOffset = transform.position;
		//OriginalPositionOffset = transform.localPosition;
		startTime = System.DateTime.MaxValue;
	}
	
	// Update is called once per frame
	void Update () {
		if (target != null) {

			var speed = 5f;

			if (startTime == System.DateTime.MaxValue) {
				startTime = System.DateTime.Now;
			}
			if (System.DateTime.Now.Subtract (startTime).TotalMilliseconds < 250) {
				speed = 5f;
				PositionOffset.y += 0.1f;// * Time.deltaTime;
			}
			if (System.DateTime.Now.Subtract (startTime).TotalMilliseconds < 500) {
				speed = 10f;
				PositionOffset.y += 0.1f;// * Time.deltaTime;
			} else if (System.DateTime.Now.Subtract (startTime).TotalMilliseconds < 750) {
				//PositionOffset.y += 0.05f;// * Time.deltaTime;
				speed = 15f;
			} else if (System.DateTime.Now.Subtract (startTime).TotalMilliseconds < 1000) {
				//PositionOffset.y += 0.2f;
				speed = 20f;
			} else if (System.DateTime.Now.Subtract (startTime).TotalMilliseconds < 2000) {
				//PositionOffset.y++;
				speed = 30f;
			} else {
				//PositionOffset.y += 0.05f;// * Time.deltaTime;
				speed = 15f;
			}

			PositionOffset.y += 0.1f;

			var step = speed * Time.deltaTime;
			var followPosition = target.transform.position + PositionOffset;
			transform.position = Vector3.MoveTowards (transform.position, followPosition, step);

			transform.LookAt(target.transform);



		}
	}

	public void Reset()
	{
		transform.localPosition = OriginalPositionOffset;
		startTime = System.DateTime.MaxValue;

	}
}
