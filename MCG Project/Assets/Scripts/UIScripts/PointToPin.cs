using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointToPin : MonoBehaviour {

	private GameController gameController;
	private PlayerInfo playerInfo;
	private CannonPlayerState player;

	public List<Material> playerColours;
	private int currentPlayer = 1;
	public GameObject arrow;

	// Use this for initialization
	void Start () {
		gameController = GameObject.Find("GameController").GetComponent<GameController>();
	}

	//private CannonPlayerState GetPlayer(GameObject item) {
	//	CannonPlayerState playerController = item.GetComponent<CannonPlayerState> ();
	//	var parent = item.transform.parent;
	//	while (parent != null && playerController == null) {
	//		playerController = parent.GetComponent<CannonPlayerState> ();
	//		if (playerController != null) {
	//			break;
	//		}
	//		parent = parent.parent;
	//	}
	//	return playerController;
	//}

    private CannonPlayerState GetPlayer(GameObject item)
    {

        if (item.GetComponent<AssociatedPlayerState>() != null)
        {
            if (item.GetComponent<AssociatedPlayerState>().playerState != null)
            {
                return item.GetComponent<AssociatedPlayerState>().playerState;
            }
        }

        CannonPlayerState playerController = item.GetComponent<CannonPlayerState>();
        var parent = item.transform.parent;
        while (parent != null && playerController == null)
        {
            if (parent.GetComponent<AssociatedPlayerState>() != null)
            {
                if (parent.GetComponent<AssociatedPlayerState>().playerState != null)
                {
                    playerController = parent.GetComponent<AssociatedPlayerState>().playerState;
                    break;
                }
            }

            playerController = parent.GetComponent<CannonPlayerState>();
            if (playerController != null)
            {
                break;
            }
            parent = parent.parent;
        }
        return playerController;
    }


    void LateUpdate () {
		transform.LookAt(gameController.GetHolePinPosition ());
		if (player == null || playerInfo == null) {
			player = GetPlayer (this.gameObject);
			playerInfo = GameObject.FindObjectOfType<PlayerInfo>();
		}

		if (playerInfo != null && player != null) {

			if (player.playerNumber != currentPlayer) {
				currentPlayer = player.playerNumber;
				var index = currentPlayer - 1;
				if (index >= playerColours.Count) {
					index = 0;
				}
				var rend = arrow.GetComponent<Renderer>();
				rend.material = playerColours[index];

			}

			//Debug.Log (string.Format ("{0},{1} = {2}", gameController.GetHolePinPosition (), transform.position, Vector3.Distance (gameController.GetHolePinPosition (), transform.position)));
			var toPin = System.Convert.ToInt32(System.Math.Ceiling(Vector3.Distance (gameController.GetHolePinPosition (), player.transform.position)));
			playerInfo.YRDSToPIN = toPin;
		}
	}
}
