using UnityEngine;

public class StoneCrawler : MobBase
{
    private Transform playerTransform;
    private bool isAggro = false;

    protected override void Start()
    {
        base.Start();

        // Snap to grid on spawn
        SnapToGrid();

        playerTransform = GameObject.FindWithTag("Player")
                                   .transform;

        /* Patrol method for mob repositioning (removed)
        patrolPoints = new Vector2[]
        {
            new Vector2(
                Mathf.Floor(transform.position.x) - 2 + 0.5f,
                Mathf.Floor(transform.position.y) + 0.5f),
            new Vector2(
                Mathf.Floor(transform.position.x) + 2 + 0.5f,
                Mathf.Floor(transform.position.y) + 0.5f)
        };
        */
    }

    public override void TakeTurn()
    {
        if (isDead) return;

        float distanceToPlayer = Vector2.Distance(
            transform.position,
            playerTransform.position
        );

        if (distanceToPlayer <= mobData.detectionRange)
            isAggro = true;

        if (isAggro)
            ChasePlayer();
    }

    void ChasePlayer()
    {
        float distanceToPlayer = Vector2.Distance(
            transform.position,
            playerTransform.position
        );

        if (distanceToPlayer <= 1.5f)
        {
            AttackPlayer();
            return;
        }

        Vector2 currentPos = GetSnappedPosition();

        // Try primary direction first
        Vector2 primaryDirection = GetDirectionToPlayer();
        Vector2 primaryTarget = currentPos + primaryDirection;

        if (IsTileWalkable(primaryTarget))
        {
            MoveTo(primaryTarget);
            return;
        }

        // Primary direction blocked
        // Try perpendicular directions as alternatives
        Vector2 altDirection1;
        Vector2 altDirection2;

        // If primary was horizontal try vertical alternatives
        if (primaryDirection.x != 0)
        {
            altDirection1 = Vector2.up;
            altDirection2 = Vector2.down;
        }
        else
        {
            // If primary was vertical try horizontal alternatives
            altDirection1 = Vector2.right;
            altDirection2 = Vector2.left;
        }

        // Pick the alternative that gets closer to the player
        Vector2 alt1Target = currentPos + altDirection1;
        Vector2 alt2Target = currentPos + altDirection2;

        float dist1 = Vector2.Distance(
            alt1Target,
            (Vector2)playerTransform.position
        );
        float dist2 = Vector2.Distance(
            alt2Target,
            (Vector2)playerTransform.position
        );

        // Try the closer alternative first
        if (dist1 <= dist2)
        {
            if (IsTileWalkable(alt1Target))
            {
                MoveTo(alt1Target);
                return;
            }
            if (IsTileWalkable(alt2Target))
            {
                MoveTo(alt2Target);
                return;
            }
        }
        else
        {
            if (IsTileWalkable(alt2Target))
            {
                MoveTo(alt2Target);
                return;
            }
            if (IsTileWalkable(alt1Target))
            {
                MoveTo(alt1Target);
                return;
            }
        }

        // Truly stuck - all three directions blocked
        // Try moving in the opposite of primary as last resort
        Vector2 backTarget = currentPos +
                             (-primaryDirection);
        if (IsTileWalkable(backTarget))
            MoveTo(backTarget);
    }

    Vector2 GetDirectionToPlayer()
    {
        Vector2 toPlayer = (Vector2)playerTransform.position
                         - (Vector2)transform.position;

        if (Mathf.Abs(toPlayer.x) >= Mathf.Abs(toPlayer.y))
            return new Vector2(Mathf.Sign(toPlayer.x), 0);
        else
            return new Vector2(0, Mathf.Sign(toPlayer.y));
    }    /* Patrol Method for other mobs (removed)
    void Patrol()
    {
        Vector2 target = patrolPoints[currentPatrolIndex];
        float distanceToPoint = Vector2.Distance(
            transform.position, target
        );

        if (distanceToPoint < 0.1f)
        {
            currentPatrolIndex =
                (currentPatrolIndex + 1) % patrolPoints.Length;
            target = patrolPoints[currentPatrolIndex];
        }

        Vector2 direction = (target -
                            GetSnappedPosition()).normalized;

        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
            direction = new Vector2(Mathf.Sign(direction.x), 0);
        else
            direction = new Vector2(0, Mathf.Sign(direction.y));

        Vector2 moveTarget = GetSnappedPosition() + direction;

        if (IsTileWalkable(moveTarget))
            MoveTo(moveTarget);
    }
    */

    void AttackPlayer()
    {
        int roll = Random.Range(1, 101);

        int hitChance = 70 + (mobData.accuracy -
                       GameManager.Instance.evasion) * 5;
        hitChance = Mathf.Clamp(hitChance, 10, 95);

        if (roll > hitChance)
        {
            Debug.Log("Stone Crawler missed. Roll: " + roll);
            return;
        }

        int damage = Random.Range(
            mobData.damageMin,
            mobData.damageMax + 1
        );

        Debug.Log("Stone Crawler attacks for " + damage);
        GameManager.Instance.TakeDamage(damage);
    }


    void SnapToGrid()
    {
        transform.position = new Vector3(
            Mathf.Floor(transform.position.x) + 0.5f,
            Mathf.Floor(transform.position.y) + 0.5f,
            0
        );
    }
}