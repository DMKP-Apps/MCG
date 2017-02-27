using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryController : MonoBehaviour {

	private const string tagBullet = "Bullet";
	private const string tagCamera = "MainCamera";

	private GameController GameController;
	void Start()
	{
		GameController = GameObject.Find ("GameController").GetComponent<GameController> ();
	}

	void OnTriggerEnter(Collider other) {

		if (other.gameObject.tag == tagBullet) {
			GameController.OutOfBounds (other.gameObject.transform.position, other.gameObject);
			Destroy (other.gameObject);
		}
		else if (other.gameObject.tag == tagCamera) {
			var cc = other.gameObject.GetComponent<MainCameraController> ();
			if (cc != null) {
				cc.explorePosition = new Vector3 (0, 0, 0);
				Debug.Log ("Camera out of Bounds");
				cc.Mode = MainCameraController.CameraMode.FollowCannon;
			}
			//
		}
	}
}
