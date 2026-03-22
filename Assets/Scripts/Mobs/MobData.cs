using UnityEngine;

[CreateAssetMenu(fileName = "NewMob",
                 menuName = "ScriptableObjects/MobData")]
public class MobData : ScriptableObject
{
    [Header("Identity")]
    public string mobName;
    public Sprite mobSprite;

    [Header("Stats")]
    public int maxHP;
    public int armor;
    public int accuracy;
    public int evasion;
    public int damageMin;
    public int damageMax;
    public int expReward;
    public int expCap;
    public int goldDrop;

    [Header("Movement")]
    public int detectionRange = 5;
    public int moveSpeed = 1;
}