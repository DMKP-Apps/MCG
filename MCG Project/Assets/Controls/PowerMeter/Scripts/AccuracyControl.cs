using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AccuracyControl : MonoBehaviour {

	public List<GameObject> percentObjects;
	public PowerControl powerControl;
	private PowerControl.MeterState state;
	public float accuracy = 0f;

	private int currentIndex = 0;
	private int previousIndex = -1;

	void Start() {
		StopMeter ();
	}

	public void StartMeter() {
		powerControl.gameObject.SetActive (true);
		powerControl.ResetMeter ();
		powerControl.StartMeter ();
	}

	public void StopMeter() {
		powerControl.gameObject.SetActive (false);
		powerControl.StopMeter ();
	}

	public void ResetMeter() {
		powerControl.ResetMeter ();
	}

	// Update is called once per frame
	void Update () {

		if (powerControl.state == PowerControl.MeterState.Running) {
			state = PowerControl.MeterState.Running;
			if (powerControl.PowerRating > 0.79f) {
				currentIndex = 4;
			} else if (powerControl.PowerRating > 0.59f) {
				currentIndex = 3;
			} else if (powerControl.PowerRating > 0.39f) {
				currentIndex = 2;
			} else if (powerControl.PowerRating > 0.19f) {
				currentIndex = 1;
			}else {
				currentIndex = 0;
			}

			if (currentIndex != previousIndex) {
				if (previousIndex > -1) {
					percentObjects [previousIndex].SetActive (false);
				}
				previousIndex = currentIndex;
				percentObjects [previousIndex].SetActive (true);

			}

			if (currentIndex > 2) {
				accuracy = powerControl.PowerRating - 0.5f;
			} else if (currentIndex < 2) {
				accuracy = (0.5f - powerControl.PowerRating) * -1;
			} else {
				accuracy = 0f;
			}

		} else if(state == PowerControl.MeterState.Running) {
			// hide all percent controls
			percentObjects.ForEach(x => x.SetActive(false));
			state = PowerControl.MeterState.Paused;
		}

	}
}
