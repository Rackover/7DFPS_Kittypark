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
        textMesh.text = Game.i.GetNameForId(playerId);
        material = bowlRenderer.material;
        material.color = Color.HSVToRGB(((playerId*13f) / 255f) % 1f, 1f, 0.5f);
    }

    public void UpdateSize() {
        var score = Game.i.GetScore(playerId);
        crockett.localScale = Vector3.right + Vector3.forward + Vector3.up * Mathf.Clamp01(Mathf.Log(score + 2f)/6f);
        shuriken.Play();
    }
}
