using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonController : MonoBehaviour {


	public GameObject bulletPrefab;
	public GameObject BulletPosition;
	public GameObject FirePrefab;
	public GameObject FirePosition;
	public Transform StartPosition;

	public GameObject Cannon;
	//public GameObject Hole;


	public GameObject Spark;


	private GameController GameController;

	private InputController inputController;

	private GameObject bullet;

	private Vector3 BulletLastPosition = new Vector3 ();

	private Transform respawnPoint = null;

	public int StrokeCount = 0;

	private GuageController GaugeController;


	void Start () {
		inputController = Cannon.GetComponent<InputController> ();
		GameController = GameObject.Find ("GameController").GetComponent<GameController> ();
		GaugeController = GameObject.Find ("Gauge").GetComponent<GuageController> ();

		MoveToPosition(StartPosition.position);

	}
	
	void Update () {


		//GameController.GetCameraController ().Position = Cannon.transform.position;
		//GameController.GetCameraController ().Rotation = Cannon.transform.rotation.eulerAngles;

		if (bullet != null) {		
			//GameController.GetCameraController ().Position = bullet.gameObject.transform.position;
			BulletLastPosition = bullet.gameObject.transform.position;
		} //else if(GameController.GetCameraController ().Mode == CameraController.CameraMode.FollowBullet) {
		//	if (respawnPoint != null) {
		//		MoveToPosition(respawnPoint.position);
		//	} else {
		//		MoveToPosition(BulletLastPosition);
		//	}

		//}

	}

	public void SetRespawnPoint(Transform target) {
	
		respawnPoint = target;
	
	}

	public void MoveToPosition(Vector3 target)
	{
		//GameController.GetCameraController ().Bullet = null;
		//GameController.GetCameraController ().SetMode(CameraController.CameraMode.FollowCannon);
		GaugeController.gameObject.SetActive (true);

		transform.position = target;
		transform.position = new Vector3 (transform.position.x, transform.position.y + 2, transform.position.z);
		//transform.forward = Hole.transform.position;
		transform.rotation = Quaternion.Euler(0, transform.rotation.y, 0);
		//transform.LookAt(Hole.transform);
		//Cannon.transform.LookAt(Hole.transform);
		respawnPoint = null;
	}

	public enum CannonState
	{
		None,
		Ready,
		Set
	}

	public CannonState State = CannonState.None;

	public void Execute()
	{
		if (bullet != null) {
			return;
		}

		switch (State) {
		case CannonState.None:
			Ready ();
			break;
		case CannonState.Ready:
			Set ();
			break;

		case CannonState.Set:
			Fire ();
			break;
		}
	}

	void Ready()
	{
		State = CannonState.Ready;
		Debug.Log ("Ready!");
		GaugeController.OnSet ();

	}

	public float ShotPower = 50.0f;
	public float PowerRate = 1.0f;

	void Set()
	{
		State = CannonState.Set;
		Debug.Log ("Set!");
		var system = Spark.GetComponent<ParticleSystem>();
		system.Play ();
		GaugeController.OnFire ();
		
	}

	void Fire()
	{
		

		State = CannonState.None;
		GaugeController.OnReady ();
		GaugeController.gameObject.SetActive (false);
		Debug.Log (Application.targetFrameRate);

		bullet = (GameObject)Instantiate(
			bulletPrefab, BulletPosition.transform.position, BulletPosition.transform.rotation);//new Vector3(transform.position.x, transform.position.y, transform.position.z + 10),

		//GameController.GetCameraController ().Bullet = bullet;
		//GameController.GetCameraController ().SetMode( CameraController.CameraMode.FollowBullet);
		//GameController.GetCameraController ().Position = BulletPosition.transform.position;

		var fire = (GameObject)Instantiate(
			FirePrefab, FirePosition.transform.position, FirePosition.transform.rotation);//new Vector3(transform.position.x, transform.position.y, transform.position.z + 10),
		
		var torque = Random.Range (-20.0f, 20.0f);
		var turn = Random.Range (-20.0f, 20.0f);

		//bullet.GetComponent<ConstantForce> ().force = 
		//	new Vector3 (GameController.Wind.x, 0.0f, GameController.Wind.y);

		bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * ShotPower * PowerRate, ForceMode.Impulse);
		bullet.GetComponent<Rigidbody>().AddTorque(transform.up * torque * turn);
		bullet.GetComponent<Rigidbody>().AddTorque(transform.right * torque * turn);

		Destroy(bullet, 10.0f);
		Destroy(fire, 5.0f);

		var system = Spark.GetComponent<ParticleSystem>();
		system.Stop ();

		StrokeCount++;
	}
}
