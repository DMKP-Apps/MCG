using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class CannonFireController : MonoBehaviour {

	private GameObject bullet;
	public GameObject BulletPosition;
	public GameObject CannonBurstPrefab;
	private GameController GameController;
	public GameObject CannonBulletParent;
    //public GameObject topographicBulletPrefab;

    private bool hasFired = false;

	private Vector3 LastBulletTransform;
    private CannonPlayerState cannonPlayerState = null;
    public CannonBodyAnimate cannonBodyAnimate;

	private LineRenderer[] lines;

    void Start() {
		GameController = GameObject.Find ("GameController").GetComponent<GameController> ();
        cannonPlayerState = this.GetComponent<CannonPlayerState>();
		lines = this.gameObject.GetComponentsInChildren<LineRenderer> ();
        if (lines != null)
        {
            lines.ToList().ForEach(line => line.gameObject.SetActive(false));
        }
        yrdsText.SetActive(false);
    }

	private Vector3 previousBulletFirePosition;
    private int previousBulletIndex = -1;
    private float previousShotPower = 0;
    private bool previousHasShotPower = false;

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

        var shotPower = GameSettings.ShotPower.HasValue ? GameSettings.ShotPower.Value : 1;

        

        if (cannonPlayerState != null && !cannonPlayerState.isOnlinePlayer && !IsFiring() && lines != null && (previousHasShotPower != GameSettings.ShotPower.HasValue || previousShotPower != shotPower || previousBulletFirePosition != BulletPosition.transform.position || previousBulletIndex != GameController.CurrentBullet)) {

            yrdsText.SetActive(true);

            previousBulletIndex = GameController.CurrentBullet;
            previousBulletFirePosition = BulletPosition.transform.position;
            previousShotPower = shotPower;
            previousHasShotPower = GameSettings.ShotPower.HasValue;

            lines.ToList().ForEach(line => {
                if (!line.gameObject.activeInHierarchy && GameSettings.ShotPower.HasValue)
                {
                    line.gameObject.SetActive(true);
                }
                else if (!line.gameObject.activeInHierarchy && line.gameObject.name.Contains("Topographic"))
                {
                    line.gameObject.SetActive(true);
                }
            });
            
			//Debug.DrawLine (transform.position, GameController.GetHolePinPosition ());
			var bulletData = GameController.GetCurrentBullet ().GetComponent<BulletData> ();
			Vector3 startVelocity = BulletPosition.transform.forward * ((bulletData.Power + bulletData.BasePower) * shotPower);
			//Debug.Log(PlotTrajectoryAtTime(BulletPosition.transform.position, startVelocity, Time.deltaTime));
			PlotTrajectory (BulletPosition.transform.position, startVelocity, Time.deltaTime, 20);
		} else if (lines != null && (bullet != null || IsFiring()) && lines.Select(x => x.gameObject.activeInHierarchy).Any(x => x)) {
            lines.ToList().ForEach(line => {
                line.gameObject.SetActive(false);
            });
            
            previousBulletIndex = -1;
            previousShotPower = 0;
            previousHasShotPower = false;
        }
	}

	public Vector3 PlotTrajectoryAtTime (Vector3 start, Vector3 startVelocity, float time) {
		return start + startVelocity*time + Physics.gravity*time*time*0.5f;
	}

    public GameObject yrdsText;
    public TextMesh yrdsDistanceText;

    public void PlotTrajectory (Vector3 start, Vector3 startVelocity, float timestep, float maxTime) {
		Vector3 prev = start;
		List<Vector3> positions = new List<Vector3> () { start };
		for (int i=1;;i++) {
			float t = timestep*i;
			if (t > maxTime) break;
			Vector3 pos = PlotTrajectoryAtTime (start, startVelocity, t);
            RaycastHit hitInfo;
            //Physics.Linecast(prev, pos, out hitInfo);

            //RaycastHit[] hit;
            //hit = Physics.RaycastAll(prev, pos);

            //if (hit != null && hit.Any(h => !h.collider.isTrigger))
            //{
            //    break;
            //}

            if (Physics.Linecast(prev, pos, out hitInfo))
            {
                if(!hitInfo.collider.isTrigger) break;
            }
            Debug.DrawLine (prev,pos,Color.red);

			//line.SetPosition (i, pos);
			positions.Add (pos);

			//Vector.DrawLine (prev,pos,Color.red);
			prev = pos;
		}
        lines.ToList().ForEach(line => {
            line.numPositions = positions.Count;
            int index = 0;
            positions.ForEach(x => line.SetPosition(index++, x));
            //line.SetPositions (positions.ToArray());
            if (line.gameObject.name.Contains("Topographic"))
            {
                line.endWidth = 20f;
                line.startWidth = 10f;
            }
            else {
                line.endWidth = 5f;
                line.startWidth = 1f;
            }
            
        });

        if (positions.Count > 0)
        {
            GameSettings.CurrentCannonRotation = transform.rotation.eulerAngles;
            GameSettings.CurrentCannonLocation = positions[0];
            GameSettings.EstimatedShotLocation = positions[positions.Count - 1];
            GameSettings.EstimatedShotHighPoint = positions.OrderByDescending(x => x.y).FirstOrDefault();

            yrdsText.transform.position = GameSettings.EstimatedShotLocation;

            var distance = Vector3.Distance(GameSettings.CurrentCannonLocation, GameSettings.EstimatedShotLocation);
            yrdsDistanceText.text = System.Math.Ceiling(System.Convert.ToDouble(distance) * 1.0936).ToString();

            //EstimatedShotHighPoint
        }
        //line.SetVertexCount (positions.Count);
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

        var torque = 0f; //UnityEngine.Random.Range(-20.0f, 20.0f);
        var turn = 0f; //UnityEngine.Random.Range(-20.0f, 20.0f);
		var power = bulletData.BasePower + (bulletData.Power * powerRate);

        if (cannonPlayerState != null)
        {
			cannonPlayerState.SendNetworkObjectData(true, power, accuracy, torque, turn, waitForFire);
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

            if (bullet.GetComponent<AssociatedPlayerState>() == null)
            {
                bullet.AddComponent(typeof(AssociatedPlayerState));
            }
            bullet.GetComponent<AssociatedPlayerState>().playerState = this.GetComponent<CannonPlayerState>();

            //topographicBulletPrefab
            /*var topoBullet = (GameObject)Instantiate(
                            topographicBulletPrefab, BulletPosition.transform.position, Quaternion.Euler(new Vector3(90f, BulletPosition.transform.rotation.eulerAngles.y, 0f)));//new Vector3(transform.position.x, transform.position.y, transform.position.z + 10),
            var topoController = topoBullet.GetComponent<TopographicBulletController>();
            if (topoController != null) {
                topoController.target = bullet;
            }*/

            ////bullet.transform.parent = CannonBulletParent.transform;

            var burst = (GameObject)Instantiate(
                CannonBurstPrefab, BulletPosition.transform.position, BulletPosition.transform.rotation);//new Vector3(transform.position.x, transform.position.y, transform.position.z + 10),

            var bulletData = bullet.GetComponent<BulletData>();
            GameController.powerRate = power / bulletData.Power;

            bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * power;
            //bullet.GetComponent<Rigidbody>().AddTorque(transform.up * torque);
            //bullet.GetComponent<Rigidbody>().AddTorque(transform.right * turn);

			var constantForce = bullet.GetComponent<ConstantForce>();
            //bullet.GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(bulletData.MaxTorque * accuracy, bulletData.MaxTorque * accuracy, 0f), ForceMode.VelocityChange);
            //bulletRigidbody.AddRelativeTorque(transform.right * (input.y * 100), ForceMode.VelocityChange);
            //bullet.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(accuracy * bulletData.MaxAccuracy, 0f, accuracy > 0 ? (accuracy * bulletData.MaxAccuracy) * 0.5f * -1 : (accuracy * bulletData.MaxAccuracy) * 0.5f), ForceMode.VelocityChange);//* Time.deltaTime

            if (constantForce != null) {
				constantForce.relativeForce = new Vector3(accuracy * bulletData.MaxAccuracy,0f, accuracy > 0 ? (accuracy * bulletData.MaxAccuracy) * 0.5f * -1 : (accuracy * bulletData.MaxAccuracy) * 0.5f);
				constantForce.relativeTorque = new Vector3(bulletData.MaxTorque * accuracy, bulletData.MaxTorque * accuracy,0f);

			}

            Destroy(bullet, 30.0f);
            Destroy(burst, 5.0f);

            hasFired = true;
            //////var cannonPlayer = this.GetComponent<CannonPlayerState>();
            //////if (cannonPlayerState != null)
            //////{
            //////    if (cannonPlayerState.MoveToParent != null) {
            //////        cannonPlayerState.MoveToParent = null;
            //////        cannonPlayerState.transform.parent = null;
            //////    }
            //////}

            GameController.Log("Power Rate: {0}; Velocity: {1}", GameController.powerRate, power);
        }
        finally {
            // destroy fireData
            fireData = null;
        }

    }
}
