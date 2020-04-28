using UnityEngine;

public class UpdateShieldUI : MonoBehaviour
{
    [SerializeField] public RectTransform HealthBar;
    private float MaxHealthBarWidth;

    void Awake()
    {
        MaxHealthBarWidth = HealthBar.rect.width;
    }


    /// <summary>
    /// Set healthbar UI to player hull percentage
    /// </summary>
    void LateUpdate()
    {
        HealthBar.sizeDelta = new Vector2(
            (GameManager.Instance.PlayerShip.HullStrength / 100) * MaxHealthBarWidth,  // x
            HealthBar.rect.height  // y
        );
    }
}
