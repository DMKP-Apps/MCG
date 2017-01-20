using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PlayerInfo : MonoBehaviour {

	public List<Sprite> PlayerBars;
	public Image imageBar;
	public Text PlayerText;
	public Text HoleText;
	public Text ScoreText;
	public Text StrokeText;
	public Text ParText;
	public Text YRDSToPINText;
	public Text ShotDistanceText;

	public Image SelectedBullet;
	public Image NextBullet;

	private GameController gameController;
	void Start()
	{
		gameController = GameObject.Find("GameController").GetComponent<GameController>();
	}

	void LateUpdate()
	{
		if(BulletIndex != gameController.CurrentBullet) {
			BulletIndex = gameController.CurrentBullet;
		}

	}

	void Update()
	{
		var currentBullet = gameController.GetCurrentShotBullet ();
		var currentPlayer = gameController.GetCurrentCannonPlayer ();
		if (currentBullet != null && currentPlayer != null) {
			ShotDistanceText.gameObject.SetActive (true);
			StrokeText.gameObject.SetActive (false);
			ParText.gameObject.SetActive (false);
			var fromShotPlayer = System.Convert.ToInt32(System.Math.Ceiling(Vector3.Distance (currentBullet.transform.position, currentPlayer.transform.position)));
			this.ShotDistance = fromShotPlayer;
		} else {
			ShotDistanceText.gameObject.SetActive (false);
			StrokeText.gameObject.SetActive (true);
			ParText.gameObject.SetActive (true);
			this.ShotDistance = 0;
		}

	}

	public int PlayerIndex {
		get { return currentPlayerIndex; }
		set { 
			SetPlayer (value);
		}
	}
	private int currentPlayerIndex = 0;

	private void SetPlayer(int index)
	{
		if (index > PlayerBars.Count - 1) {
			index = 0;
		}

		currentPlayerIndex = index;
		imageBar.sprite = PlayerBars [currentPlayerIndex];
		PlayerText.text = string.Format ("Player {0}", currentPlayerIndex + 1);

	}

	public int HoleNumber {
		get { return currentHoleNumber; }
		set { 
			SetHoleNumber (value);
		}
	}
	private int currentHoleNumber = 0;

	private void SetHoleNumber(int index)
	{

		currentHoleNumber = index;
		HoleText.text = string.Format ("{0}", currentHoleNumber);

	}

	public int Score {
		get { return score; }
		set { 
			SetScore(value);
		}
	}
	private int score = 0;

	private void SetScore(int index)
	{

		score = index;
		if (score < 1) {
			ScoreText.text = string.Format ("{0}", score);
		} else {
			ScoreText.text = string.Format ("+{0}", score);
		}

	}

	public int Stroke {
		get { return currentStroke; }
		set { 
			SetStroke (value);
		}
	}
	private int currentStroke = 0;

	private void SetStroke(int index)
	{

		currentStroke = index;
		StrokeText.text = string.Format ("Stroke: {0}", currentStroke);

	}

	public int Par {
		get { return currentPar; }
		set { 
			SetPar (value);
		}
	}
	private int currentPar = 0;

	private void SetPar(int index)
	{

		currentPar = index;
		ParText.text = string.Format ("Par: {0}", currentPar);

	}

	public int YRDSToPIN {
		get { return currentYRDSToPIN; }
		set { 
			SetYRDSToPIN (value);
		}
	}
	private int currentYRDSToPIN = 0;

	private void SetYRDSToPIN(int index)
	{

		currentYRDSToPIN = index;
		YRDSToPINText.text = string.Format ("{0}", currentYRDSToPIN);

	}

	public int ShotDistance {
		get { return currentShotDistance; }
		set { 
			SetShotDistance (value);
		}
	}
	private int currentShotDistance = 0;

	private void SetShotDistance(int index)
	{

		currentShotDistance = index;
		ShotDistanceText.text = string.Format ("Shot: {0} YRDS", currentShotDistance);

	}

	public int BulletIndex {
		get { return currentBulletIndex; }
		set { 
			SetBulletIndex (value);
		}
	}
	private int currentBulletIndex = 1;

	private void SetBulletIndex(int index)
	{

		currentBulletIndex = index;

		var bulletPrefabs = gameController.bulletPrefabs.Select (x => x.GetComponent<BulletData> ()).ToList();
		SelectedBullet.sprite = bulletPrefabs [currentBulletIndex - 1].Image;
		NextBullet.sprite = bulletPrefabs [currentBulletIndex + 1 > bulletPrefabs.Count ? 0 : currentBulletIndex].Image;

	}

}
