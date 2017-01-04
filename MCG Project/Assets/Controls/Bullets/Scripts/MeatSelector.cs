using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeatSelector : MonoBehaviour {

	public List<Sprite> Images;

	public Image Image;

	private int currentIndex = 0;

	public GameController GameController;

	void Start() {
		GameController = GameObject.Find ("GameController").GetComponent<GameController> ();
	}

	public void OnNextMeatButton() {

		currentIndex++;
		if (currentIndex + 1 > Images.Count) {
			currentIndex = 0;
		}

		Image.sprite = Images [currentIndex];

		GameController.CurrentBullet = currentIndex + 1;

	}
}
