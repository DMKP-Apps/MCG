using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInputController : MonoBehaviour {

	public PowerControl powerControl;
	private GameController GameController;
	public GuageController GuageController;


	
	// Update is called once per frame
	void Update () {
		GameController = GameObject.Find("GameController").GetComponent<GameController>();
	}

	public void OnPowerButtonClick()
	{
		var currentCannon = GameController.GetCurrentCannonPlayer ();
		var currentCannonState = currentCannon.GetComponent<CannonPlayerState> ();

		if (currentCannonState.CurrentState == CannonPlayerState.State.None) {
		
			currentCannonState.CurrentState = CannonPlayerState.State.Ready;
			GuageController.OnSet ();
			powerControl.StartMeter ();
		
		}
		else if (currentCannonState.CurrentState == CannonPlayerState.State.Ready) {

			currentCannonState.CurrentState = CannonPlayerState.State.Set;
			GuageController.OnFire ();
			powerControl.StopMeter ();

		}
		else if (currentCannonState.CurrentState == CannonPlayerState.State.Set) {
			 
			currentCannonState.CurrentState = CannonPlayerState.State.None;

			var fireController = currentCannon.GetComponent<CannonFireController> ();
			fireController.Fire (powerControl.PowerRating);
			powerControl.ResetMeter ();
			GuageController.OnReady ();
			//currentCannonState.ResetBarrowPosition ();

		}

	}

	public void OnNextMeatButton() {
		var currentIndex = GameController.CurrentBullet;
		currentIndex++;
		if (currentIndex > GameController.bulletPrefabs.Count) {
			currentIndex = 1;
		}
	
		GameController.CurrentBullet = currentIndex;

	}

	public void OnPauseButton() {
		
		GameController.EndGame();

	}



}
