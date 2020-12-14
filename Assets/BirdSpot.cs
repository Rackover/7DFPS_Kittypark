using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdSpot : MonoBehaviour
{
    public bool isRestingSpot = false;

    public float safeDistance = 2f;

    public static List<BirdSpot> spots = new List<BirdSpot>();

    public int id;

    public static BirdSpot GetSpotWithID(int id) {
        return spots.Find(o => o.id == id);
    }

    public static BirdSpot GetUnusedSpot() {
        return spots.Find(o => o.id == 0);
    }

    void Awake()
    {
        spots.Add(this);
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
