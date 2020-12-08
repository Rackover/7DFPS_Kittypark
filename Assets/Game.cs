using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public List<Player> players = new List<Player>();

    public static Game i;

    void Awake(){
        i = this;
    }
}
