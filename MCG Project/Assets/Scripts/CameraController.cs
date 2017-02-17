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
	private GameObject CameraShot1;
	public GameObject CameraPosition;
	public GameObject Camera2Position;
	public Vector3 explorePosition;

	public CannonFireController fireController;
	private GameController GameController;

	public float MoveSpeed = 1.0f;
	public float DirectionModifier = 1.0f;
	public bool EnableRotation = true;

	public enum CameraMode
	{
		FollowCannon,
		FollowBullet,
		Explore
	}

	public CameraMode Mode = CameraMode.FollowCannon;
	public GameObject cameraHomePostion;
	public bool canInputMovement = false;

	private Vector3 cameraTargetOffset;

	//private SmoothLookAt lookAtController;

	void Start () 
	{
		cameraTargetOffset = LookAtTarget.transform.position - transform.position;
		PositionOffset = transform.localPosition;
		Mode = CameraMode.FollowCannon;

		GameController = GameObject.Find ("GameController").GetComponent<GameController> ();
		GameController.RegisterCameraController (this);

        CameraShot1 = GameController.GetBulletCamera();
		//lookAtController = this.GetComponent<SmoothLookAt> ();
    }

	public bool RunActionCamera = true;
	public float followSpeed = 1f;

	void Update() {

		Rotation = Cannon.transform.rotation.eulerAngles;
		Position = Cannon.transform.position;

		var bullet = fireController.GetBullet ();
		if (bullet == null && CameraShot1 != null && CameraShot1.activeInHierarchy) {//Mode == CameraMode.FollowBullet) {

            Mode = CameraMode.FollowCannon;
			//transform.localPosition = PositionOffset;
			CameraShot1.SetActive (false);
			var followCamera = CameraShot1.GetComponent<CameraFollow> ();
			followCamera.alernateTarget = Cannon;
			followCamera.Reset ();
		}
		else if (bullet != null && Mode == CameraMode.FollowCannon) {
            CameraShot1.transform.position = transform.position;
            CameraShot1.transform.LookAt(bullet.transform);

            var followCamera = CameraShot1.GetComponent<CameraFollow> ();
			followCamera.target = bullet;
			followCamera.powerRate = GameController.powerRate;
			//followCamera.PositionOffset = CameraShot1.transform.localPosition - bullet.transform.localPosition;
			followCamera.PositionOffset = CameraShot1.transform.position - bullet.transform.position;
            followCamera.PositionOffset.y += 10;


            CameraShot1.SetActive (true);
		}

	}

	public float cameraHeightOffset = 4f;

	void LateUpdate () 
	{
		var bullet = fireController.GetBullet ();


		if (bullet == null && (Mode == CameraMode.FollowBullet || Mode == CameraMode.FollowCannon)) {


			var rotation = CameraPosition.transform.rotation.eulerAngles.x;
			if (rotation < 0) {
				rotation *= -1;
			}
			if (rotation > 180) {
				rotation -= 360;
			
			}
			rotation *= -1;



			var offset = rotation / 35;
			if (offset < 0) {
				offset *= -1 * 0.5f;
			}

			Debug.Log(string.Format("Rotaition: {0}", rotation));

			Mode = CameraMode.FollowCannon;

			var localPosition = CameraPosition.transform.localPosition;
			localPosition.z = -12 - (cameraHeightOffset * offset);
			CameraPosition.transform.localPosition = localPosition;

			var followPosition = CameraPosition.transform.position;	
			//followPosition = LookAtTarget.transform.position - cameraTargetOffset;
			followPosition.y = LookAtTarget.transform.position.y + (cameraHeightOffset * offset);
			//followPosition.z = LookAtTarget.transform.position.z - (cameraHeightOffset * offset);
			//followPosition += cameraTargetOffset; 
			//var distance = LookAtTarget.transform

			var minStep = 2f * Time.deltaTime;
			var step = (Vector3.Distance (followPosition, transform.position) * 0.8f) * Time.deltaTime;
			if (step < minStep) {
				step = minStep;
			}

			transform.position = Vector3.MoveTowards (transform.position, followPosition, step);
			//followPosition = transform.localPosition;
			//followPosition.z -= (cameraHeightOffset * offset);
			//transform.localPosition = followPosition;

			transform.LookAt (LookAtTarget.transform);



		} else if (Mode == CameraMode.Explore && !canInputMovement) {
		
			var followPosition = Camera2Position.transform.position;				

			var minStep = 10f * Time.deltaTime;
			var step = (Vector3.Distance (followPosition, transform.position) * 1f) * Time.deltaTime;
			if (step < minStep) {
				step = minStep;
			}

			transform.position = Vector3.MoveTowards (transform.position, followPosition, step);
			transform.LookAt (LookAtTarget.transform);
			explorePosition = new Vector3 (0, 0, 0);

			canInputMovement = Vector3.Distance( transform.position, followPosition) < 1;
		
		}
		else if (Mode == CameraMode.Explore && canInputMovement) {
			//transform.LookAt (LookAtTarget.transform);
			if(explorePosition == new Vector3(0,0,0)) {
				return;
			}
			var followPosition = (explorePosition) ;				

			var minStep = 1f * Time.deltaTime;
			var step = (Vector3.Distance (followPosition, transform.position) * 1f) * Time.deltaTime;
			if (step < minStep) {
				step = minStep;
			}

			transform.position = Vector3.MoveTowards (transform.position, followPosition, step);

		}
		else {
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

    public void FollowAlternate()
    {
        CameraShot1.GetComponent<CameraFollow>().FollowAlternate();
    }


    /*public void FollowCannon()
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
*/

}
