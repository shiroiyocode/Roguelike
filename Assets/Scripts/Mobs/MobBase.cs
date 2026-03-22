using UnityEngine;

public abstract class MobBase : MonoBehaviour,
                                TurnQueue.ITurnTaker
{
    [Header("Stats")]
    public MobData mobData;

    protected int currentHP;
    protected bool isDead = false;

    protected virtual void Start()
    {
        if (mobData != null)
            currentHP = mobData.maxHP;

        // Register with TurnQueue when mob spawns
        TurnQueue.Instance.Register(this);
    }

        public abstract void TakeTurn();

    // Called when mob receives damage
    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;

        int actualDamage = Mathf.Max(1, amount - mobData.armor);
        currentHP -= actualDamage;
        currentHP = Mathf.Max(currentHP, 0);  

        Debug.Log(mobData.mobName + " took " +
                  actualDamage + " damage. HP: " +
                  currentHP + "/" + mobData.maxHP);

        if (currentHP <= 0)
            Die();
    }

    protected virtual void Die()
    {
        isDead = true;

        // Give player EXP
        GameManager.Instance.GainEXP(mobData.expReward);

        // Give player gold if mob drops any
        if (mobData.goldDrop > 0)
            GameManager.Instance.AddGold(mobData.goldDrop);

        // Unregister from TurnQueue
        TurnQueue.Instance.Unregister(this);

        Debug.Log(mobData.mobName + " died.");
        Destroy(gameObject);
    }

    // Check if a tile is walkable for mob movement
    protected bool IsTileWalkable(Vector2 position)
    {
        Collider2D hit = Physics2D.OverlapBox(
            position,
            Vector2.one * 0.9f,
            0f,
            LayerMask.GetMask("Walls")
        );
        return hit == null;
    }

    // Move mob to a target position
    protected void MoveTo(Vector2 targetPosition)
    {
        transform.position = new Vector3(
            targetPosition.x,
            targetPosition.y,
            0
        );
    }

    protected Vector2 GetSnappedPosition()
    {
        return new Vector2(
            Mathf.Floor(transform.position.x) + 0.5f,
            Mathf.Floor(transform.position.y) + 0.5f
        );
    }
}