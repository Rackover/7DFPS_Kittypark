using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    [SerializeField] float flightTime = 2f;

    public BirdSpot currentSpot;
    public Animator birdAnimator;

    public int id;
    bool isFlying = false;
    bool isOnGround = false;

    // Start is called before the first frame update
    void Start() {
        RefreshFromSpot();
    }

    public void RefreshFromSpot() {
        transform.position = currentSpot.transform.position;
        transform.LookAt(new Vector3(0, transform.position.y, 0));
    }

    public void Flap(){
        Debug.Log("Flap");
        transform.LookAt(new Vector3(0, transform.position.y, 0));
        // Play flap animation
    }

    public void Hop(){
        Vector3 targetPosition = Vector3.Lerp(transform.position + Vector3.right * Random.value + Vector3.up * Random.value, currentSpot.transform.position, 0.5f);
        StartCoroutine(Coroutine_HopTo(targetPosition));
    }

    public void PickGrain() {
        Debug.Log("Pick");
        // Play grain animation
    }


    public void FlyTo(BirdSpot spot){
        StartCoroutine(Coroutine_FlyTo(spot));
    }

    IEnumerator Coroutine_FlyTo(BirdSpot spot) {
        isFlying = true;
        currentSpot = spot;

        float time = 0f;
        float delta = 1 / 60f;
        Vector3 startPosition = this.transform.position;
        while(Vector3.Distance(this.transform.position, spot.transform.position) > 0.04f) {
            this.transform.position = Vector3.Lerp(startPosition, spot.transform.position, time);
            time += delta / flightTime;
            yield return new WaitForSeconds(delta);
        }

        RefreshFromSpot();

        isFlying = false;
    }

    IEnumerator Coroutine_HopTo(Vector3 targetPosition) {
        float steps = 10f; // Keep that FLOAT thanks
        Vector3 startPosition = transform.position;
        for (int i = 1; i <= steps; i++) {
            transform.position = Vector3.Lerp(startPosition, targetPosition, steps / i);
            yield return new WaitForEndOfFrame();
        }
    }
}
