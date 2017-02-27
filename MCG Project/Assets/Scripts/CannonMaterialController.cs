using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonMaterialController : MonoBehaviour {

    public GameObject CannonBarrel;
    public GameObject CannonWheelLeft;
    public GameObject CannonWheelRight;

    public void SetMaterial(Material materialBarrel, Material materialWheel)
    {
        var rendCannon = CannonBarrel.GetComponent<Renderer>();
        rendCannon.material = materialBarrel;

        var rendWheelL = CannonWheelLeft.GetComponent<Renderer>();
        rendWheelL.material = materialWheel;

        var rendWheelR = CannonWheelRight.GetComponent<Renderer>();
        rendWheelR.material = materialWheel;
    }

}
