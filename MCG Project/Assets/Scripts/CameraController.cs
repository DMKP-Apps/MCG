using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour {

	public Vector3 PositionOffset = new Vector3 (0.0f, 0.0f, 0.0f);
	public Vector3 Position = new Vector3(0.0f,0.0f,0.0f);
	public Vector3 Rotation;
	private float previousXRotation = 0.0f;
	public GameObject Cannon;
	public GameObject LookAtTarget;

	public CannonFireController fireController;
	private GameController GameController;

	public float MoveSpeed = 1.0f;
	public float DirectionModifier = 1.0f;
	public bool EnableRotation = true;

	public enum CameraMode
	{
		FollowCannon,
		FollowBullet,
	}

	public CameraMode Mode = CameraMode.FollowCannon;

	void Start () 
	{
		
		PositionOffset = transform.localPosition;
		Mode = CameraMode.FollowCannon;

		GameController = GameObject.Find ("GameController").GetComponent<GameController> ();
	}

	public bool RunActionCamera = true;
	public float followSpeed = 1f;

	void Update() {

		Rotation = Cannon.transform.rotation.eulerAngles;
		Position = Cannon.transform.position;

		var bullet = fireController.GetBullet ();
		if (bullet == null && Mode == CameraMode.FollowBullet) {
			Mode = CameraMode.FollowCannon;
			transform.localPosition = PositionOffset;
		}

	}

	void LateUpdate () 
	{
		var bullet = fireController.GetBullet ();


		if (bullet == null) {
			FollowCannon ();
			transform.LookAt (LookAtTarget.transform);
			//PositionOffset = transform.localPosition;

		} else {
			Mode = CameraMode.FollowBullet;
			if (bullet.activeInHierarchy) {
				transform.localPosition = (bullet.transform.localPosition + PositionOffset) * followSpeed;
				transform.LookAt (bullet.transform);
			} else {
				//transform.localPosition = (GameController.GetLastHazardContactPoint() + PositionOffset * 1.2f);
				transform.LookAt (GameController.GetLastHazardContactPoint());
			}

		
		}



	}


	public void FollowCannon()
	{
		double sourceY = System.Math.Floor (Rotation.y);
		double destY = System.Math.Floor (transform.rotation.eulerAngles.y);

		bool keepRotating = CheckKeepRotating(sourceY, destY, MoveSpeed);

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
