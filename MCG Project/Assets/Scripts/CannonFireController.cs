﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonFireController : MonoBehaviour {

	private GameObject bullet;
	public GameObject BulletPosition;
	public GameObject CannonBurstPrefab;
	public GameController GameController;

	private bool hasFired = false;

	private Vector3 LastBulletTransform;

	void Start() {
		GameController = GameObject.Find ("GameController").GetComponent<GameController> ();
	}

	void Update()
	{
		if (bullet != null) {
			LastBulletTransform = bullet.transform.position;
		} else if (bullet == null && hasFired) {
			GameController.StrokeComplete (LastBulletTransform);
			hasFired = false;
		}
	}

	public GameObject GetBullet() {
	
		return bullet;
	
	}

	public Vector3 GetBulletTransform() {

		return LastBulletTransform;

	}

	public void Fire(GameObject bulletPrefab, float powerRate) {
	
		bullet = (GameObject)Instantiate(
			bulletPrefab, BulletPosition.transform.position, BulletPosition.transform.rotation);//new Vector3(transform.position.x, transform.position.y, transform.position.z + 10),

		bullet.transform.parent = this.transform;
		//bullet.transform.localRotation = bulletPrefab.transform.localRotation;
		//bullet.GetComponent<Rigidbody>().rotation = bulletPrefab.transform.localRotation;
		var burst = (GameObject)Instantiate(
			CannonBurstPrefab, BulletPosition.transform.position, BulletPosition.transform.rotation);//new Vector3(transform.position.x, transform.position.y, transform.position.z + 10),



		var bulletData = bullet.GetComponent<BulletData> ();

		var torque = Random.Range (-20.0f, 20.0f);
		var turn = Random.Range (-20.0f, 20.0f);

		bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletData.Power * powerRate, ForceMode.Impulse);
		bullet.GetComponent<Rigidbody>().AddTorque(transform.up * torque * turn);
		bullet.GetComponent<Rigidbody>().AddTorque(transform.right * torque * turn);

		Destroy(bullet, 10.0f);
		Destroy(burst, 5.0f);

		hasFired = true;
	
	}
}
