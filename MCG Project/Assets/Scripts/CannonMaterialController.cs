using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonMaterialController : MonoBehaviour {

    public GameObject CannonBarrel;
    public GameObject CannonWheelLeft;
    public GameObject CannonWheelRight;

    public void SetMaterial(Material materialBarrel, Material materialWheel)
    {
        if (materialBarrel != null && CannonBarrel != null)
        {
            var rendCannon = CannonBarrel.GetComponent<Renderer>();
            rendCannon.material = materialBarrel;
        }

        if (materialWheel != null && CannonWheelLeft != null)
        {
            var rendWheelL = CannonWheelLeft.GetComponent<Renderer>();
            rendWheelL.material = materialWheel;
        }

        if (materialWheel != null && CannonWheelRight != null)
        {
            var rendWheelR = CannonWheelRight.GetComponent<Renderer>();
            rendWheelR.material = materialWheel;
        }

    }

}
