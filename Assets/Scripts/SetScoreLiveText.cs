using UnityEngine;
using TMPro;

public class SetScoreLiveText : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI ScoreText;

    private void LateUpdate()
    {
        // Update score every tick
        ScoreText.text = $"Score: {GameManager.Instance.Score}";
    }
}
