using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHitController : MonoBehaviour {

	public GameObject splash;
	private GameController GameController;
	private string HitDetected = string.Empty;

	private const string tagWater = "Water";
	private const string tagHole = "Hole";

	void Start()
	{
		GameController = GameObject.Find ("GameController").GetComponent<GameController> ();
	}

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
			if (contact.otherCollider.gameObject.tag == tagHole)
            {
                //GameController.Reset ();
				Destroy(this.gameObject, 5f);
                
            }
			else if (contact.otherCollider.gameObject.tag == tagWater && HitDetected != tagWater )
			{
				HitDetected = tagWater;
				var ps = (GameObject)Instantiate(splash, new Vector3(contact.point.x, contact.point.y, contact.point.z), Quaternion.Euler(-90,0,0));
				//GameController.WaterHazard (contact.otherCollider.gameObject, contact.point);
				this.gameObject.SetActive (false);
				Destroy(ps, 5f);
			}
        }
        

    }
}
