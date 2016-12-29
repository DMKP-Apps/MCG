using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindControl : MonoBehaviour {

    public Vector2 Wind = new Vector2();

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        //transform.rotation = Quaternion.Euler(Wind.x * 18, 0, Wind.y * 18);
        float value = 0.0f;
        if (Wind.x == 0 && Wind.y == 0) {
            value = 90;
        }
        else if (Wind.x > 0 && Wind.y > 0)
        {
            value = 45;
        }
        else if (Wind.x == 0 && Wind.y > 0)
        {
            value = 0;
        }
        else if (Wind.x > 0 && Wind.y < 0)
        {
            value = -45;
        }
        else if (Wind.x == 0 && Wind.y < 0)
        {
            value = -90;
        }
        else if (Wind.x < 0 && Wind.y < 0)
        {
            value = -135;
        }
        else if (Wind.x < 0 && Wind.y == 0)
        {
            value = -180;
        }
        else if (Wind.x < 0 && Wind.y > 0)
        {
            value = 135;
        }

        transform.rotation = Quaternion.Euler(-45, 0, value);

        var x = Wind.x < 0 ? Wind.x * -1 : Wind.x;
        var y = Wind.y < 0 ? Wind.y * -1 : Wind.y;

        Debug.Log(string.Format("Wind: {0}", (x + y).ToString("0.00")));


    }
}
