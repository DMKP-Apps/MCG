using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HoleController : MonoBehaviour {

	public int Par = 3;

	private List<Transform> raceTeePositions = null;
	private Transform teePosition = null;
	private Transform holePosition = null;

	public bool allPlayersComplete = false;	
	public GameObject StartCamera;
	public GameObject EndCamera;
	public CameraPathAnimator EndCameraAnimator;
	public GameController GameController;

	public string HoleTitle = string.Empty;

	public int showPlayerHoleCompleteFor = 5000; 
	private System.DateTime startTime = System.DateTime.MaxValue;

	void Start() {
		GameController = GameObject.Find ("GameController").GetComponent<GameController> ();
		if (string.IsNullOrEmpty (HoleTitle)) {
			HoleTitle = gameObject.name;
		}
	}

	public Transform tee {
		get { 
			if (teePosition == null) {
				foreach (Transform child in this.transform) {
					if (child.tag == "Tee") {
						teePosition = child.gameObject.transform;
						break;
					}
				}
			}
			return teePosition;
		}
	}

	public List<Transform> raceTees {
		get { 
			if (raceTeePositions == null) {
				List<Transform> positions = new List<Transform> ();
				foreach (Transform child in this.transform) {
					if (child.tag == "TeeRace") {
						positions.Add(child.gameObject.transform);
					}
				}
				raceTeePositions = positions.OrderBy (x => x.gameObject.name).ToList ();
			}
			return raceTeePositions;
		}
	}

	public void HoleCompleted()
	{
		EndCamera.SetActive (true);
		EndCameraAnimator.startPercent = 0;
		EndCameraAnimator.Play ();


		if (!allPlayersComplete || GameSettings.playerMode == PlayerMode.ServerMultiplayer) {
			startTime = System.DateTime.Now;
		} else {
			startTime = System.DateTime.MaxValue;
		}

	}

	public void Update() {
		if (startTime != System.DateTime.MaxValue && EndCamera.activeInHierarchy) {
			if (System.DateTime.Now.Subtract (startTime).TotalMilliseconds > showPlayerHoleCompleteFor) {
				startTime = System.DateTime.MaxValue;
				DeactivateEndCamera ();
			}
		}
	}

	public Transform hole {
		get { 
			if (holePosition == null) {
				foreach (Transform child in this.transform) {
					if (child.tag == "Hole") {
						holePosition = child.gameObject.transform;
						break;
					}
				}
			}
			return holePosition;
		}
	}

	public void DeactivateHoleCameras() {
	
		if (StartCamera != null && StartCamera.activeInHierarchy) {
			DeactivateStartCamera ();
		}
		if (EndCamera != null && EndCamera.activeInHierarchy) {
			DeactivateEndCamera ();
		}

	
	}

	public void DeactivateStartCamera()
	{
		if (StartCamera != null) {
			StartCamera.SetActive (false);
		}
	}

	public void DeactivateEndCamera()
	{
		if (EndCamera != null) {
			EndCamera.SetActive (false);
			GameController.NextHole ();
			EndCameraAnimator.Stop ();
		}
	}


	public bool IsHoleCameraActive()
	{
		return StartCamera.activeInHierarchy || EndCamera.activeInHierarchy;
	}
}
