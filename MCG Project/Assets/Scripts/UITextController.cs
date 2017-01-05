using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITextController : MonoBehaviour {

	public Text Hole;
	public Text Par;
	public Text Stroke;
	public Text Player;
	public Text HoleCompleteScore;

	public GameObject alertWaterHazard;
	public GameObject alertOutOfBounds;
	
	public void SetHole(int value) {

		Hole.text = string.Format ("Hole: {0}", value);

	}
	public void SetPar(int value) {

		Par.text = string.Format ("Par: {0}", value);

	}
	public void SetStroke(int value) {

		Stroke.text = string.Format ("Stroke: {0}", value);

	}
	public void SetPlayer(int value) {

		Player.text = string.Format ("Player {0}", value);

	}
	public void SetPlayer(int value, int score) {

		Player.text = string.Format ("Player {0} ({1})", value, score);

	}

	public void SetHoleCompleteScore(int player, string value) {

		HoleCompleteScore.text = string.Format ("Score: {0}", value);

	}

	public void ShowWaterHazard()
	{
		var activateForTime = alertWaterHazard.GetComponent<ActiveForTime> ();
		activateForTime.Show ();

	}

	public void ShowOutOfBounds()
	{
		var activateForTime = alertOutOfBounds.GetComponent<ActiveForTime> ();
		activateForTime.Show ();

	}
}
