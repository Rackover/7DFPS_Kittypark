using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour {
    [SerializeField] float flightTime = 2f;
    [SerializeField] ParticleSystem shuriken;
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip flap;
    [SerializeField] AudioClip dieSound;

    public BirdSpot currentSpot;
    public Animator birdAnimator;

    public int id;
    public float timeout = 10f;

    bool isFlying = false;
    bool isOnGround = false;
    DateTime lastUpdate = DateTime.Now;

    // Start is called before the first frame update
    void Start() {
        RefreshFromSpot();
    }

    private void Update() {
        birdAnimator.SetBool("IsFlying", isFlying);

        if ((DateTime.Now - lastUpdate).TotalSeconds > timeout) {
            // glitched
            Debug.Log("Killing bird "+id+" because no info since "+ (DateTime.Now - lastUpdate).TotalSeconds+" seconds, assuming it's glitched");
            Kill();
        }
    }

    public void RefreshFromSpot() {
        lastUpdate = DateTime.Now;
        transform.position = currentSpot.transform.position;
        transform.LookAt(new Vector3(0, transform.position.y, 0));
    }

    public void Flap() {
        lastUpdate = DateTime.Now;
        transform.LookAt(new Vector3(0, transform.position.y, 0));
        birdAnimator.SetTrigger("OnFlap");
        source.PlayOneShot(flap);
        // Play flap animation
    }

    public void Heartbeat() {
        lastUpdate = DateTime.Now;
    }

    public void Hop() {
        lastUpdate = DateTime.Now;
        Vector3 targetPosition = Vector3.Lerp(transform.position + Vector3.right * UnityEngine.Random.value + Vector3.forward * UnityEngine.Random.value, currentSpot.transform.position, 0.5f);
        StartCoroutine(Coroutine_HopTo(targetPosition));
        birdAnimator.SetTrigger("OnHop");
    }

    public void PickGrain() {
        lastUpdate = DateTime.Now;
        birdAnimator.SetTrigger("OnPick");
        // Play grain animation
    }

    public void Kill(bool withFX=false) {
        Game.i.deadBirds.Add(id);

        if (withFX) {
            shuriken.Play();
            shuriken.transform.parent = null;
            Destroy(shuriken.gameObject, 3f);
        }

        Destroy(gameObject);
    }

    public void FlyTo(BirdSpot spot) {
        lastUpdate = DateTime.Now;
        StartCoroutine(Coroutine_FlyTo(spot));
    }

    IEnumerator Coroutine_FlyTo(BirdSpot spot) {
        isFlying = true;
        currentSpot = spot;

        float time = 0f;
        float delta = 1 / 60f;
        Vector3 startPosition = this.transform.position;
        while (Vector3.Distance(this.transform.position, spot.transform.position) > 0.04f) {
            transform.LookAt(spot.transform.position);
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

        transform.LookAt(Vector3.one * (UnityEngine.Random.value * 2 - 1), Vector3.up);
    }

    private void OnTriggerEnter(Collider other) {
        var p = other.GetComponentInChildren<PlayerMovement>();
        if (p) {
            if (p.IsLocal && !p.IsGrounded) {
                // Caught!
                Game.i.SendCaughtBird(this.id);
                p.playerScript.source.PlayOneShot(dieSound);
                Kill(withFX:true);
            }
        }
    }
}
