﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
	public GameObject playerPrefab;
	public GameObject playerOnlinePrefab;
	public List<GameObject> holePrefabs = new List<GameObject> ();
	public int CurrentHole = 1;
	public List<GameObject> bulletPrefabs = new List<GameObject> ();
    //public int CurrentBullet = 1;
    private GameObject BulletCamera;
    private TestController testController;

    private string endMatchGameScene = "EndMatch";

    //private bool isHoleComplete = false;
    //private bool isHazardPoint = false;

    //var playerController = player.GetComponent<CannonPlayerState> ();
    //playerController.isHoleComplete = false;
    //playerController.isHarzard = false;

    public List<Material> CannonBarrelMaterials = new List<Material>();
    public List<Material> CannonWheelMaterials = new List<Material>();

    public int currentCannonBarrelMaterial = 1;
    public int currentCannonWheelMaterial = 1;

    public PowerControl speedControl;
    public PowerControl accuracyControl;

    void Start()
    {

        var touchUtility = GameObject.FindObjectOfType<TouchUtility>();
        if (touchUtility != null)
        {
            touchUtility.Subscribe(this, (touches) => TouchDetected(), TouchEventType.Began);
        }

        testController = GameObject.FindObjectOfType<TestController>();
        BulletCamera = GameObject.Find("BulletCamera");
        BulletCamera.SetActive(false);

        scoring.Add(-3, "Albatross");
        scoring.Add(-2, "Eagle");
        scoring.Add(-1, "Birdie");
        scoring.Add(0, "Par");
        scoring.Add(1, "Bogey");
        scoring.Add(2, "Double Bogey");
        scoring.Add(3, "Triple Bogey");

        var tcam = GameObject.FindObjectOfType<TopographicCamera>();
        if (tcam != null)
        {
            topographicCamera = tcam.gameObject;
        }

        switch (GameSettings.playerMode)
        {
            case PlayerMode.Single:
                NumberOfPlayers = 1;
                break;
            case PlayerMode.LocalMultiplayer:
                NumberOfPlayers = GameSettings.LocalMultiplayerCount < 2 ? 2 : GameSettings.LocalMultiplayerCount;
                break;
            case PlayerMode.ServerMultiplayer:
                NumberOfPlayers = GameSettings.Room.attendees.Count;
                CurrentHole = GameSettings.HoleStatus.currentHoleIndex;
                break;
        }
        
        BeginHole();
        CurrentBullet = 1;
    }

    void OnApplicationFocus( bool hasFocus )
	{
		if (!hasFocus && NetworkClientManager.IsOnline) {
			//this.EndGame ();
		}
	}



	public void EndGame() {
		NetworkClientManager.Logoff ();
		SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);

	}

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
            var bullet = GetCurrentBullet().GetComponent<BulletData>();
            if (speedControl != null && bullet != null)
            {
                speedControl.Speed = speedControl.BaseSpeed * bullet.PowerMeterSpeedMultiplier;
            }
            if (accuracyControl != null && bullet != null)
            {
                accuracyControl.Speed = speedControl.BaseSpeed * bullet.AccuracyMeterSpeedMultiplier;
            }
        }
	}

	public int NumberOfPlayers = 1;
	private int currentPlayer = 0;
	private GameObject hole;

	public UITextController textController;
	//private GameObject bullet;

	public GameObject ControlsContainer;
	public GameObject HoleCompleteAlert;

    private GameObject topographicCamera;


    private Dictionary<int, string> scoring = new Dictionary<int, string>();

	public float powerRate = 0f;

	private MainCameraController cameraController;

	public void RegisterCameraController(MainCameraController camera)
	{
		cameraController = camera;
	}

    public MainCameraController getMainCamera()
    {
        return cameraController;
    }

    public void Log(string output) {

		textController.Log (output);

	}

	public void Log(string output, params object[] args) {

		textController.Log(output, args);
        
	}

    public GameObject GetBulletCamera()
    {
        return BulletCamera;
    }


	public bool IsShooting()
	{
        if (player == null) {
            return false;
        }
		var fireController = player.GetComponent<CannonFireController> ();
		return fireController.GetBullet ();
	}

    public string GetHoleId()
    {
        return hole == null ? null : hole.gameObject.name;
    }

	public Vector3 GetHolePinPosition()
	{
		return hole == null ? new Vector3() : hole.GetComponent<HoleController>().hole.transform.position;
	}

    public float GetDistanceToPin()
    {
        return hole.GetComponent<HoleController>().GetDistanceToPin(player.transform);
    }

	public bool IsHoleCameraActive()
	{
		var holeController = hole.GetComponent<HoleController> ();
		return holeController.IsHoleCameraActive ();
	}

	public GameObject GetCurrentShotBullet()
	{
		if (player == null) {
			return null;
		}
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
				//cameraController.FollowAlternate();
			}
		}


	}

	private System.DateTime lastPlayerInfoRequest = System.DateTime.Now;
	private bool checkEndingHole = false;
	void Update() 
	{
		var setActive = true;

		if(hole != null) {
			var holeController = hole.GetComponent<HoleController> ();
			setActive = !holeController.IsHoleCameraActive ();
            textController.playerInfo.gameObject.SetActive(setActive);

        }
		if (setActive && player != null) {
			var fireController = player.GetComponent<CannonFireController> ();
            setActive = !fireController.IsFiring();
		}
		if (cameraController != null && cameraController.Mode == MainCameraController.CameraMode.Explore) {
			setActive = false;
		}

		ControlsContainer.SetActive (setActive);
        if (topographicCamera != null)
        {
            topographicCamera.SetActive(setActive);
        }

        //if (1 == 1) {
        //    var holeController = hole.GetComponent<HoleController>();
        //    holeController.hole.position = GameSettings.EstimatedShotLocation;
        //    //GameSettings.EstimatedShotLocation
        //}

        if (GameSettings.playerMode == PlayerMode.ServerMultiplayer && !buildingHole) {
			if (System.DateTime.Now.Subtract (lastPlayerInfoRequest).TotalMilliseconds > 1000 && !checkEndingHole) {
				
				lastPlayerInfoRequest = System.DateTime.Now;
				NetworkClientManager.GetGameInfo (this, () => {
					if(checkEndingHole || buildingHole) {
						return;
					}
					checkEndingHole = true;
					try
					{
						if(GameSettings.GameInfo != null && GameSettings.GameInfo.Hole != null) {

                            if (GameSettings.GameInfo.Hole.status != RoomStatus.InProgress)
                            {   // hole is not current running...
                                if (GameSettings.GameInfo.Hole.status == RoomStatus.Closed)
                                {   // todo... show game closed window....
                                    NetworkClientManager.Logoff();
                                    SceneManager.LoadScene("GameClosed", LoadSceneMode.Single);
                                }
                                else if (GameSettings.GameInfo.Hole.status == RoomStatus.HoleCompleted)
                                {   // hole is over... continue to end match scene...
                                    if (GameSettings.GameInfo.Hole._1st != null && GameSettings.GameInfo.Hole._1st.UID == NetworkClientManager.player.UID)
                                    {
                                        GameSettings.HoleStatus.playerRanking = 1;
                                    }
                                    else if (GameSettings.GameInfo.Hole._2nd != null && GameSettings.GameInfo.Hole._2nd.UID == NetworkClientManager.player.UID)
                                    {
                                        GameSettings.HoleStatus.playerRanking = 2;
                                    }
                                    else if (GameSettings.GameInfo.Hole._3rd != null && GameSettings.GameInfo.Hole._3rd.UID == NetworkClientManager.player.UID)
                                    {
                                        GameSettings.HoleStatus.playerRanking = 3;
                                    }
                                    else
                                    {
                                        GameSettings.HoleStatus.playerRanking = 4;
                                    }

                                    SceneManager.LoadScene(endMatchGameScene, LoadSceneMode.Single);
                                }
                            }
                            
						}
					}
					finally
					{
						checkEndingHole = false;
					}


				});
			}
		}

	}



	private void MoveToNextPlayer() {
		var holeController = hole.GetComponent<HoleController> ();
		if (GameSettings.playerMode != PlayerMode.ServerMultiplayer || (GameSettings.playerMode == PlayerMode.ServerMultiplayer && !GameSettings.isRace)) {
			if (currentPlayer > -1) {
                player.GetComponent<CannonPlayerState>().SetPlayerActive(false);
                player.SetActive (false);
			}
			currentPlayer++;
			if (currentPlayer >= players.Count) {
				currentPlayer = 0;
			}
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
        }
			
		var playerController = player.GetComponent<CannonPlayerState> ();
        playerController.SetPlayerActive(true);
        playerController.isHoleComplete = false;
		playerController.isHarzard = false;

		textController.SetStroke (playerController.Stroke);
		textController.SetPlayer (playerController.playerNumber, playerController.TotalScore);

        

    }

	private void MoveToPosition(Vector3 position) {

        position.y += 0.5f;
        player.transform.position = position;
        		
		var holeController = hole.GetComponent<HoleController> ();
		var playerCannon = player.GetComponent<CannonFireController> ();

        var nextCheckPoint = holeController.GetNextCheckPointLocation(player.transform);

        player.transform.forward = nextCheckPoint.position;
		player.transform.LookAt (nextCheckPoint.position);
		player.transform.rotation = Quaternion.Euler(new Vector3(0f,player.transform.rotation.eulerAngles.y, 0f));
		playerCannon.AimCannon (nextCheckPoint.position);

        var playerState = player.GetComponent<CannonPlayerState>();
        if (playerState.MoveToParent != null)
        {
            player.transform.parent = null;
            player.transform.localScale = new Vector3(1, 1, 1);
            player.transform.parent = playerState.MoveToParent;
            playerState.MoveToParent = null;
        }
        else if (player.transform.parent != null) {
            player.transform.parent = null;
            player.transform.localScale = new Vector3(1, 1, 1);
        }

        //var distance = holeController.GetNextCheckPointLocation(player.transform);

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

	private CannonPlayerState GetPlayerState(GameObject item) {

        if (item.GetComponent<AssociatedPlayerState>() != null)
        {
            if (item.GetComponent<AssociatedPlayerState>().playerState != null)
            {
                return item.GetComponent<AssociatedPlayerState>().playerState;
            }
        }

		CannonPlayerState playerController = item.GetComponent<CannonPlayerState> ();
		var parent = item.transform.parent;
		while (parent != null && playerController == null) {
            if (parent.GetComponent<AssociatedPlayerState>() != null)
            {
                if (parent.GetComponent<AssociatedPlayerState>().playerState != null)
                {
                    playerController = parent.GetComponent<AssociatedPlayerState>().playerState;
                    break;
                }
            }

            playerController = parent.GetComponent<CannonPlayerState> ();
			if (playerController != null) {
				break;
			}
			parent = parent.parent;
		}
		return playerController;
	}

	public void WaterHazard(GameObject water, Vector3 contactPoint, GameObject bullet)
	{
		CannonPlayerState playerController = GetPlayerState (bullet);

		if (playerController == null) {
			return;
		}

		playerController.Stroke += 2;
		playerController.isHarzard = true;

		if (playerController.isOnlinePlayer && GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.isRace) {
			return;
		} 

		lastContactPoint = contactPoint;
        //isHazardPoint = true;
        // get the closet respawn point for the current water hazard
        var respawnPoints = GetChildrenByTag(water.transform, "Respawn");
        var respawnPosition = respawnPoints.Select(x => new
        {
            transform = x,
            distance = Vector3.Distance(x.position, bullet.transform.position)
        }).OrderBy(x => x.distance).FirstOrDefault();

        if (respawnPosition != null) {

            playerController.MoveToParent = respawnPosition.transform;
            // move player into the next position
            MoveToPosition(respawnPosition.transform.position);
		}

		textController.ShowWaterHazard ();


	}

    private List<Transform> GetChildrenByTag(Transform parent, string tag)
    {
        var waterData = parent.gameObject.GetComponent<WaterData>();
        if (waterData != null) {
            return waterData.respawnPositions;
        }

        List<Transform> results = new List<Transform>();
        if (parent != null)
        {
            foreach (Transform child in parent)
            {
                if (child.tag == tag)
                {
                    results.Add(child.gameObject.transform);
                }
            }
        }
        return results;
        
    }



    public void OutOfBounds(Vector3 contactPoint, GameObject bullet)
	{
		CannonPlayerState playerController = GetPlayerState (bullet);

		if (playerController == null) {
			return;
		}

		playerController.Stroke += 2;
		playerController.isHarzard = true;

		if (playerController.isOnlinePlayer && GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.isRace) {
			return;
		} 

		
		lastContactPoint = contactPoint;
		//isHazardPoint = true;

		//var playerController = player.GetComponent<CannonPlayerState> ();
		//playerController.Stroke += 2;
		//textController.SetStroke (playerController.Stroke);

		textController.ShowOutOfBounds ();

	}

    public void UnplayableHazard(GameObject bullet, Vector3 contactPoint)
    {
        CannonPlayerState playerController = GetPlayerState(bullet);

        if (playerController == null)
        {
            return;
        }

        playerController.Stroke += 2;
        playerController.isHarzard = true;

        if (playerController.isOnlinePlayer && GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.isRace)
        {
            return;
        }


        lastContactPoint = contactPoint;
        textController.UnplayableHazard();

    }

    public void HoleOver(GameObject bullet)
	{
		CannonPlayerState playerController = GetPlayerState (bullet);

		if (isHoleComplete) {
			return;
		}

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

		if (GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.isRace) {
			if (playerController.isOnlinePlayer && GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.isRace) {
				return;
			} 

			playerController.SendNetworkObjectData ();
			holeController.EndCamera.SetActive (true);
			playerController.gameObject.SetActive (false);
			return;
		} 



		holeController.HoleCompleted ();

		textController.SetHoleCompleteScore (playerController.playerNumber, scoreValue);
		//textController.SetPlayer (1, playerController.TotalScore);
	}

	public void NextHole()
	{
		var holeController = hole.GetComponent<HoleController> ();

		if (!holeController.allPlayersComplete) {
			return;
		}

		CurrentHole++;
		if (CurrentHole > holePrefabs.Count) {
			CurrentHole = 1;
		}


		BeginHole ();

	}

	class playerState {
		public int TotalScore;
		public int StrokeCount;
		public int PlayerNumber;
		public int CurrentBullet;
        public string playerKey;
        public int BarrelMaterial;
        public int WheelMaterial;
    }
	private bool buildingHole= false;
	private void BeginHole() {

		if (buildingHole) {
			return;
		}

		buildingHole = true;
		try
		{
			//isHoleComplete = false;
			//isHazardPoint = false;

			// destroy the previous hole if exists
			if(hole != null) {
				Destroy (hole);
			}

			if (CurrentHole > holePrefabs.Count) {
				CurrentHole = 1;
			}

            if (testController != null)
            {
                testController.SetCurrentHole(CurrentHole);
            }


			hole = (GameObject)Instantiate (holePrefabs [CurrentHole - 1]);
			var holeController = hole.GetComponent<HoleController> ();
			holeController.allPlayersComplete = false;
			var watchActivate = HoleCompleteAlert.GetComponent<ActivateWithGameObject> ();
			watchActivate.WatchObject = holeController.EndCamera;

            if (GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.isRace)
            {
                watchActivate.WatchObject = null;
            }

			if(GameSettings.HoleStatus == null) {
				GameSettings.HoleStatus = new HoleStatus() {
				};
			}
			GameSettings.HoleStatus.currentHoleIndex = CurrentHole;
			GameSettings.HoleStatus.currentHoleName = holeController.HoleTitle;

            if (GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.isRace) {
                holeController.StartCamera.SetActive(false);
            }

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
						CurrentBullet = playerController.currentBullet,
	                    playerKey = playerController.playerKey
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

				int i = 0;
				playerScores.OrderBy (x => x.StrokeCount).ToList ().ForEach (p => {

					var isRacePlayer = false;
					isRacePlayer = GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.isRace && !string.IsNullOrEmpty(p.playerKey)
						&& NetworkClientManager.player != null && NetworkClientManager.player.UID != p.playerKey;

					if(GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.isRace && !isRacePlayer) {
						currentPlayer = i;
					}


					GameObject currentPlayerObject;
					Transform tee = holeController.tee;
                    if (GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.isRace && holeController.raceTees.Count > i)
                    {
                        tee = holeController.raceTees[i];
                    }
                    else if (GameSettings.playerMode == PlayerMode.LocalMultiplayer && p.PlayerNumber - 1 < holeController.raceTees.Count)
                    {
                        tee = holeController.raceTees[p.PlayerNumber - 1];
                    }
                    

					i++;

					if(isRacePlayer) {
						currentPlayerObject = (GameObject)Instantiate (playerOnlinePrefab, tee.position, tee.rotation);
					}
					else 
					{
						currentPlayerObject = (GameObject)Instantiate (playerPrefab, tee.position, tee.rotation);
                        var materialController = currentPlayerObject.GetComponent<CannonMaterialController>();
                        var barrel = GameSettings.CannonBarrelMaterial;
                        if (barrel == null)
                        {
                            var mindex = currentCannonBarrelMaterial - 1;
                            if (mindex < 0 || mindex >= CannonBarrelMaterials.Count)
                            {
                                mindex = 0;
                            }
                            barrel = CannonBarrelMaterials[mindex];
                        }

                        var wheel = GameSettings.CannonWheelMaterial;
                        if (wheel == null)
                        {
                            var mindex = currentCannonWheelMaterial - 1;
                            if (mindex < 0 || mindex >= CannonWheelMaterials.Count)
                            {
                                mindex = 0;
                            }

                            wheel = CannonWheelMaterials[mindex];
                        }

                        materialController.SetMaterial(barrel, wheel);

                    }


                    var pController = currentPlayerObject.GetComponent<CannonPlayerState> ();

	                if (GameSettings.playerMode == PlayerMode.ServerMultiplayer && !string.IsNullOrEmpty(p.playerKey))
	                {
	                    pController.SetPlayerInfo(p.playerKey);
	                }


	                pController.Stroke = 1;
					pController.TotalScore = p.TotalScore;
					pController.playerNumber = p.PlayerNumber;
					pController.isHoleComplete = false;
					pController.isHarzard = false;
					pController.currentBullet = p.CurrentBullet;
					// set the current object to in-active
					currentPlayerObject.SetActive(GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.isRace);
	                // add the game object to the list
					currentPlayerObject.name = string.Format("CannonPlayer{0}", p.PlayerNumber);
					players.Add(currentPlayerObject);

				});

			} else {

                var activePlayers = GameSettings.Room != null ? GameSettings.isRace ? GameSettings.Room.attendees.OrderBy(x => x.playerNumber).ToList() : GameSettings.Room.attendees.OrderBy(x => x.position).ToList() : new List<RoomAttendee>();
                var isOnlineRace = GameSettings.Room != null && activePlayers.Count >= NumberOfPlayers && GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.isRace;
                // create the player and de-activate them within the scene
                for (int i = 0; i < NumberOfPlayers; i++) {
                    
                    var isRacePlayer = false;
                    var playerKey = GameSettings.Room != null && activePlayers.Count >= NumberOfPlayers ? activePlayers[i].UID : string.Empty;
                    isRacePlayer = GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.isRace && !string.IsNullOrEmpty(playerKey)
                        && NetworkClientManager.player != null && NetworkClientManager.player.UID != playerKey;

                    
                    if (GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.isRace && !isRacePlayer)
                    {
                        currentPlayer = i;
                    }

                    GameObject currentPlayerObject;
                    Transform tee = holeController.tee;
                    if (GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.isRace && holeController.raceTees.Count > i)
                    {
                        tee = holeController.raceTees[i];
                    }
                    else if (GameSettings.playerMode == PlayerMode.LocalMultiplayer && i < holeController.raceTees.Count)
                    {
                        tee = holeController.raceTees[i];
                    }


                    if (isRacePlayer)
                    {
                        currentPlayerObject = (GameObject)Instantiate(playerOnlinePrefab, tee.position, tee.rotation);
                    }
                    else
                    {
                        currentPlayerObject = (GameObject)Instantiate(playerPrefab, tee.position, tee.rotation);
                        var materialController = currentPlayerObject.GetComponent<CannonMaterialController>();
                        var barrel = GameSettings.CannonBarrelMaterial;
                        if (barrel == null)
                        {
                            var mindex = currentCannonBarrelMaterial - 1;
                            if (mindex < 0 || mindex >= CannonBarrelMaterials.Count)
                            {
                                mindex = 0;
                            }
                            barrel = CannonBarrelMaterials[mindex];
                        }

                        var wheel = GameSettings.CannonWheelMaterial;
                        if (wheel == null)
                        {
                            var mindex = currentCannonWheelMaterial - 1;
                            if (mindex < 0 || mindex >= CannonWheelMaterials.Count)
                            {
                                mindex = 0;
                            }

                            wheel = CannonWheelMaterials[mindex];
                        }
                        
                        materialController.SetMaterial(barrel, wheel);



                    }

                    var pController = currentPlayerObject.GetComponent<CannonPlayerState>();

                    if (GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.Room != null && activePlayers.Count >= NumberOfPlayers)
                    {
                        pController.SetPlayerInfo(playerKey);
                    }

                    pController.Stroke = 1;
                    pController.TotalScore = 0;
                    pController.playerNumber = i + 1;
                    pController.isHoleComplete = false;
                    pController.isHarzard = false;
                    // set the current object to in-active
                    var isactive = (GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.isRace && !isRacePlayer) || (isRacePlayer && !activePlayers[i].Removed);
                    currentPlayerObject.SetActive(isactive);
                    // add the game object to the list
                    currentPlayerObject.name = string.Format("CannonPlayer{0}", i + 1);
                    players.Add(currentPlayerObject);


                }
				
			}

			if(!(GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.isRace)) {
				currentPlayer = -1;
			}
			MoveToNextPlayer ();
		}
		finally {
			buildingHole = false;
		}
	}

	public GameObject GetCurrentCannonPlayer()
	{
		return player;

	}

	public GameObject GetCurrentBullet(int? playerCurrentBullet = null)
	{
        var index = playerCurrentBullet.HasValue ? playerCurrentBullet.Value : CurrentBullet;
        if (bulletPrefabs != null && bulletPrefabs.Count >= index) {
			var bullet = bulletPrefabs [index - 1];
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

