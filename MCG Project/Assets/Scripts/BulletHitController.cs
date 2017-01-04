using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHitController : MonoBehaviour {

	public GameObject splash;
	private GameController GameController;
	private string HitDetected = string.Empty;

	private const string tagWater = "Water";
	private const string tagHole = "Hole";

	private Vector3 lastPosition;
	private Vector3 lastRotation;

	void LateUpdate()
	{
		var destroyObject = false;
		if (!DetectMovement (lastPosition, transform.position) && !DetectMovement (lastRotation, transform.rotation.eulerAngles)) {
			destroyObject = true;
		}
		lastPosition = transform.position;
		lastRotation = transform.rotation.eulerAngles;
		if (destroyObject) {
			Destroy (this.gameObject);
		}

	}

	private bool DetectMovement(Vector3 previous, Vector3 current) {
		var multiplier = 10f;
		var px = float.Parse((System.Math.Floor (previous.x * multiplier) / 10).ToString());
		var py = float.Parse((System.Math.Floor (previous.y * multiplier) / 10).ToString());
		var pz = float.Parse((System.Math.Floor (previous.z * multiplier) / 10).ToString());

		var cx = float.Parse((System.Math.Floor (current.x * multiplier) / 10).ToString());
		var cy = float.Parse((System.Math.Floor (current.y * multiplier) / 10).ToString());
		var cz = float.Parse((System.Math.Floor (current.z * multiplier) / 10).ToString());

		return new Vector3 (px, py, px) != new Vector3 (cx, cy, cx);
	
	}

	void Start()
	{
		GameController = GameObject.Find ("GameController").GetComponent<GameController> ();
	}

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
			if (contact.otherCollider.gameObject.tag == tagHole && HitDetected != tagHole)
            {
				GameController.HoleOver ();
				Destroy(this.gameObject);
				HitDetected = tagHole;
                
            }
			else if (contact.otherCollider.gameObject.tag == tagWater && HitDetected != tagWater )
			{
				HitDetected = tagWater;
				GameController.WaterHazard (contact.otherCollider.gameObject, contact.point);
				this.gameObject.SetActive (false);
				if (splash != null) {
					var ps = (GameObject)Instantiate (splash, new Vector3 (contact.point.x, contact.point.y, contact.point.z), Quaternion.Euler (-90, 0, 0));
					Destroy (ps, 5f);
				}
				Destroy(this.gameObject, 5f);
			}
        }
        

    }
}
