using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    [SerializeField] Player playerScript;

    [SerializeField] private float mouseSensitivity = 20f;
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float accelerationSpeed = 20f;
    [SerializeField] private float maxWalkPlanarSpeed = 15f;
    [SerializeField] private float maxCrouchingPlanarspeed = 5f;
    [SerializeField] private float maxSprintingPlanarSpeed = 30f;
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private float maxFallSpeed = 1000f;
    [SerializeField] private float fakeFriction = 0.8f;
    [SerializeField] private float raycastDistance = 0.55f;
    [SerializeField] private float maxSprintDuration = 2f;
    [SerializeField] private float strafeReduxAmount = 0.33f;
    [SerializeField] private float wallJumpCooldown = 0.25f;
    [SerializeField] private float wallJumpRadius = 0.13f;
    [SerializeField] private float wallJumpLookAtSpeed = 25f;
    [SerializeField] private float wallJumpNormalProportion = 0.5f;
    [SerializeField] private float wallJumpAdditionalForce = 5f;
    [SerializeField] private bool enableWallJumps = true;
    [SerializeField] private SphereCollider groundCollider;
    [SerializeField] private SphereCollider wallJumpTrigger;
    [SerializeField] private new PlayerCamera camera;
    [SerializeField] private ParticleSystem jumpFX;

    public float Speed => Vector3.Scale(Vector3.right + Vector3.forward, body.velocity).magnitude;
    public bool IsGrounded => isGrounded;
    public float GroundColliderRadius => groundCollider.radius;
    public bool IsCrouching => isCrouching;
    public bool IsSprinting => isSprinting;
    public bool IsJumping => isJumping;
    public float MouseSensitivity => mouseSensitivity;
    public float MaxPlanarSpeed => maxSprintingPlanarSpeed;
    public float CrouchingSpeedReduction => maxCrouchingPlanarspeed / maxWalkPlanarSpeed;

    public bool IsLocal => playerScript.IsLocal;

    Vector3 movementVector = Vector3.zero;

    Rigidbody body;
    bool isGrounded;
    bool hasJumped = false;
    bool isCrouching;
    bool isJumping;
    bool isSprinting;
    bool isAgainstWall;

    float lastWallJumpTime;
    float sprintStaminaRemaining = 0f;
    List<Collider> againstWalls = new List<Collider>();

    // Start is called before the first frame update
    void Start() {
        body = GetComponent<Rigidbody>();

        wallJumpTrigger.radius = GroundColliderRadius + wallJumpRadius;

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update() {
        Cursor.visible = false;
        var inputMouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        transform.Rotate(transform.up * inputMouse.x * Time.deltaTime * mouseSensitivity * 1.5f);

        isJumping = isJumping || Input.GetButtonDown("Jump");

        bool isStrafing = Mathf.Round(Input.GetAxis("Horizontal")) != 0f;
        bool isMovingForward = Input.GetAxis("Vertical") > 0f && !isStrafing;

        isSprinting = Input.GetButton("Sprint") && isMovingForward;
        isCrouching = Input.GetButton("Crouch") && !isSprinting && !isStrafing;
    }

    bool CheckIfAgainstWall(out Vector3 oppositeDirection) {
        oppositeDirection = Vector3.zero;
        if (isGrounded || !enableWallJumps) {
            return false;
        }

        if (againstWalls.Count > 0) {
            var hitPoint = againstWalls[0].ClosestPoint(transform.position);
            oppositeDirection = transform.position-hitPoint;
            return true;
        }

        return false;
    }

    bool CheckIfIsGrounded(out float distance) {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance)) {
            distance = Vector3.Distance(transform.position, hit.point) / raycastDistance;
            return true;
        }

        distance = Mathf.Infinity;
        return false;
    }

    private void FixedUpdate() {

        bool wasGrounded = isGrounded;
        isGrounded = CheckIfIsGrounded(out float groundDistanceAmount);

        if (isGrounded) {
            // reset walljump cooldown when grounded
            lastWallJumpTime = 0f;
        }

        isAgainstWall = CheckIfAgainstWall(out Vector3 wallJumpDirection);

        var inputMovement = new Vector3(Input.GetAxis("Horizontal")* strafeReduxAmount, 0f, Input.GetAxis("Vertical"));

        var localMovementVector = transform.InverseTransformVector(movementVector);
        var planarLocalVelocity = Vector3.Scale(
            Vector3.forward * (isGrounded ? Mathf.Abs(inputMovement.z) * fakeFriction + 1f - fakeFriction : 1f)
            + Vector3.right * (isGrounded ? Mathf.Abs(inputMovement.x) * fakeFriction + 1f - fakeFriction : 1f),
            localMovementVector
        );

        var maxPlanarSpeed = isSprinting ? maxSprintingPlanarSpeed : (isCrouching ? maxCrouchingPlanarspeed : maxWalkPlanarSpeed);

        planarLocalVelocity += (Vector3.forward * inputMovement.z + Vector3.right * inputMovement.x) * accelerationSpeed * Time.deltaTime;
        planarLocalVelocity = Vector3.ClampMagnitude(planarLocalVelocity, maxPlanarSpeed);

        localMovementVector.x = planarLocalVelocity.x;
        localMovementVector.z = planarLocalVelocity.z;

        if (isGrounded) {
            if (!wasGrounded) {
                hasJumped = false;
            }

            if (isJumping) {
                if (!hasJumped) {
                    localMovementVector.y = jumpForce;
                    PlayJumpFX();
                    hasJumped = true;
                }
            }
            else {
                localMovementVector.y = -(groundDistanceAmount * raycastDistance - groundCollider.radius);
            }
        }
        else {
            if (isAgainstWall && isJumping && lastWallJumpTime < Time.time - wallJumpCooldown) {
                localMovementVector = Vector3.Lerp(localMovementVector + wallJumpDirection * jumpForce, wallJumpDirection * (jumpForce + localMovementVector.magnitude), wallJumpNormalProportion);
                localMovementVector.y = jumpForce + wallJumpAdditionalForce;

                PlayJumpFX();
                lastWallJumpTime = Time.time;
                ////StartCoroutine(LookToSmoothly(wallJumpDirection));

                hasJumped = true;
            }
            else {
                localMovementVector.y = Mathf.Max(localMovementVector.y - gravity * Time.deltaTime, -maxFallSpeed);

                if (wasGrounded && !hasJumped) {
                    // Reset gravity when just exiting the floor
                    localMovementVector.y = 0f;
                }
            }
        }

        movementVector = transform.TransformVector(localMovementVector);

        ////Debug.Log("GROUNDED: " + isGrounded + "  IsJumping? " + isJumping + "  hasJumped? " + hasJumped + "  Y vector " + movementVector.y);

        // Reset buffers
        isJumping = false;

        ApplyMovementVector();
    }


    IEnumerator LookToSmoothly(Vector3 direction) {
        while (lastWallJumpTime > Time.time - wallJumpCooldown) { 
            transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * wallJumpLookAtSpeed);
            yield return new WaitForEndOfFrame();
        }
    }

    void PlayJumpFX() {
        if (jumpFX) {
            jumpFX.transform.forward = -movementVector;
            jumpFX.Play();
        }
    }
    void ApplyMovementVector() {

        body.velocity = movementVector;
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.isTrigger) {
            againstWalls.Add(other);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (!other.isTrigger) {
            againstWalls.Remove(other);
        }
    }

    private void OnDrawGizmos() {
    }
}
