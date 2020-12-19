using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bowl : MonoBehaviour
{
    [SerializeField] private Transform crockett;
    [SerializeField] private Renderer bowlRenderer;
    [SerializeField] private TextMesh textMesh;
    [SerializeField] private float maxScale = 1f;
    [SerializeField] private ParticleSystem shuriken;

    public int playerId;
    
    private Material material;

    public void UpdateBowl() {
        Debug.Log("Getting name for player id " + playerId);
        textMesh.text = Game.i.GetNameForId(playerId);
        Debug.Log("Instantiating bowl material");
        material = bowlRenderer.material;

        Debug.Log("Casting PID to float PID");
        float fId = playerId;

        Debug.Log("Setting color");
        material.color = Color.HSVToRGB(((fId * 13f) / 255f) % 1f, 1f, 0.5f);

        Debug.Log("Done updating bowl!");
    }

    public void UpdateSize() {
        var score = Game.i.GetScore(playerId);
        Debug.Log("About to set scale to to Log(score+2)/6...");
        crockett.localScale = Vector3.right + Vector3.forward + Vector3.up * Mathf.Clamp01(Mathf.Log(score + 2f)/6f);
        Debug.Log(Mathf.Clamp01(Mathf.Log(score + 2f) / 6f));
        Debug.Log("Done!");
        shuriken.Play();
    }
}
