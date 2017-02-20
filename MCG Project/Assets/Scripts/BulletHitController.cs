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

    private Vector3? _addSpin = null;
    private Vector3 _forward;
    private Vector3 _right;

    public void AddSpin(Vector2 input)
    {
        if (_addSpin.HasValue || bulletRigidbody == null)
        {
            return;
        }
        bulletRigidbody.maxAngularVelocity = 18;
        bulletRigidbody.AddRelativeTorque(new Vector3(input.y * 2f, input.x * 2f, 0f), ForceMode.VelocityChange);
        //input.y = 0;
        //_addSpin = input;
        if (input.y > 0) {
            input.y = 1;
        }
        var y = input.y / 8.5f;
        var x = input.x / 50;

        //if (y < 0)
        //{
        //    y *= -1;
        //}
        //if (x < 0)
        //{
        //    x *= -1;
        //}
        //if (y > x)
        //{
        //    y = input.y / 8.5f;
        //    x = input.x / 50;
        //}
        //else {
        //    y = input.y / 8;
        //    x = input.x / 50;
        //}
        _addSpin = _forward * (y) + _right * (x);// new Vector3(input.x, 0f, input.y);
        //bulletRigidbody.AddRelativeForce(force, ForceMode.Acceleration);
        //bulletRigidbody.AddRelativeTorque(Vector3.forward * input.y * 10);
        //bulletRigidbody..Rotate(new Vector3(input.y * 1000, input.x * 1000 * -1));
        //bulletRigidbody.AddRelativeTorque(new Vector3(input.y * 1000, input.x * 1000 * -1), ForceMode.VelocityChange);
        //bulletRigidbody.AddRelativeTorque(transform.right * (input.y * 100), ForceMode.VelocityChange);
        // bulletRigidbody.AddRelativeForce(new Vector3(input.x / 10 * Time.deltaTime, 0f, -0.5f), ForceMode.VelocityChange);//* Time.deltaTime

        //new Vector3(accuracy * bulletData.MaxAccuracy, 0f, accuracy > 0 ? (accuracy * bulletData.MaxAccuracy) * 0.5f * -1 : (accuracy * bulletData.MaxAccuracy) * 0.5f)
        Debug.Log(string.Format("touch: {0}, {1}, {2}", input.x, input.y, bulletRigidbody.velocity.sqrMagnitude));
    }

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
        _forward = transform.forward;
        _right = transform.right;


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

        if (collidingIds.Count > 0 && bulletRigidbody.maxAngularVelocity > 7)
        {
            bulletRigidbody.maxAngularVelocity = 7;
            if (_addSpin.HasValue)
            {
               bulletRigidbody.AddForce(_addSpin.Value, ForceMode.VelocityChange);
                Debug.Log("");
                Debug.Log(string.Format("ADD FORCE: {0}, {1}, {2}", _addSpin.Value.x, _addSpin.Value.y, _addSpin.Value.z));
            }
        }

        
        
        

    }
    void OnCollisionExit(Collision collision)
    {
        collidingIds = collision.contacts.Select(x => x.otherCollider.gameObject.GetInstanceID()).ToList();
        

    }
}
