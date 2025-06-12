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
    public float dotLimit = -0.5f;
    
    public List<Color> frontColors;
    public List<Color> backColors;

    public GameObject text;
    
    
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
        Vector3 newPos = transform.position + CanoeController.Instance.transform.forward * -FallSpeed * Time.deltaTime;
        newPos = new Vector3(newPos.x, 0f, newPos.z);
        transform.position = newPos;

        timer += Time.deltaTime;
        //if  (timer >= maxTimer)
        //    Destroy(gameObject);
        
        Vector3 direction = (transform.position - CanoeController.Instance.transform.position).normalized;
        Debug.DrawRay(transform.position, direction, Color.red);
        if (Vector3.Dot(CanoeController.Instance.transform.forward, direction) < dotLimit)
        {
            Destroy(gameObject);
        }
        
        float t = timer / (CanoeController.Instance.stateDelayTime + 0.5f);
        if (t > 1)
        {
            Destroy(text);
            Debug.Log("Destroy");
            return;
        }
        
        float size = Mathf.Lerp(0, 1, t);
        Vector3 scale = new Vector3(size, size, size);
        text.transform.localScale = scale;
        
        float y = Mathf.Lerp(1, 3, t);
        text.transform.localPosition = new Vector3(text.transform.localPosition.x, y, text.transform.localPosition.z);
    }
    
    public void SetVisuals(EInputType inputType, bool isRight, bool isFront)
    {
        _material = GetComponent<MeshRenderer>().material;
        TextMeshProUGUI text = GetComponentInChildren<TextMeshProUGUI>();
        Color color = Color.white;
        switch (inputType)
        {
            case EInputType.StraightLeft:
                color = isRight ? frontColors[0] : backColors[0];
                text.text = !isRight ? "V" : "U";
                break;
            case EInputType.StraightRight:
                color = isRight ? backColors[0] : frontColors[0];
                text.text = !isRight ? "R" : "N";
                break;
            case EInputType.Left:
                color = isFront ? backColors[0] : frontColors[0];
                text.text = isFront ? "U" : "N";
                break;
            case EInputType.Right:
                color = isFront ? frontColors[0] : backColors[0];
                text.text = isFront ? "R" : "V";
                break;
            case EInputType.LeftHard:
                color = isRight ? frontColors[0] : backColors[0];
                text.text = !isRight ? "V" : "U";
                break;
            case EInputType.RightHard:
                color = isRight ? backColors[0] : frontColors[0];
                text.text = !isRight ? "R" : "N";
                break;
        }
        
        _material.SetColor("_BaseColor", color);
    }
}