using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundShake : TransformAction
{


    public bool Enabled;
    public float maxDistance = 100f;
    public float maxForce = 10f;
    public float minForce = 10f;

    protected override void OnRun()
    {
        if (Enabled)
        {
            var player = GameController.GetCurrentCannonPlayer();
            if (player != null && player.GetComponent<Rigidbody>() != null)
            {
                var distance = Vector3.Distance(this.transform.position, player.transform.position);
                if (distance < maxDistance)
                {
                    var force = maxForce == minForce ? maxForce : (maxForce - minForce) * (1 - distance / maxDistance) + minForce;
                    var rbody = player.GetComponent<Rigidbody>();
                    rbody.AddForce(rbody.transform.up * force, ForceMode.Impulse);
                }
            }

        }
    }
}
