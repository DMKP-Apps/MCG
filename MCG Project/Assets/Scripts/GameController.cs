using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
	public GameObject playerPrefab;
	public List<GameObject> holePrefabs = new List<GameObject> ();
	public int CurrentHole = 1;

	private GameObject player;
	private GameObject hole;

	void Start() {
	
		if (holePrefabs.Count > CurrentHole - 1) {
		
			hole = (GameObject)Instantiate (holePrefabs [0]);
		
		}

		if (playerPrefab != null && hole != null) {
			var holeController = hole.GetComponent<HoleController> ();
			player = (GameObject)Instantiate (playerPrefab, holeController.tee.position, holeController.tee.rotation);
			
		}
	
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
