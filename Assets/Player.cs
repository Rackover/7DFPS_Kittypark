using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    public bool IsLocal { get; set; } = false;

    public GameObject FPSContainer;
    public GameObject externalViewContainer;
    public Animator externalAnimator;

    public Texture[] furTextures;
    public Renderer bodyRenderer;

    public PlayerMovement movement;
    public int id = 0;
    public float catchUpSpeed = 4f;
    public ParticleSystem jumpShuriken;
    public TextMesh textMesh;

    NetControllers.DeserializedPlayerMove previousMovement;
    NetControllers.DeserializedPlayerMove targetMovement;

    float timer = 0f;
    bool isJumping;

    private void Awake() {
        FPSContainer.SetActive(false);
        externalViewContainer.SetActive(false);
    }

    // Start is called before the first frame update
    void Start() {
        if (IsLocal) {
            FPSContainer.SetActive(true);
        }
        else {
            externalViewContainer.SetActive(true);
            bodyRenderer.material.mainTexture = furTextures[id % furTextures.Length];
        }
    }

    private void Update() {
        if (IsLocal) return;

        timer += Time.deltaTime * catchUpSpeed;

        if (targetMovement != null) {
            float normalizedTimer = Mathf.Clamp01(timer);
            transform.position = Vector3.Slerp(previousMovement?.position ?? transform.position, targetMovement.position, normalizedTimer);
            transform.rotation = Quaternion.Slerp(previousMovement?.rotation ?? transform.rotation, targetMovement.rotation, normalizedTimer);

            externalAnimator.SetBool("IsMoving", Vector3.Distance(transform.position, targetMovement.position) > 0.02f);
            externalAnimator.SetBool("IsRunning", targetMovement.isRunning);
            externalAnimator.SetBool("IsSneaking", targetMovement.isSneaking && !targetMovement.isRunning);
            externalAnimator.SetBool("IsJumping", targetMovement.isJumping);

            if (targetMovement.isJumping) {
                if (!isJumping) {
                    jumpShuriken.Play();
                }
                isJumping = true;
            }
            else {
                isJumping = false;
            }
        }

        textMesh.text = Game.i.GetNameForId(id);
    }

    private void FixedUpdate() {
        if (IsLocal) {
            Game.i.SendMyPosition(movement.transform.position, movement.transform.rotation, movement);
        }
    }

    // Remote stuff

    public void UpdatePosition(NetControllers.DeserializedPlayerMove movement) {

        ////previousMovement = targetMovement;
        previousMovement = new NetControllers.DeserializedPlayerMove() {
            position = transform.position,
            rotation = transform.rotation
        };

        targetMovement = movement;

        timer = 0f;
    }

}
