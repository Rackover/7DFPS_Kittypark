using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public float mouseSensitivity = 20f;
    public float gravity = 20f;
    public float accelerationSpeed = 20f;
    public float maxPlanarSpeed = 30f;
    public float jumpForce = 20f;
    public float maxFallSpeed = 1000f;
    public float fakeFriction = 0.8f;
    public float raycastDistance = 0.55f;

    public float Speed => Vector3.Scale(Vector3.right + Vector3.forward, body.velocity).magnitude;
    public bool IsGrounded => isGrounded;

    Vector3 movementVector = Vector3.zero;

    SphereCollider groundCollider;
    Rigidbody body;
    bool isGrounded;
    bool hasJumped = false;
    bool isJumping;

    // Start is called before the first frame update
    void Start() {
        body = GetComponent<Rigidbody>();
        groundCollider = GetComponent<SphereCollider>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update() {
        Cursor.visible = false;
        var inputMouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        transform.Rotate(transform.up * inputMouse.x * Time.deltaTime * mouseSensitivity * 1.5f);

        isJumping = isJumping || Input.GetButtonDown("Jump");

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

        var inputMovement = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        var localMovementVector = transform.InverseTransformVector(movementVector);
        var planarLocalVelocity = Vector3.Scale(
            Vector3.forward * (isGrounded ? Mathf.Abs(inputMovement.z) * fakeFriction + 1f - fakeFriction : 1f)
            + Vector3.right * (isGrounded ? Mathf.Abs(inputMovement.x) * fakeFriction + 1f - fakeFriction : 1f),
            localMovementVector
        );

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
                    hasJumped = true;
                }
            }
            else {
                localMovementVector.y = -(groundDistanceAmount * raycastDistance - groundCollider.radius);
            }
        }
        else {
            localMovementVector.y = Mathf.Max(localMovementVector.y - gravity * Time.deltaTime, -maxFallSpeed);

            if (wasGrounded && !hasJumped) {
                // Reset gravity when just exiting the floor
                localMovementVector.y = 0f;
            }
        }

        movementVector = transform.TransformVector(localMovementVector);

        ////Debug.Log("GROUNDED: " + isGrounded + "  IsJumping? " + isJumping + "  hasJumped? " + hasJumped + "  Y vector " + movementVector.y);

        // Reset buffer
        isJumping = false;

        ApplyMovementVector();
    }

    void ApplyMovementVector() {

        body.velocity = movementVector;
    }
}
