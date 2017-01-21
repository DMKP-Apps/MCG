using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CannonFireController : MonoBehaviour {

	private GameObject bullet;
	public GameObject BulletPosition;
	public GameObject CannonBurstPrefab;
	private GameController GameController;
	public GameObject CannonBulletParent;

    
	private bool hasFired = false;

	private Vector3 LastBulletTransform;
    private CannonPlayerState cannonPlayerState = null;
    public CannonBodyAnimate cannonBodyAnimate;


    void Start() {
		GameController = GameObject.Find ("GameController").GetComponent<GameController> ();
        cannonPlayerState = this.GetComponent<CannonPlayerState>();
    }

	void Update()
	{
        if (fireData != null && fireData.canFire()) {
            OnFire();
        }

		if (bullet != null) {
			LastBulletTransform = bullet.transform.position;
		} else if (bullet == null && hasFired) {
			if (!(GameSettings.playerMode == PlayerMode.ServerMultiplayer && GameSettings.isRace && cannonPlayerState.isOnlinePlayer)) {
				GameController.StrokeComplete (LastBulletTransform);
			}
			hasFired = false;
		}
	}

	public GameObject GetBullet() {
		return bullet;
	}

    public bool IsFiring()
    {
        return fireData != null || GetBullet() != null;
    }


    public Vector3 GetBulletTransform() {

		return LastBulletTransform;

	}

	public void AimCannon(Vector3 position) {
	
		//CannonBulletParent.transform.forward = position;
		CannonBulletParent.transform.localRotation = Quaternion.Euler (new Vector3 (0, 0, 0));
		var inputController = CannonBulletParent.GetComponent<InputController> ();
		if (inputController != null) { 
			inputController.Reset ();
		}
		//CannonBulletParent.transform.LookAt (position);
	
	}

    public double waitForFire = 1000;

    //private DateTime _waitFireStartTime = DateTime.MaxValue;
    class FireData
    {
        public DateTime waitStartTime = System.DateTime.Now;
        public float torque;
        public float turn;
        public float power;
		public float accuracy;
        public GameObject bullet;
        public double waitForFireMilliseconds = 1000;
        public bool canFire()
        {
            return DateTime.Now.Subtract(waitStartTime).TotalMilliseconds > this.waitForFireMilliseconds;
        }
    }

    private FireData fireData = null;

	public void Fire(float powerRate, float accuracy) {

        fireData = null;

        GameController.powerRate = powerRate;
        GameObject bulletPrefab = GameController.GetCurrentBullet();
        var bulletData = bulletPrefab.GetComponent<BulletData>();

        var torque = UnityEngine.Random.Range(-20.0f, 20.0f);
        var turn = UnityEngine.Random.Range(-20.0f, 20.0f);
		var power = bulletData.BasePower + (bulletData.Power * powerRate);

        if (cannonPlayerState != null)
        {
			cannonPlayerState.SendNetworkObjectData(true, power, torque, turn, waitForFire);
        }

		Fire(power, accuracy, torque, turn, waitForFire);

    }

	public void Fire(float power, float accuracy, float torque, float turn, double waitTime, int? currentBullet = null)
    {
        if (cannonBodyAnimate != null) {
            cannonBodyAnimate.PlayAnimation();
        }
		if (cannonPlayerState.isOnlinePlayer) {
			var system = cannonPlayerState.Spark.GetComponent<ParticleSystem>();
			system.Play();
		}
        fireData = new FireData()
        {
            torque = torque,
            turn = turn,
            power = power,
            waitForFireMilliseconds = waitTime,
			accuracy = accuracy,
            bullet = GameController.GetCurrentBullet(currentBullet)
        };
    }

    private void OnFire()
    {
        try
        {
            
            var system = cannonPlayerState.Spark.GetComponent<ParticleSystem>();
            system.Stop();

            if (cannonBodyAnimate != null)
            {
                cannonBodyAnimate.StopAnimation();
            }

            if (fireData == null)
            {
                return;
            }

            float power = fireData.power;
            float torque = fireData.torque;
            float turn = fireData.turn;
			float accuracy = fireData.accuracy;
            GameObject currentBullet = fireData.bullet;

            // destroy fireData
            fireData = null;

            bullet = (GameObject)Instantiate(
                currentBullet, BulletPosition.transform.position, BulletPosition.transform.rotation);//new Vector3(transform.position.x, transform.position.y, transform.position.z + 10),

            bullet.transform.parent = CannonBulletParent.transform;

            var burst = (GameObject)Instantiate(
                CannonBurstPrefab, BulletPosition.transform.position, BulletPosition.transform.rotation);//new Vector3(transform.position.x, transform.position.y, transform.position.z + 10),

            var bulletData = bullet.GetComponent<BulletData>();
            GameController.powerRate = power / bulletData.Power;

            bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * power;
            bullet.GetComponent<Rigidbody>().AddTorque(transform.up * torque);
            bullet.GetComponent<Rigidbody>().AddTorque(transform.right * turn);

			var constantForce = bullet.GetComponent<ConstantForce>();
			if(constantForce != null) {
				constantForce.relativeForce = new Vector3(accuracy * bulletData.MaxAccuracy,0f,0f);
				constantForce.relativeTorque = new Vector3(bulletData.MaxTorque * accuracy, bulletData.MaxTorque * accuracy,0f);

			}

            Destroy(bullet, 30.0f);
            Destroy(burst, 5.0f);

            hasFired = true;

            GameController.Log("Power Rate: {0}; Velocity: {1}", GameController.powerRate, power);
        }
        finally {
            // destroy fireData
            fireData = null;
        }

    }
}
