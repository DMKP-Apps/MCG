using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMarker : MonoBehaviour {

	private GameController gameController;
	private CannonPlayerState player;

	public List<Material> playerColours;
	private int currentPlayer = 0;
	

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
		
		if (player == null) {
			player = GetPlayer (this.gameObject);
			
		}

		if (player != null) {

			if (player.playerNumber != currentPlayer) {
				currentPlayer = player.playerNumber;
				var index = currentPlayer - 1;
				if (index >= playerColours.Count) {
					index = 0;
				}
				var rend = this.GetComponent<Renderer>();
				rend.material = playerColours[index];

			}
		}
	}
}
