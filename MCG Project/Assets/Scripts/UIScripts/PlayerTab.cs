using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTab : MonoBehaviour {

    public Image Bar;
    public Text PlayerText;
    private int playerNumber = 1;

    public List<Sprite> Bars;

    public void SetPlayerNumber(int number)
    {
        playerNumber = number;
        if (Bar != null && Bars != null && Bars.Count >= playerNumber && playerNumber > 0)
        {
            Bar.sprite = Bars[playerNumber - 1];
        }

        if (PlayerText != null)
        {
            PlayerText.text = string.Format("Player {0}", playerNumber);
        }
    }
}
