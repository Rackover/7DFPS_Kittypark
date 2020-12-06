using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public GameObject explosion;
    public Rigidbody body;
    public float velocity = 60f;
    public float lifeTime = 2f;

    float startTime;

    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        body.velocity = transform.forward * velocity;
        body.angularVelocity = transform.forward * 10f;
    }

    // Update is called once per frame
    void Update()
    {
        if (lifeTime + startTime < Time.time) {
            Explode();
        }
    }

    private void OnTriggerEnter(Collider other) {
        Explode();
    }
    void Explode() {
        explosion.transform.parent = null;
        explosion.SetActive(true);
        Destroy(explosion, 4f);
        Destroy(this.gameObject);
    }
}
