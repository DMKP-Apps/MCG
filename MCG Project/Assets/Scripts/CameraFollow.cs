using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public Vector3 PositionOffset = new Vector3 (0.0f, 0.0f, 0.0f);
	public Vector3 OriginalPositionOffset = new Vector3 (0.0f, 0.0f, 0.0f);
	public GameObject target;
	public GameObject alernateTarget;
	private System.DateTime startTime = System.DateTime.MaxValue;

	private bool followSource = true;

	// Use this for initialization
	void Start () {
		//PositionOffset = transform.position;
		//OriginalPositionOffset = transform.localPosition;
		startTime = System.DateTime.MaxValue;
	}

	public float powerRate = 0f;
	
	// Update is called once per frame
	void Update () {
		if (target != null) {
				
			if (followSource || alernateTarget == null) {
				followSourceTarget ();
			} else {
				followAlternateTarget ();
			}
			transform.LookAt(target.transform);



		}
	}

	public void FollowAlternate() {
		followSource = !followSource;
	}

	private void followSourceTarget() {
		var speed = 5f;

		if (startTime == System.DateTime.MaxValue) {
			startTime = System.DateTime.Now;
		}
		if (System.DateTime.Now.Subtract (startTime).TotalMilliseconds < 250) {
			PositionOffset.y += 1f * Time.deltaTime;
			speed = 5f;
		}
		if (System.DateTime.Now.Subtract (startTime).TotalMilliseconds < 500) {
			speed = 15f * powerRate;
			//PositionOffset.y += 2f * Time.deltaTime;
		} else if (System.DateTime.Now.Subtract (startTime).TotalMilliseconds < 750) {
			//PositionOffset.y += 2f * Time.deltaTime;
			speed = 30f * powerRate;
		} else if (System.DateTime.Now.Subtract (startTime).TotalMilliseconds < 1000) {

			speed = 40f * powerRate;
		} else if (System.DateTime.Now.Subtract (startTime).TotalMilliseconds < 2000) {

			speed = 50f * powerRate;
		} else {

			speed = 30f * powerRate;
			//PositionOffset.y += 0.5f * Time.deltaTime;
		}

		var step = speed * Time.deltaTime;
		var followPosition = target.transform.position + PositionOffset;
		transform.position = Vector3.MoveTowards (transform.position, followPosition, step);
	}

	private void followAlternateTarget() {
		var speed = 10f;

		var step = speed * Time.deltaTime;
		var followPosition = alernateTarget.transform.position + PositionOffset;
		transform.position = Vector3.MoveTowards (transform.position, followPosition, step);
	}


	public void Reset()
	{
		followSource = true;
		transform.localPosition = OriginalPositionOffset;
		startTime = System.DateTime.MaxValue;

	}
}
