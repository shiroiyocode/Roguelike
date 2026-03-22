using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    [Header("Player Stats")]
    public int currentHP = 30;
    public int maxHP = 30;
    public int currentGold = 0;
    public int currentFloor = 1;
    public int currentEXP = 0;
    public int expToNextLevel = 20;
    public int playerLevel = 1;
    public int evasion = 8; 
    public int accuracy = 12;
    public int mobsKilled = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Called when player takes damage
    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0);

        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateHUD();

        if (currentHP <= 0)
            GameOver();
    }

    // Called when player heals
    public void Heal(int amount)
    {
        currentHP += amount;
        currentHP = Mathf.Min(currentHP, maxHP);

        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateHUD();
    }

    // Called when player picks up gold
    public void AddGold(int amount)
    {
        currentGold += amount;

        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateHUD();
    }

    // Called when player spends gold
    public bool SpendGold(int amount)
    {
        if (currentGold < amount)
            return false;

        currentGold -= amount;
        return true;
    }

    // Called when player gains EXP
    public void GainEXP(int amount)
    {
        currentEXP += amount;
        mobsKilled++;

        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateHUD();

        if (currentEXP >= expToNextLevel)
            LevelUp();
    }

    void LevelUp()
    {
        playerLevel++;
        currentEXP -= expToNextLevel;
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.4f);
        maxHP += 5;
        currentHP = maxHP;
        Debug.Log("Level up! Now level " + playerLevel);
    }

    void GameOver()
    {
        Debug.Log("Game Over");
    }

    // Called when descending to next floor
    public void DescendFloor()
    {
        currentFloor++;
    }
}
