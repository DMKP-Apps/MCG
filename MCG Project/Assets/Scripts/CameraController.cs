using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour {

	public Vector3 PositionOffset = new Vector3 (0.0f, 0.0f, 0.0f);
	public Vector3 Position = new Vector3(0.0f,0.0f,0.0f);
	public Vector3 RotationOffset;
	public Vector3 Rotation;
	private float previousXRotation = 0.0f;
	public GameObject Cannon;
	public GameObject LookAtTarget;

	public float MoveSpeed = 1.0f;
	public float DirectionModifier = 1.0f;
	public bool EnableRotation = true;
	//public GameObject Bullet;

	//public GameObject ShotCamera1;
	//public GameObject ShotCamera2;

	public enum CameraMode
	{
		FollowCannon,
		FollowBullet,
	}

	public CameraMode Mode = CameraMode.FollowCannon;

	void Start () 
	{
		//ShotCamera1.SetActive (false);
		//ShotCamera2.SetActive (false);

		PositionOffset = transform.localPosition;
		RotationOffset = transform.localEulerAngles;

		SetMode (CameraMode.FollowCannon);
	}

	public bool RunActionCamera = true;

	void Update() {

		Rotation = Cannon.transform.rotation.eulerAngles;
		Position = Cannon.transform.position;

	}

	void LateUpdate () 
	{
		switch (Mode) {

		case CameraMode.FollowCannon:

			FollowCannon ();
			transform.LookAt (LookAtTarget.transform);
			break;

		case CameraMode.FollowBullet:

			/*if (Bullet != null && Bullet.activeInHierarchy) {
				transform.LookAt (Bullet.transform);
			}

			if (RunActionCamera && System.DateTime.Now.Subtract (startTime).TotalMilliseconds >= 5000 &&
				!ShotCamera1.activeInHierarchy && !ShotCamera2.activeInHierarchy) {

				//var rand = Random.Range (0, 10);
				//if (rand % 2 == 0) {
					//ShotCamera1.SetActive (true);
				//} else {
					ShotCamera2.SetActive (true);
				//}

			}
			else if (!RunActionCamera && (ShotCamera1.activeInHierarchy || !ShotCamera2.activeInHierarchy)) {


				ShotCamera1.SetActive (false);
				ShotCamera2.SetActive (false);


			}*/
			break;

		}

	}

	private System.DateTime startTime = System.DateTime.Now;

	public void SetMode(CameraMode mode)
	{
		Mode = mode;
		switch (Mode) {

		case CameraMode.FollowCannon:
			transform.localPosition = PositionOffset;
			transform.localEulerAngles = RotationOffset;
			//ShotCamera1.SetActive (false);
			//ShotCamera2.SetActive (false);
			//ShotCamera2.GetComponent<CameraFollow> ().Reset ();

			break;

		case CameraMode.FollowBullet:
			startTime = System.DateTime.Now;
			RunActionCamera = true;
			//ShotCamera1.GetComponent<CameraLookAt> ().target = Bullet;
			//ShotCamera2.GetComponent<CameraFollow> ().target = Bullet;
			break;

		} 
	}

	public void FollowCannon()
	{
		double sourceY = System.Math.Floor (Rotation.y);
		double destY = System.Math.Floor (transform.rotation.eulerAngles.y);

		bool keepRotating = CheckKeepRotating(sourceY, destY, MoveSpeed);

		var currentXRotation = float.Parse(System.Math.Floor (Rotation.x).ToString());
		if (currentXRotation > 180) {
			currentXRotation -= 360;
		}
		if (previousXRotation == 0) {
			previousXRotation = currentXRotation;
		}
		else if (currentXRotation != previousXRotation) {
			var move = (currentXRotation - previousXRotation) * Time.deltaTime * 3f * -1;
			previousXRotation = currentXRotation;

		}

		if (keepRotating && EnableRotation) {
			transform.RotateAround (Position, Vector3.up, MoveSpeed * Time.deltaTime * DirectionModifier);
		}

	}

	public bool CheckKeepRotating(double sourceY, double destY, float moveSpeed)
	{
		sourceY = System.Math.Floor (sourceY / MoveSpeed) * moveSpeed;
		destY = System.Math.Floor (destY / MoveSpeed) * moveSpeed;

		if (sourceY > 180) {
			sourceY -= 360;
		}
		if (destY > 180) {
			destY -= 360;
		}

		var moveY = 0;

		if (destY - sourceY < -180) {
			
			DirectionModifier = -1;
		}
		else if (destY - sourceY > 180) {
			DirectionModifier = 1;
		}
		else if (destY - sourceY < 0) {
			DirectionModifier = 1;
		}
		else {
			DirectionModifier = -1;
		}

		return sourceY != destY;
	}


}
