using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
	public GameObject playerPrefab;
	public List<GameObject> holePrefabs = new List<GameObject> ();
	public int CurrentHole = 1;
	public List<GameObject> bulletPrefabs = new List<GameObject> ();
	public int CurrentBullet = 1;

	private bool isHoleComplete = false;
	private bool isHazardPoint = false;

	private GameObject player;
	private GameObject hole;

	public UITextController textController;
	//private GameObject bullet;

	public GameObject ControlsContainer;
	public GameObject HoleCompleteAlert;

	private Dictionary<int, string> scoring = new Dictionary<int, string>();


	void Start() {
	
		scoring.Add (-3, "Albatross");
		scoring.Add (-2, "Eagle");
		scoring.Add (-1, "Birdie");
		scoring.Add (0, "Par");
		scoring.Add (1, "Bogey");
		scoring.Add (2, "Double Bogey");
		scoring.Add (3, "Triple Bogey");

		if (holePrefabs.Count > CurrentHole - 1) {
		
			hole = (GameObject)Instantiate (holePrefabs [CurrentHole - 1]);

		}

		if (playerPrefab != null && hole != null) {
			var holeController = hole.GetComponent<HoleController> ();
			player = (GameObject)Instantiate (playerPrefab, holeController.tee.position, holeController.tee.rotation);

			var playerController = player.GetComponent<CannonPlayerState> ();
			playerController.Stroke = 1;
			textController.SetStroke (playerController.Stroke);
			textController.SetPar (holeController.Par);
			textController.SetPlayer (1);
			textController.SetHole (CurrentHole);

			var watchActivate = HoleCompleteAlert.GetComponent<ActivateWithGameObject> ();
			watchActivate.WatchObject = holeController.EndCamera;
			
		}
	
	}

	public void TouchDetected() {

		if(hole != null) {
			var holeController = hole.GetComponent<HoleController> ();
			if (holeController.IsHoleCameraActive()) {
				holeController.DeactivateHoleCameras ();
			}

		}


	}

	void Update() 
	{
		var setActive = true;

		if(hole != null) {
			var holeController = hole.GetComponent<HoleController> ();
			setActive = !holeController.IsHoleCameraActive ();
		}
		if (setActive && player != null) {
			var fireController = player.GetComponent<CannonFireController> ();
			setActive = fireController.GetBullet () == null;
		}

		ControlsContainer.SetActive (setActive);
	}

	private void MoveToPosition(Vector3 position) {
	
		player.transform.position = position; 

		var holeController = hole.GetComponent<HoleController> ();
		player.transform.forward = holeController.hole.position;
		player.transform.LookAt (holeController.hole.position);

		var playerCannon = player.GetComponent<CannonFireController> ();
		playerCannon.AimCannon (holeController.hole.position);
	
	}

	public void StrokeComplete(Vector3 lastPosition)
	{
		if (isHoleComplete || isHazardPoint) {
			return;
		}
		// move player into the next position
		MoveToPosition(lastPosition);

		var playerController = player.GetComponent<CannonPlayerState> ();
		playerController.Stroke++;
		textController.SetStroke (playerController.Stroke);

	}

	private Vector3 lastContactPoint;
	public Vector3 GetLastHazardContactPoint() {
		return lastContactPoint;
	}

	public void WaterHazard(GameObject water, Vector3 contactPoint)
	{
		lastContactPoint = contactPoint;
		isHazardPoint = true;
		// get the closet respawn point for the current water hazard
		var respawnPosition = water.transform.FindChild("Respawn");
		if (respawnPosition != null) {

			// move player into the next position
			MoveToPosition(respawnPosition.transform.position);
		}

		var playerController = player.GetComponent<CannonPlayerState> ();
		playerController.Stroke += 2;
		textController.SetStroke (playerController.Stroke);

		textController.ShowWaterHazard ();

	}

	public void OutOfBounds(Vector3 contactPoint)
	{
		lastContactPoint = contactPoint;
		isHazardPoint = true;

		var playerController = player.GetComponent<CannonPlayerState> ();
		playerController.Stroke += 2;
		textController.SetStroke (playerController.Stroke);

		textController.ShowOutOfBounds ();

	}


	public void HoleOver()
	{
		if (isHoleComplete) {
			return;
		}
		isHoleComplete = true;
		isHazardPoint = false;
		var holeController = hole.GetComponent<HoleController> ();
		holeController.HoleCompleted ();

		var playerController = player.GetComponent<CannonPlayerState> ();

		var score = playerController.Stroke;
		var par = holeController.Par;

		playerController.Stroke = 1;
		textController.SetStroke (playerController.Stroke);

		var scoreValue = string.Empty;
		if (score == par) {
			score = 0;
			scoreValue = string.Format("{0} ({1})", scoring[0], 0);
		}
		else if (score < par) {
			score = (par - score) * -1;
			if (!scoring.ContainsKey(score)) {
				scoreValue = string.Format ("{0} ({1})", "Under", score);
			} else {
				scoreValue = string.Format("{0} ({1})", scoring[score], score);
			}

		}
		else {
			score = (score - par);
			if (!scoring.ContainsKey(score)) {
				scoreValue = string.Format ("{0} (+{1})", "Over", score);
			} else {
				scoreValue = string.Format("{0} (+{1})", scoring[score], score);
			}
		}

		playerController.TotalScore += score;

		textController.SetHoleCompleteScore (1, scoreValue);
		textController.SetPlayer (1, playerController.TotalScore);
	}

	public void NextHole()
	{
		isHoleComplete = false;
		isHazardPoint = false;

		CurrentHole++;
		if (CurrentHole + 1 > holePrefabs.Count) {
			CurrentHole = 1;
		}

		// destroy the previous hole if exists
		if(hole != null) {
			Destroy (hole);
		}

		hole = (GameObject)Instantiate (holePrefabs [CurrentHole - 1]);
		var holeController = hole.GetComponent<HoleController> ();

		var watchActivate = HoleCompleteAlert.GetComponent<ActivateWithGameObject> ();
		watchActivate.WatchObject = holeController.EndCamera;

		textController.SetPar (holeController.Par);
		textController.SetPlayer (1);
		textController.SetHole (CurrentHole);

		//player.transform.position = holeController.tee.position; 
		//player.transform.rotation = holeController.tee.rotation;

		// move player into the next position
		MoveToPosition(holeController.tee.position);

	}

	public GameObject GetCurrentCannonPlayer()
	{
		return player;

	}

	public GameObject GetCurrentBullet()
	{
		isHazardPoint = false;
		isHoleComplete = false;
		if (bulletPrefabs != null && bulletPrefabs.Count >= CurrentBullet) {
			var bullet = bulletPrefabs [CurrentBullet - 1];

			return bullet;
		} else
			return null;


	}

}

/*
public class GameController : MonoBehaviour {

	public Vector2 Wind = new Vector2 (0.0f, 0.0f);

	public GameObject MainCamera;
	public GameObject Cannon;
	public GameObject Hole;
	private CameraController CameraController;
	private CannonController CannonController;

	public Transform weatherVane;

	public Text WindCaption;
	public Text HoleCaption;
	public Text ParCaption;
	public Text StrokeCaption;
	public Text ScoreCaption;

	private int currentPar;

	public CameraController GetCameraController()
	{
		return CameraController;
	}

	// Use this for initialization
	void Start () {
		CameraController = MainCamera.GetComponent<CameraController> ();
		CannonController = Cannon.GetComponent<CannonController> ();
		currentPar = Hole.gameObject.GetComponent<HoleController> ().Par;
		//Wind = new Vector2 (Random.Range(-1.5f,1.5f), Random.Range(-1.5f,1.5f));
	}
	
	// Update is called once per frame
	void Update () {
		
		var force = new Vector3 (Wind.x, 0.0f, Wind.y);
		weatherVane.forward = force.normalized;
		WindCaption.text = string.Format ("Wind: {0} mph", force.magnitude.ToString("0.00"));
		StrokeCaption.text = string.Format ("Stroke: {0}", CannonController.StrokeCount);
		ParCaption.text = string.Format ("Par: {0}", currentPar);

	}

	public void Reset ()
	{
		ScoreCaption.text = string.Format ("Score: {0}", currentPar > CannonController.StrokeCount ? "-" + (currentPar - CannonController.StrokeCount).ToString() : currentPar < CannonController.StrokeCount ? "+" + (CannonController.StrokeCount - currentPar).ToString() : "E" );
		var camera = Hole.transform.FindChild ("HoleCamera");
		if (camera != null) {
		
			var cameraActivate = camera.gameObject.GetComponent<ActiveForTime> ();
			if (cameraActivate != null) {
				cameraActivate.Show ();			
			}
		
		}
		currentPar = Hole.gameObject.GetComponent<HoleController> ().Par;
		CannonController.StrokeCount = 0;
		CannonController.MoveToPosition (CannonController.StartPosition.position);
		//Wind = new Vector2 (Random.Range(-1.5f,1.5f), Random.Range(-1.5f,1.5f));
	}

	/\*public void WaterHazard (GameObject water, Vector3 target)
	{
		CannonController.StrokeCount++;
		CameraController.RunActionCamera = false;
		// check if the water has a camera
		var camera = water.transform.FindChild("WaterCamera");

		if (camera != null) {
			var waterCamera = camera.GetComponent<WaterCamera> ();
			if (waterCamera != null) {
			
				waterCamera.SetFocus (target);
			}

		}

		//var respawnTarget = water.transform.FindChild("Respawn");
		Transform respawnPoint = null;
		foreach(Transform child in water.transform){
			if (child.tag == "Respawn") {
				respawnPoint = child.gameObject.transform;
				break;
			}
		}
		if (respawnPoint != null) {
			CannonController.SetRespawnPoint (respawnPoint);
		}
	}/\*

 
}*/
