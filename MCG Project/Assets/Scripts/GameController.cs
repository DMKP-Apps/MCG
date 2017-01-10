using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameController : MonoBehaviour {
	public GameObject playerPrefab;
	public List<GameObject> holePrefabs = new List<GameObject> ();
	public int CurrentHole = 1;
	public List<GameObject> bulletPrefabs = new List<GameObject> ();
	//public int CurrentBullet = 1;

	//private bool isHoleComplete = false;
	//private bool isHazardPoint = false;

	//var playerController = player.GetComponent<CannonPlayerState> ();
	//playerController.isHoleComplete = false;
	//playerController.isHarzard = false;

	private List<GameObject> players;
	private GameObject player 
	{ 
		get 
		{ 
			if (players == null || players.Count < 1) {
				return null;
			} else {
				return players [currentPlayer];
			}
			 
		} 
	}
	private bool isHoleComplete 
	{ 
		get 
		{ 
			if (player == null) {
				return false;
			} else {
				var playerController = player.GetComponent<CannonPlayerState> ();
				return playerController.isHoleComplete;
			}

		} 
	}

	private bool isHazardPoint 
	{ 
		get 
		{ 
			if (player == null) {
				return false;
			} else {
				var playerController = player.GetComponent<CannonPlayerState> ();
				return playerController.isHarzard;
			}

		} 
	}

	public int CurrentBullet 
	{ 
		get 
		{ 
			if (player == null) {
				return 1;
			} else {
				var playerController = player.GetComponent<CannonPlayerState> ();
				return playerController.currentBullet;
			}

		} 
		set { 
			var playerController = player.GetComponent<CannonPlayerState> ();
			playerController.currentBullet = value;
		}
	}

	public int NumberOfPlayers = 1;
	private int currentPlayer = 0;
	private GameObject hole;

	public UITextController textController;
	//private GameObject bullet;

	public GameObject ControlsContainer;
	public GameObject HoleCompleteAlert;

	private Dictionary<int, string> scoring = new Dictionary<int, string>();

	public float powerRate = 0f;

	private CameraController cameraController;

	public void RegisterCameraController(CameraController camera)
	{
		cameraController = camera;
	}

	public void Log(string output) {

		textController.Log (output);

	}

	public void Log(string output, params object[] args) {

		textController.Log(output, args);

	}

	void Start() {
	
		scoring.Add (-3, "Albatross");
		scoring.Add (-2, "Eagle");
		scoring.Add (-1, "Birdie");
		scoring.Add (0, "Par");
		scoring.Add (1, "Bogey");
		scoring.Add (2, "Double Bogey");
		scoring.Add (3, "Triple Bogey");

		/*if (holePrefabs.Count > CurrentHole - 1) {
		
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
			
		}*/
	
		BeginHole ();

	}

	public bool IsShooting()
	{
		var fireController = player.GetComponent<CannonFireController> ();
		return fireController.GetBullet ();
	}

	public void TouchDetected() {

		if(hole != null) {
			var holeController = hole.GetComponent<HoleController> ();
			if (holeController.IsHoleCameraActive()) {
				holeController.DeactivateHoleCameras ();
			}

			if (IsShooting () && cameraController != null) {
				// route camera back to cannon.
				cameraController.CameraShot1.GetComponent<CameraFollow>().FollowAlternate();
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



	private void MoveToNextPlayer() {
		if (currentPlayer > -1) {
			player.SetActive (false);
		}
		currentPlayer++;
		if (currentPlayer >= players.Count) {
			currentPlayer = 0;
		}
		var holeController = hole.GetComponent<HoleController> ();
		var playerList = players.Select (p => new { 
			PlayerState = p.GetComponent<CannonPlayerState> (),
			Position = p.transform.position,
			DistanceFromHole = Vector3.Distance(p.transform.position, holeController.hole.position)
		}).ToList();

		if (!playerList.Any (p => p.PlayerState.Stroke < 2) && playerList.Any (p => !p.PlayerState.isHoleComplete)) {
			// every player has shot once... determine the next shooter based on distance from tee.
			var tmpplayer = playerList.Where (p => !p.PlayerState.isHoleComplete).OrderByDescending (p => p.DistanceFromHole)
				.FirstOrDefault ();
			if (tmpplayer != null) {
				currentPlayer = playerList.FindIndex (p => p.PlayerState.playerNumber == tmpplayer.PlayerState.playerNumber);
			}
		}

		player.SetActive (true);

		var playerController = player.GetComponent<CannonPlayerState> ();
		playerController.isHoleComplete = false;
		playerController.isHarzard = false;

		textController.SetStroke (playerController.Stroke);
		textController.SetPlayer (playerController.playerNumber, playerController.TotalScore);

	}

	private void MoveToPosition(Vector3 position) {
	
		player.transform.position = position; 
		var holeController = hole.GetComponent<HoleController> ();
		var playerCannon = player.GetComponent<CannonFireController> ();

		player.transform.forward = holeController.hole.position;
		player.transform.LookAt (holeController.hole.position);
		//player.transform.localRotation = Quaternion.Euler(new Vector3(0f,player.transform.localRotation.y, 0f));
		//player.transform.rotation = Quaternion.Euler(new Vector3(0f,player.transform.rotation.y, 0f));
		playerCannon.AimCannon (holeController.hole.position);
		

	}

	public void StrokeComplete(Vector3 lastPosition)
	{
		if (isHoleComplete || isHazardPoint) {
			MoveToNextPlayer ();
			return;
		}

		// move player into the next position
		MoveToPosition(lastPosition);

		var playerController = player.GetComponent<CannonPlayerState> ();
		playerController.Stroke++;
		//textController.SetStroke (playerController.Stroke);

		MoveToNextPlayer ();

	}

	private Vector3 lastContactPoint;
	public Vector3 GetLastHazardContactPoint() {
		return lastContactPoint;
	}

	public void WaterHazard(GameObject water, Vector3 contactPoint)
	{
		var playerController = player.GetComponent<CannonPlayerState> ();
		playerController.Stroke += 2;
		playerController.isHarzard = true;

		lastContactPoint = contactPoint;
		//isHazardPoint = true;
		// get the closet respawn point for the current water hazard
		var respawnPosition = water.transform.FindChild("Respawn");
		if (respawnPosition != null) {

			// move player into the next position
			MoveToPosition(respawnPosition.transform.position);
		}

		textController.ShowWaterHazard ();


	}

	public void OutOfBounds(Vector3 contactPoint)
	{
		var playerController = player.GetComponent<CannonPlayerState> ();
		playerController.Stroke += 2;
		playerController.isHarzard = true;
		
		lastContactPoint = contactPoint;
		//isHazardPoint = true;

		//var playerController = player.GetComponent<CannonPlayerState> ();
		//playerController.Stroke += 2;
		//textController.SetStroke (playerController.Stroke);

		textController.ShowOutOfBounds ();

	}


	public void HoleOver()
	{
		if (isHoleComplete) {
			return;
		}
		//isHoleComplete = true;
		//isHazardPoint = false;
		var playerController = player.GetComponent<CannonPlayerState> ();
		playerController.isHoleComplete = true;

		var holeController = hole.GetComponent<HoleController> ();


		//var playerController = player.GetComponent<CannonPlayerState> ();

		var score = playerController.Stroke;
		var par = holeController.Par;

		//playerController.Stroke = 1;
		//textController.SetStroke (playerController.Stroke);

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

		holeController.allPlayersComplete = !players.Select (p => p.GetComponent<CannonPlayerState> ()).Any (p => !p.isHoleComplete);
		holeController.HoleCompleted ();

		textController.SetHoleCompleteScore (playerController.playerNumber, scoreValue);
		//textController.SetPlayer (1, playerController.TotalScore);
	}

	public void NextHole()
	{
		var holeController = hole.GetComponent<HoleController> ();

		//var allPlayersComplete = !players.Select (p => p.GetComponent<CannonPlayerState> ()).Any (p => !p.isHoleComplete);
		if (!holeController.allPlayersComplete) {
			return;
		}

		CurrentHole++;
		if (CurrentHole > holePrefabs.Count) {
			CurrentHole = 1;
		}



		//player.transform.position = holeController.tee.position; 
		//player.transform.rotation = holeController.tee.rotation;

		// move player into the next position
		//MoveToPosition(holeController.tee.position);



		/*var playerController = player.GetComponent<CannonPlayerState> ();
		var totalScore = playerController.TotalScore;

		Destroy (player);

		player = (GameObject)Instantiate (playerPrefab, holeController.tee.position, holeController.tee.rotation);

	 	playerController = player.GetComponent<CannonPlayerState> ();
		playerController.Stroke = 1;
		playerController.TotalScore = totalScore;
		textController.SetStroke (playerController.Stroke);
		textController.SetPar (holeController.Par);
		textController.SetPlayer (1, totalScore);
		textController.SetHole (CurrentHole);
		*/
		BeginHole ();

	}

	class playerState {
		public int TotalScore;
		public int StrokeCount;
		public int PlayerNumber;
		public int CurrentBullet;
	}

	private void BeginHole() {
		
		//isHoleComplete = false;
		//isHazardPoint = false;

		// destroy the previous hole if exists
		if(hole != null) {
			Destroy (hole);
		}

		hole = (GameObject)Instantiate (holePrefabs [CurrentHole - 1]);
		var holeController = hole.GetComponent<HoleController> ();
		holeController.allPlayersComplete = false;
		var watchActivate = HoleCompleteAlert.GetComponent<ActivateWithGameObject> ();
		watchActivate.WatchObject = holeController.EndCamera;

		textController.SetPar (holeController.Par);
		textController.SetHole (CurrentHole);

		if (players == null) {
			players = new List<GameObject> ();
		}
		List<playerState> playerScores = new List<playerState> ();
		if (players.Count > 0) {
			players.ForEach (p => { 

				var playerController = p.GetComponent<CannonPlayerState> ();
				playerState state = new playerState() {
					TotalScore = playerController.TotalScore,
					StrokeCount = playerController.Stroke,
					PlayerNumber = playerController.playerNumber,
					CurrentBullet = playerController.currentBullet
				};
				playerScores.Add(state);
				Destroy (p); 
			});
		}
		players = new List<GameObject> ();

		if (NumberOfPlayers < 1) {
			NumberOfPlayers = 1;
		}
		if (NumberOfPlayers > 4) {
			NumberOfPlayers = 4;
		}

		currentPlayer = 0;
		if (playerScores.Count > 0) {

			playerScores.OrderBy (x => x.StrokeCount).ToList ().ForEach (p => {
				var player = (GameObject)Instantiate (playerPrefab, holeController.tee.position, holeController.tee.rotation);

				var pController = player.GetComponent<CannonPlayerState> ();
				pController.Stroke = 1;
				pController.TotalScore = p.TotalScore;
				pController.playerNumber = p.PlayerNumber;
				pController.isHoleComplete = false;
				pController.isHarzard = false;
				pController.currentBullet = p.CurrentBullet;

				// set the current object to in-active
				player.SetActive(false);
				// add the game object to the list
				players.Add(player);

			});

		} else {
			// create the player and de-activate them within the scene
			for(int i = 0; i < NumberOfPlayers; i++) {
				var player = (GameObject)Instantiate (playerPrefab, holeController.tee.position, holeController.tee.rotation);

				var pController = player.GetComponent<CannonPlayerState> ();
				pController.Stroke = 1;
				pController.TotalScore = 0;
				pController.playerNumber = i + 1;
				pController.isHoleComplete = false;
				pController.isHarzard = false;
				// set the current object to in-active
				player.SetActive(false);
				// add the game object to the list
				players.Add(player);
			}
		}
		//textController.SetPlayer (1);
		currentPlayer = -1;
		MoveToNextPlayer ();
	}

	public GameObject GetCurrentCannonPlayer()
	{
		return player;

	}

	public GameObject GetCurrentBullet()
	{
		//isHazardPoint = false;
		//isHoleComplete = false;
		if (bulletPrefabs != null && bulletPrefabs.Count >= CurrentBullet) {
			var bullet = bulletPrefabs [CurrentBullet - 1];

			return bullet;
		} else
			return null;


	}

	public void OnAddPlayer() {
		NumberOfPlayers++;
		if (players != null) {
			players.ForEach (p => { 
				Destroy (p); 
			});
		}
		players = new List<GameObject> ();
		BeginHole ();
	}
	public void OnMinusPlayer() {
		NumberOfPlayers--;
		if (players != null) {
			players.ForEach (p => { 
				Destroy (p); 
			});
		}
		players = new List<GameObject> ();
		BeginHole ();
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
