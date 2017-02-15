﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CannonPlayerState : MonoBehaviour {

	public GameObject Spark;

	public int Stroke = 1;
	public int TotalScore = 0;
	public int playerNumber = 0;
	public bool isHoleComplete = false;
	public bool isHarzard = false;
	public int currentBullet = 1;
	private GameObject CannonBarrow;

    public int updateServerMilliseconds = 1000;
    private GameController GameController;
	private DateTime lastUpdateTime = DateTime.Now.AddMilliseconds(1500);

    public bool isOnlinePlayer = false;
    public string playerKey = string.Empty;
    private CannonFireController cannonFireController;

    public Transform MoveToParent = null;

    private Guid playerId = Guid.NewGuid();
    public Guid GetPlayerId()
    {
        return playerId;
    }

    private TestController testController;
    private double elapsedTime = 0;
    private DateTime? elapsedTimeOffset = null;

    void Start()
	{
		CannonBarrow = transform.FindChild ("Cannon").gameObject;
        GameController = GameObject.Find("GameController").GetComponent<GameController>();
        cannonFireController = this.GetComponent<CannonFireController>();
        testController = GameObject.FindObjectOfType<TestController>();
    }

    public void SetPlayerActive(bool isActive)
    {
        if (isActive && elapsedTimeOffset.HasValue)
        {
            elapsedTime += DateTime.Now.Subtract(elapsedTimeOffset.Value).TotalMilliseconds;
        }
        else if (!isActive)
        {
            elapsedTimeOffset = DateTime.Now;
        }

    }

    public bool isFiring()
    {
        return cannonFireController.IsFiring();
    }

    public void SetPlayerInfo(string key) {
        cannonFireController = this.GetComponent<CannonFireController>();
        CannonBarrow = transform.FindChild("Cannon").gameObject;
        playerKey = key;
        if (NetworkClientManager.player != null)
        {
            isOnlinePlayer = playerKey != NetworkClientManager.player.UID;
        }

        if (isOnlinePlayer)
        {
            //var rigidBody = this.GetComponent<Rigidbody>();
            //if (!rigidBody.isKinematic)
            //{
            //    rigidBody.isKinematic = true;
            //}

            var inputController = CannonBarrow.GetComponent<InputController>();
			if (inputController != null) {
				inputController.AllowInput = false;
			}
        }
    }

	public enum State
	{
		None,
		Ready,
		Set,
		Fire
	}

	private State _state = State.None;

	public State CurrentState {
		get { 
			return _state;
		}
		set { 
			_state = value;
			if (_state == State.Set) {
				var system = Spark.GetComponent<ParticleSystem> ();
				system.Play ();
			} /*else {
				var system = Spark.GetComponent<ParticleSystem>();
				system.Stop ();


			}*/
		}
	}

	public void ResetBarrowPosition()
	{
		CannonBarrow.transform.localRotation = Quaternion.Euler(new Vector3 (0, 0, 0));
	}

    private NetworkObjectData _previousObjectData = new NetworkObjectData();

	public void SendNetworkObjectData(bool fired = false, float power = 0f, float accuracy = 0f, float torque = 0f, float turn = 0f, double wait = 0f) {


        NetworkObjectData data = new NetworkObjectData()
        {
            uniqueId = Guid.NewGuid().ToString(),
            objectName = gameObject.name,
            fire = fired,
            holeId = GameController.GetHoleId(),
            root_position_x = transform.position.x,
            root_position_y = transform.position.y,
            root_position_z = transform.position.z,
            root_rotation_x = transform.rotation.eulerAngles.x,
            root_rotation_y = transform.rotation.eulerAngles.y,
            root_rotation_z = transform.rotation.eulerAngles.z,
            cannon_position_x = CannonBarrow.transform.position.x,
            cannon_position_y = CannonBarrow.transform.position.y,
            cannon_position_z = CannonBarrow.transform.position.z,
            cannon_rotation_x = CannonBarrow.transform.localRotation.eulerAngles.x,
            cannon_rotation_y = CannonBarrow.transform.localRotation.eulerAngles.y,
            cannon_rotation_z = CannonBarrow.transform.localRotation.eulerAngles.z,
            currentBullet = currentBullet,
            fire_power = power,
            fire_torque = torque,
            fire_turn = turn,
            fire_accurracy = accuracy,
            stroke = Stroke,
            holeComplete = isHoleComplete,
            waitMilliseconds = wait,
            timeStamp = DateTime.Now.AddMilliseconds(elapsedTime * -1)
        };

        if (testController != null && !data.Compare(_previousObjectData)) {
            testController.logPlayerData(this, data);
        }

        if (data.holeComplete)
        {
            elapsedTime = 0;
        }

        if (!NetworkClientManager.IsOnline)
        {
            return;
        }

        if (!data.Compare(_previousObjectData))
        {
            _previousObjectData = data;
            Debug.Log("Send object changes");
            NetworkClientManager.SendGameObjectData(data, this);
        }
    }

    class SmoothLookAtRotation
    {
        public Vector3 Destination;       
    }

    SmoothLookAtRotation _rotateDirection;

    public void OnUpdateRemoteData(NetworkObjectData objectData)
    {
		if (objectData == null) {
			//GameController.EndGame ();
			return;
		
		}
			
        // new data
        //Debug.Log(objectData);
        gameObject.transform.position = new Vector3(objectData.root_position_x, objectData.root_position_y, objectData.root_position_z);
        //gameObject.transform.rotation = Quaternion.Euler(new Vector3(objectData.root_rotation_x, objectData.root_rotation_y, objectData.root_rotation_z));
        gameObject.transform.eulerAngles = new Vector3(objectData.root_rotation_x, objectData.root_rotation_y, objectData.root_rotation_z);

        if (_rotateDirection == null)
        {
            _rotateDirection = new SmoothLookAtRotation();
        }

        //rotate us over time according to speed until we are in the required rotation
        _rotateDirection.Destination = new Vector3(objectData.cannon_rotation_x, objectData.cannon_rotation_y, objectData.cannon_rotation_z);

        currentBullet = objectData.currentBullet;

        if (objectData.fire)
        {
            CannonBarrow.transform.localEulerAngles = new Vector3(objectData.cannon_rotation_x, objectData.cannon_rotation_y, objectData.cannon_rotation_z);
            cannonFireController.Fire(objectData.fire_power, objectData.fire_accurracy, objectData.fire_torque, objectData.fire_turn, objectData.waitMilliseconds, objectData.currentBullet);

        }

        if (objectData.holeComplete)
        {
            //gameObject.SetActive(false);
        }
        


    }

    // Update is called once per frame
    void Update () {
        //if (!NetworkClientManager.IsOnline)
        //{
        //   return;
        //}	

        if (!this.gameObject.activeInHierarchy) {
            return;
        }

        if (_rotateDirection != null)
        {
            var quat = Quaternion.Euler(_rotateDirection.Destination);
            CannonBarrow.transform.localRotation = Quaternion.Slerp(CannonBarrow.transform.localRotation, quat, Time.deltaTime * 8);
        }

        if (isOnlinePlayer)
		{
			if (GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.GameInfo != null && GameSettings.GameInfo.Items != null) {
				var data = GameSettings.GameInfo.Items.FirstOrDefault (x => x.objectId == playerKey);
				if (data != null && data.uniqueId != _previousObjectData.uniqueId) {
					_previousObjectData = data;
					OnUpdateRemoteData (data);
				}
			}
		}

		if (cannonFireController.IsFiring()) {
			return;
		}

        if (DateTime.Now.Subtract(lastUpdateTime).TotalMilliseconds > updateServerMilliseconds) {
            lastUpdateTime = DateTime.Now;

            if (!isOnlinePlayer)
            {
                // set the game object information
                SendNetworkObjectData();
            }
            
        }
	}


}
