using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BulletHitController : MonoBehaviour {

	public GameObject splash;
	private GameController GameController;
	private string HitDetected = string.Empty;

	private const string tagWater = "Water";
	private const string tagHole = "Hole";
    private const string tagCollectableItem = "CollectableItem";



    private System.DateTime time = System.DateTime.Now;

    private Rigidbody bulletRigidbody;
    float angularDrag = 0.05f;

    public float velocitySqrMagThreshold = 1f;
    public float maxDrag = 10f;
    void Start()
    {
        bulletRigidbody = transform.GetComponent<Rigidbody>();
        GameController = GameObject.Find("GameController").GetComponent<GameController>();
        angularDrag = bulletRigidbody.angularDrag;
    }

	void LateUpdate()
	{
        if (System.DateTime.Now.Subtract (time).TotalMilliseconds > 500) {

        	time = System.DateTime.Now;
        
             if (bulletRigidbody != null && bulletRigidbody.velocity.magnitude < 0.3f)
            {
                
                Destroy(this.gameObject);
            }


		}
	}

    void Update()
    {
        float rBVelocitySqrMag = bulletRigidbody.velocity.sqrMagnitude;
        bool canActivateDragDamping = false;

        //Vector3.

        //Debug.Log(string.Join(",", collidingIds.Select(x => x.ToString()).ToArray()));

        if (rBVelocitySqrMag < velocitySqrMagThreshold)
        {
            canActivateDragDamping = true;
        }
        else
        {
            canActivateDragDamping = false;
        }

        if (canActivateDragDamping && collidingIds.Count > 0)
        {
            var veldiff = (velocitySqrMagThreshold - rBVelocitySqrMag) / velocitySqrMagThreshold;

            bulletRigidbody.drag = maxDrag * veldiff;
            bulletRigidbody.angularDrag = maxDrag;

            var constantForce = this.GetComponent<ConstantForce>();
            if (constantForce != null)
            {
                constantForce.relativeForce = new Vector3(0, 0, 0);
                constantForce.relativeTorque = new Vector3(0, 0, 0);
            }
        }
        else
        {
            bulletRigidbody.drag = 0;
            bulletRigidbody.angularDrag = angularDrag;
        }
    }


    public List<int> collidingIds = new List<int>();

    void OnCollisionEnter(Collision collision)
    {
        collidingIds = collision.contacts.Where(x => x.otherCollider.tag != tagCollectableItem).Select(x => x.otherCollider.gameObject.GetInstanceID()).ToList();
        
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
    void OnCollisionExit(Collision collision)
    {
        collidingIds = collision.contacts.Select(x => x.otherCollider.gameObject.GetInstanceID()).ToList();
        

    }
}
