using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PromptObject : MonoBehaviour
{
    public static float FallSpeed = 15f;
    public static float OffsetStrength = 1f;
    
    public EInputType inputType;
    public bool canBePressed = true;
    //public float fallSpeed = 150f;
    public Vector3 targetPosition;
    
    public List<Color> frontColors;
    public List<Color> backColors;

    private float timer;
    private float maxTimer = 5;
    private Material _material;

    private void Start()
    {
        if (_material == null)
            Debug.LogError("No Material attached to PromptObject");
    }

    void Update()
    {
        // Simple falling animation
        transform.position += CanoeController.Instance.transform.forward * -FallSpeed * Time.deltaTime;
        
        timer += Time.deltaTime;
        if  (timer >= maxTimer)
            Destroy(gameObject);
    }
    
    public void SetVisuals(EInputType inputType, bool isRight, bool isFront)
    {
        _material = GetComponent<MeshRenderer>().material;
        TextMeshProUGUI text = GetComponentInChildren<TextMeshProUGUI>();
        Color color = Color.white;
        switch (inputType)
        {
            case EInputType.StraightLeft:
                color = isRight ? backColors[0] : frontColors[1];
                text.text = !isRight ? "Z" : "P";
                break;
            case EInputType.StraightRight:
                color = isRight ? backColors[1] : frontColors[0];
                text.text = !isRight ? "C" : "I";
                break;
            case EInputType.Left:
                color = isFront ? backColors[1] : frontColors[1];
                text.text = isFront ? "P" : "C";
                break;
            case EInputType.Right:
                color = isFront ? backColors[0] : frontColors[0];
                text.text = isFront ? "I" : "Z";
                break;
            case EInputType.LeftHard:
                color = isRight ? backColors[1] : frontColors[0];
                text.text = !isRight ? "Z" : "P";
                break;
            case EInputType.RightHard:
                color = isRight ? backColors[0] : frontColors[1];
                text.text = !isRight ? "I" : "C";
                break;
        }
        
        _material.SetColor("_BaseColor", color);
    }
}