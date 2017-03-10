using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIInputController : MonoBehaviour {

	public PowerControl powerControl;
	private GameController GameController;
	public GuageController GuageController;
    private GameObject controls;

    private bool isSpaceKeyDown = false;
    private bool isCtrlKeyDown = false;
    private bool isPauseKeyDown = false;


    // Update is called once per frame
    void Update () {
        if (GameController == null)
        {
            GameController = GameObject.Find("GameController").GetComponent<GameController>();
        }

        if (controls == null)
        {
            var uitxtctrl = GameObject.FindObjectOfType<UITextController>();
            if (uitxtctrl != null)
            {
                controls = uitxtctrl.gameObject;
            }
        }

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftAlt)) && controls != null && controls.activeInHierarchy && !isSpaceKeyDown)
        {
            isSpaceKeyDown = true;
            //_time = DateTime.Now;
            OnPowerButtonClick();
        }
        else if((Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.LeftAlt)))
        {
            isSpaceKeyDown = false;
        }

        if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftShift)) && controls != null && controls.activeInHierarchy && !isCtrlKeyDown)
        {
            isCtrlKeyDown = true;
            //_time = DateTime.Now;
            OnNextMeatButton();
        }
        else if ((Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.LeftShift)))
        {
            isCtrlKeyDown = false;
        }

        if ((Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) && !isPauseKeyDown)
        {
            isPauseKeyDown = true;
            //_time = DateTime.Now;
            OnPauseButton();
        }
        else if ((Input.GetKeyUp(KeyCode.Alpha1) || Input.GetKeyUp(KeyCode.Keypad1) || Input.GetKeyUp(KeyCode.Alpha2) || Input.GetKeyUp(KeyCode.Keypad2)))
        {
            isPauseKeyDown = false;
        }

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
			fireController.Fire (powerControl.PowerRating, GuageController.Accuracy.accuracy);
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
