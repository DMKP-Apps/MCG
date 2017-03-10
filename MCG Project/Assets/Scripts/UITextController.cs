using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITextController : MonoBehaviour {

	public PlayerInfo playerInfo;
	public Text HoleCompleteScore;
	//public Text OutputLog;

	public GameObject alertWaterHazard;
	public GameObject alertOutOfBounds;
    public GameObject alertUnplayable;

    public void SetHole(int value) {

		playerInfo.HoleNumber = value;


	}
	public void SetPar(int value) {
		playerInfo.Par = value;


	}
	public void SetStroke(int value) {
		playerInfo.Stroke = value;

	}
	public void SetPlayer(int value) {
		SetPlayer (value, 0);
	}

	public void SetPlayer(int value, int score) {
		playerInfo.PlayerIndex = value - 1;
		playerInfo.Score = score;


	}

	public void Log(string output) {

		//OutputLog.text = output;

	}

	public void Log(string output, params object[] args) {

		//OutputLog.text = string.Format(output, args);

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

    public void UnplayableHazard()
    {
        if (alertUnplayable == null)
        {
            return;
        }
        var activateForTime = alertUnplayable.GetComponent<ActiveForTime>();
        if (activateForTime != null)
        {
            activateForTime.Show();
        }

    }
}
