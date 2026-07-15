using UnityEngine;
using TMPro;

public class Lock : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public static int number {  get; private set; }
    private void Start()
    {
        UpdateLock(Random.Range(4, 12));
        textMeshPro.text = $"Unlock after {number} moves";
    }
    private void UpdateLock(int num)
    {
        number = num;
        textMeshPro.text = $"Unlock after {number} moves";
    }
    public void LockCheck()
    {
        --number;
        textMeshPro.text = $"Unlock after {number} moves";
        if (number <= 0) Destroy(gameObject);
    }
}
