﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuageController : MonoBehaviour {

	public Sprite Ready;
	public Sprite Set;
	public Sprite Fire;
	public Image Image;
	public PowerControl Meter;
	public AccuracyControl Accuracy;

	// Use this for initialization
	void Start () {
		Accuracy.StopMeter ();
		Accuracy.ResetMeter ();
		Accuracy.gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnReady()
	{
		Image.sprite = Ready;
		Meter.ResetMeter ();
		Accuracy.StopMeter ();
		Accuracy.ResetMeter ();
		Accuracy.gameObject.SetActive (false);
		GameSettings.ShotPower = null;

	}
	public void OnSet()
	{
		Image.sprite = Set;
		//Meter.SetActive (true);

	}

	public void OnFire()
	{

		Image.sprite = Fire;
		Accuracy.gameObject.SetActive (true);
		Accuracy.StartMeter ();
		GameSettings.ShotPower = Meter.PowerRating;

	}
		

}
