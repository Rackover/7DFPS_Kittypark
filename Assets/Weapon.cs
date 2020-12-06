using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameObject missilePrefab;
    public Transform rocketParent;
    public float delayBetweenShoot = 1f;

    float lastShot = 0f;
    Animator weaponAnimator;

    private void Start() {
        weaponAnimator = GetComponent<Animator>();
    }

    public void Fire() {
        if (lastShot < Time.time - delayBetweenShoot) {
            Instantiate(missilePrefab, rocketParent).transform.parent = null;
            weaponAnimator.SetTrigger("OnShoot");
            lastShot = Time.time;
        }
    }
}
