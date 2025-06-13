using System.Collections;
using UnityEngine;

public class TogglePaddle : MonoBehaviour
{
    public GameObject left;
    public GameObject right;
    
    public float TimerDuration = 2;
    public float WaitDuration = 1;
    private float timer;
    private bool isRight = true;
    private bool canStart = false;

    private void Start()
    {
        StartCoroutine(WaitAnimationStartCoroutine());
    }

    private void Update()
    {
        if (!canStart)
            return;
        
        timer += Time.deltaTime;
        if (timer >= TimerDuration)
        {
            timer = 0;
            TogglePaddles();
            Debug.Log("change " + isRight);
        }
    }

    void TogglePaddles()
    {
        isRight = !isRight;
        right.SetActive(isRight);
        left.SetActive(!isRight);
    }

    IEnumerator WaitAnimationStartCoroutine()
    {
        yield return new WaitForSeconds(WaitDuration);
        canStart = true;
        Debug.Log("Started");
    }
}
