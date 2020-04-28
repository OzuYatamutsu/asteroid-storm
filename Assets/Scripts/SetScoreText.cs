using UnityEngine;
using TMPro;

public class SetScoreText : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI ScoreText;

    private void Awake()
    {
        // Access and set score text on awake
        ScoreText.text = $"Final score: {GameManager.Instance.Score}";
    }
}
