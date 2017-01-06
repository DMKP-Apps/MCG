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
	public GameObject CameraShot1;
	public GameObject CameraPosition;

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
		GameController.RegisterCameraController (this);
	}

	public bool RunActionCamera = true;
	public float followSpeed = 1f;

	void Update() {

		Rotation = Cannon.transform.rotation.eulerAngles;
		Position = Cannon.transform.position;

		var bullet = fireController.GetBullet ();
		if (bullet == null && Mode == CameraMode.FollowBullet) {
			Mode = CameraMode.FollowCannon;
			//transform.localPosition = PositionOffset;
			CameraShot1.SetActive (false);
			var followCamera = CameraShot1.GetComponent<CameraFollow> ();
			followCamera.alernateTarget = Cannon;
			followCamera.Reset ();
		}
		else if (bullet != null && Mode == CameraMode.FollowCannon) {
			var followCamera = CameraShot1.GetComponent<CameraFollow> ();
			followCamera.target = bullet;
			followCamera.powerRate = GameController.powerRate;
			//followCamera.PositionOffset = CameraShot1.transform.localPosition - bullet.transform.localPosition;
			followCamera.PositionOffset = CameraShot1.transform.position - bullet.transform.position;

			CameraShot1.SetActive (true);
		}

	}

	void LateUpdate () 
	{
		var bullet = fireController.GetBullet ();


		if (bullet == null) {
			//FollowCannon ();
			var step = 2.5f * Time.deltaTime;
			var followPosition = CameraPosition.transform.position;
			followPosition.y = transform.position.y;
			transform.position = Vector3.MoveTowards (transform.position, followPosition, step);

			transform.LookAt (LookAtTarget.transform);
			//PositionOffset = transform.localPosition;

		} else {
			Mode = CameraMode.FollowBullet;
			if (bullet.activeInHierarchy) {
				//transform.localPosition = (bullet.transform.localPosition + PositionOffset) * followSpeed;
				//transform.LookAt (bullet.transform);
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
