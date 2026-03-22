using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Text References")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI expText;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateHUD();
    }

    public void UpdateHUD()
    {
        if (GameManager.Instance == null) return;

        hpText.text = "HP: " + GameManager.Instance.currentHP +
                      " / " + GameManager.Instance.maxHP;

        goldText.text = "Gold: " +
                        GameManager.Instance.currentGold;

        expText.text = "EXP: " +
                       GameManager.Instance.currentEXP +
                       " / " +
                       GameManager.Instance.expToNextLevel;
    }
}