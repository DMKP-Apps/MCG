using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlEvents : MonoBehaviour {

    public PowerControl PowerControl;

    public void OnMeterButtonClick()
    {
        PowerControl.ToggleMeter();
        Debug.Log(string.Format("Power: {0}", PowerControl.PowerRating));



    }
}
