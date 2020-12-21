using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtLocal : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Game.i.LocalPlayer) {
            transform.LookAt(Game.i.LocalPlayer.movement.transform.position);
        }
    }
}
