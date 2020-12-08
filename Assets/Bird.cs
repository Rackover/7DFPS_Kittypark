using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    public float flapChance = 0.2f;
    public float hopChance = 0.5f;

    bool isFlying = false;
    bool isOnGround = false;
    float courage = 0f;
    BirdSpot currentSpot;

    // 1) Pick resting spot
    // 2) At some point, take a random decision between other resting spot and non resting with no cats
    // 3) If non resting, hop around a little bit until someone attacks
    // 4) Try to flee if getting too close
    // X) Flap once in a while

    // Start is called before the first frame update
    void Start()
    {
        courage = Random.value;

        var spot = BirdSpot.GetRestingSpot();
        transform.position = spot.transform.position;
        transform.LookAt(new Vector3(0, transform.position.y, 0));

        StartCoroutine(AI());
    }
    
    IEnumerator AI(){
        // Offset
        yield return new WaitForSeconds(Random.value);

        while (true){
            if (isOnGround) {
                
                if (IsPlayerNearby()){
                    
                }

                if (Random.value > hopChance){
                    Hop();
                }
                else{
                    PickGrain();
                }
            }
            else{
                MoveHead();
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    void Flap(){

    }

    void Hop(){

    }

    void PickGrain(){

    }

    void MoveHead(){

    }

    void FlyTo(BirdSpot spot){

    }

    bool IsPlayerNearby(){
        if (!currentSpot){
            return false;
        }

        return Game.i.players.FindAll(o=>Vector3.Distance(currentSpot.transform.position, o.transform.position) > currentSpot.safeDistance * (1f-courage)).Count > 0;
    }
}
