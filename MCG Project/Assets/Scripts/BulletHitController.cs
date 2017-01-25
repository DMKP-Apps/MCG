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

	//private System.DateTime time = System.DateTime.Now;

    private Rigidbody bulletRigidbody;
    void Start()
    {
        bulletRigidbody = transform.GetComponent<Rigidbody>();
        GameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

	void LateUpdate()
	{
        //if (System.DateTime.Now.Subtract (time).TotalMilliseconds > 300) {

        //	time = System.DateTime.Now;
        Debug.Log(bulletRigidbody.velocity.magnitude);
        if (bulletRigidbody != null && bulletRigidbody.velocity.magnitude < 0.5f)
            {
                
                Destroy(this.gameObject);
            }

			/*var destroyObject = false;
			if (!DetectMovement (lastPosition, transform.position) && !DetectMovement (lastRotation, transform.rotation.eulerAngles)) {
				destroyObject = true;
			}
			lastPosition = transform.position;
			lastRotation = transform.rotation.eulerAngles;
			if (destroyObject) {
				Destroy (this.gameObject);
			}*/
		//}
	}

	/*private bool DetectMovement(Vector3 previous, Vector3 current) {
		var multiplier = 10f;
		var px = float.Parse((System.Math.Floor (previous.x * multiplier) / multiplier).ToString());
		var py = float.Parse((System.Math.Floor (previous.y * multiplier) / multiplier).ToString());
		var pz = float.Parse((System.Math.Floor (previous.z * multiplier) / multiplier).ToString());

		var cx = float.Parse((System.Math.Floor (current.x * multiplier) / multiplier).ToString());
		var cy = float.Parse((System.Math.Floor (current.y * multiplier) / multiplier).ToString());
		var cz = float.Parse((System.Math.Floor (current.z * multiplier) / multiplier).ToString());

		var distance = Vector3.Distance (new Vector3 (px, py, px), new Vector3 (cx, cy, cx));

		return !(distance < 1f);
		//Debug.Log(string.Format("Distance: {0}", distance));

		//return new Vector3 (px, py, px) != new Vector3 (cx, cy, cx);
	
	}*/


    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
			if (contact.otherCollider.gameObject.tag == tagHole && HitDetected != tagHole)
            {
				GameController.HoleOver (this.gameObject);
				Destroy(this.gameObject);
				HitDetected = tagHole;
                
            }
			else if (contact.otherCollider.gameObject.tag == tagWater && HitDetected != tagWater )
			{
				HitDetected = tagWater;
				GameController.WaterHazard (contact.otherCollider.gameObject, contact.point, this.gameObject);
				this.gameObject.SetActive (false);
				if (splash != null) {
					var ps = (GameObject)Instantiate (splash, new Vector3 (contact.point.x, contact.point.y, contact.point.z), Quaternion.Euler (-90, 0, 0));
					Destroy (ps, 3f);
				}
				Destroy(this.gameObject, 3f);
			}
        }
        

    }
}
