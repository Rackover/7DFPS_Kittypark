using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour {
    public float tiltAmount = 3.5f;
    public float catchUpSpeed = 20f;
    public float tiltLerpSpeed = 10f;
    public float xBobAmplitude = 0.01f;
    public float yBobAmplitude = 0.1f;
    public float headHeight = 1.66f;
    public float bobSpeed = 20f;
    public float fov = 70f;
    public float bonusFov = 20f;
    public int bobLevels = 4;
    public float weaponBobAmount = 0.1f;
    public float weaponBobCatchUpSpeed = 15f;

    public PlayerMovement player;
    public Weapon weapon;

    float smoothPlayerSpeedAmount = 0f;
    float currentTilt = 0f;
    new Camera camera;
    Vector3 weaponAnchor;

    // Start is called before the first frame update
    void Start() {
        camera = GetComponent<Camera>();
        weaponAnchor = weapon.transform.localPosition;
    }

    // Update is called once per frame
    void Update() {

        var speedAmount = player.Speed / player.maxPlanarSpeed;
        var speedAmountForBob = Mathf.Round(speedAmount * bobLevels) / bobLevels;

        smoothPlayerSpeedAmount = Mathf.Lerp(smoothPlayerSpeedAmount, speedAmount, 4f * Time.deltaTime);

        camera.fieldOfView = fov + bonusFov * smoothPlayerSpeedAmount;

        var tilt = -transform.InverseTransformPoint(player.transform.position).x * tiltAmount;
        currentTilt = Mathf.Lerp(currentTilt, tilt, tiltLerpSpeed * Time.deltaTime);

        var inputMouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        transform.Rotate(-Vector3.right * inputMouse.y * Time.deltaTime * player.mouseSensitivity, Space.Self);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, player.transform.eulerAngles.y, player.transform.eulerAngles.z + currentTilt);

        var bobDisplacement = GetBobDisplacement(player.IsGrounded ? speedAmountForBob : 0f);

        var weaponBob = GetBobDisplacement(player.IsGrounded ? smoothPlayerSpeedAmount : 0f) * weaponBobAmount;
        weapon.transform.localPosition = Vector3.Lerp(weapon.transform.localPosition , weaponAnchor + new Vector3(weaponBob.x, weaponBob.y, 0), weaponBobCatchUpSpeed * Time.deltaTime);

        var target = player.transform.position;
        target += transform.up * headHeight;
        target += transform.right * bobDisplacement.x;
        target += transform.up * bobDisplacement.y;
        transform.position = Vector3.Lerp(transform.position, target, catchUpSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Fire1")) {
            weapon.Fire();
        }
    }

    Vector2 GetBobDisplacement(float speedAmount) {
        var bob = new Vector2(Mathf.Sin(Time.time * speedAmount * bobSpeed) * xBobAmplitude, ((Mathf.Sin(Time.time * speedAmount * bobSpeed * 2f) + 1f) / 2f) * yBobAmplitude);

        return bob;
    }
}
