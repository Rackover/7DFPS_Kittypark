using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour {
    [SerializeField] Player playerScript;

    [SerializeField] private float tiltAmount = 3.5f;
    [SerializeField] private float catchUpSpeed = 20f;
    [SerializeField] private float tiltLerpSpeed = 10f;
    [SerializeField] private float xBobAmplitude = 0.01f;
    [SerializeField] private float yBobAmplitude = 0.1f;
    [SerializeField] private float yBobSprintAmplitude = 0.5f;
    [SerializeField] private float headHeight = 1.66f;
    [SerializeField] private float headHeightWhenCrouching = 0.3f;
    [SerializeField] private float bobSpeed = 20f;
    [SerializeField] private float bobSprintSpeed = 6f;
    [SerializeField] private float fov = 70f;
    [SerializeField] private float bonusFov = 20f;
    [SerializeField] private float bobLevels = 4;
    [SerializeField] private float weaponBobAmount = 0.1f;
    [SerializeField] private float weaponBobCatchUpSpeed = 15f;
    [SerializeField] private Animator handsAnimator;
    [SerializeField] private Renderer handsRenderer;
    [SerializeField] private Transform foregroundHands;
    [SerializeField] private bool correctHandsAngle = true;

    public PlayerMovement player;
    public Weapon weapon;

    float smoothPlayerSpeedAmount = 0f;
    float currentTilt = 0f;
    new Camera camera;
    Vector3 foregroundAnchor;

    // Start is called before the first frame update
    void Start() {
        camera = GetComponent<Camera>();
        foregroundAnchor = foregroundHands.localPosition;
    }

    // Update is called once per frame
    void Update() {

        var speedAmount = player.Speed / player.MaxPlanarSpeed;
        var speedAmountForBob = Mathf.Round(speedAmount * bobLevels) / bobLevels;

        smoothPlayerSpeedAmount = Mathf.Lerp(smoothPlayerSpeedAmount, speedAmount, 4f * Time.deltaTime);

        camera.fieldOfView = fov + bonusFov * smoothPlayerSpeedAmount;

        var tilt = -transform.InverseTransformPoint(player.transform.position).x * tiltAmount;
        currentTilt = Mathf.Lerp(currentTilt, tilt, tiltLerpSpeed * Time.deltaTime);

        var inputMouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        transform.Rotate(-Vector3.right * inputMouse.y * Time.deltaTime * player.MouseSensitivity, Space.Self);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, player.transform.eulerAngles.y, player.transform.eulerAngles.z + currentTilt);

        var bobDisplacement = GetBobDisplacement(player.IsGrounded ? speedAmountForBob : 0f);

        var target = player.transform.position;
        target += transform.up * ((player.IsCrouching ? headHeightWhenCrouching : headHeight) - player.GroundColliderRadius);
        target += transform.right * bobDisplacement.x;
        target += transform.up * bobDisplacement.y;
        transform.position = Vector3.Lerp(transform.position, target, catchUpSpeed * Time.deltaTime);

        // Unused
        if (weapon) {
            if (Input.GetButtonDown("Fire1")) {
                weapon.Fire();
            }
        }

        var weaponBob = GetBobDisplacement(player.IsGrounded ? smoothPlayerSpeedAmount : 0f) * weaponBobAmount;

        // Hands
        handsAnimator.SetBool("IsSprinting", player.IsSprinting && player.IsGrounded);
        handsAnimator.SetBool("IsCrouching", player.IsCrouching && player.IsGrounded);
        handsAnimator.SetBool("IsJumping", !player.IsGrounded);

        if (correctHandsAngle) {
            foregroundHands.localPosition = Vector3.Lerp(foregroundHands.localPosition, foregroundAnchor + new Vector3(weaponBob.x, weaponBob.y, 0), weaponBobCatchUpSpeed * Time.deltaTime);
        }

        if (player.IsCrouching || player.IsSprinting || !player.IsGrounded) {
            handsRenderer.enabled = true;
            float angle = foregroundHands.localEulerAngles.z;

            if (player.IsCrouching) {
                handsAnimator.speed = smoothPlayerSpeedAmount / player.CrouchingSpeedReduction;
            }
            else {
                handsAnimator.speed = 1f;
            }

            if (correctHandsAngle) {
                foregroundHands.eulerAngles = player.transform.eulerAngles + Vector3.up * -90;
            }

            // Covnert to 180s

            // reduce the angle  
            angle = angle % 360;

            // force it to be the positive remainder, so that 0 <= angle < 360  
            angle = (angle + 360) % 360;

            // force into the minimum absolute value residue class, so that -180 < angle <= 180  
            if (angle > 180) {
                angle -= 360;
            }

            if (correctHandsAngle) {
                foregroundHands.localPosition += Vector3.up * Mathf.Min(0f, (angle / 90f) * 0.25f);
            }
        }
        else {
            handsRenderer.enabled = false;
        }
    }

    Vector2 GetBobDisplacement(float speedAmount) {
        var bob = new Vector2(Mathf.Sin(Time.time * speedAmount * (player.IsSprinting ? bobSprintSpeed : bobSpeed)) * xBobAmplitude, ((Mathf.Sin(Time.time * speedAmount * (player.IsSprinting ? bobSprintSpeed : bobSpeed) * 2f) + 1f) / 2f) * (player.IsSprinting ? yBobSprintAmplitude : yBobAmplitude));

        return bob;
    }
}
