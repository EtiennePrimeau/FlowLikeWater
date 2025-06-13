using TMPro;
using UnityEngine;

public class FinishUI : MonoBehaviour
{

    public TextMeshProUGUI hits;
    public TextMeshProUGUI misses;
    public TextMeshProUGUI efficiency;

    public void SetScore()
    {
        float hit = GameManager.Instance.promptsHit;
        float miss = GameManager.Instance.promptsMissed;
        hits.text = "HITS  :   " + hit.ToString();
        misses.text = "MISSES  :   " + miss.ToString();
        float prct = hit / (hit + miss) * 100f;
        efficiency.text = "ACCURACY  :   " + $"{prct:F2}";
    }
    
}
