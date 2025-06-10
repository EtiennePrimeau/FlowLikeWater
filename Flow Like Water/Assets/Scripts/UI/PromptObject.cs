using UnityEngine;

public class PromptObject : MonoBehaviour
{
    public static float fallSpeed = 15;
    
    public EInputType inputType;
    public bool canBePressed = true;
    //public float fallSpeed = 150f;
    public Vector3 targetPosition;

    private float timer;
    private float maxTimer = 5;
    
    
    void Update()
    {
        // Simple falling animation
        transform.position += CanoeController.Instance.transform.forward * -fallSpeed * Time.deltaTime;
        
        timer += Time.deltaTime;
        if  (timer >= maxTimer)
            Destroy(gameObject);
    }

    public float GetSpeed()
    {
        return fallSpeed;
    }
}