using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdSpot : MonoBehaviour
{
    public bool IsSafe {get; private set;}

    public bool isRestingSpot = false;

    public Bird currentBird;

    public float safeDistance = 2f;

    private static List<BirdSpot> spots = new List<BirdSpot>();

    private static List<BirdSpot> RestingSpots => spots.FindAll(o=>o.isRestingSpot);

    private static List<BirdSpot> SafeGroundSpots => spots.FindAll(o=>!o.isRestingSpot && o.IsSafe);

    public static BirdSpot GetSafeGroundSpot(){
        var safeSpots = SafeGroundSpots.FindAll(o=>o.currentBird == null);
        if (safeSpots.Count <= 0){
            return null;
        }

        return safeSpots[Random.Range(0, safeSpots.Count)];
    }

    public static BirdSpot GetRestingSpot(){
        var restSpots = RestingSpots.FindAll(o=>o.currentBird == null);
        return restSpots[Random.Range(0, restSpots.Count)];
    }

    void Awake()
    {
        spots.Add(this);
    }

    void Update(){
        IsSafe = Game.i.players.FindAll(o=>Vector3.Distance(o.transform.position, transform.position) < safeDistance).Count < 0;
    }

#if UNITY_EDITOR
    void OnDrawGizmos(){
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(transform.position, Vector3.one*0.4f);

        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, safeDistance);
    }
#endif
}
