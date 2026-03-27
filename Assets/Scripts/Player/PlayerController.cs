using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveDistance = 1f;
    private Camera mainCamera;
    private List<Vector2> currentPath = new List<Vector2>();
    private bool isTakingStep = false;
    private MobBase pendingAttackTarget = null;

    [Header("Combat Stats")]
    public int damageMin = 3;
    public int damageMax = 7;
    public int accuracy = 12;
    public int evasion = 8;

    void Start()
    {
        SnapToGrid();
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (!TurnQueue.Instance.IsPlayerTurn()) return;

        Vector2 direction = Vector2.zero;

        if (Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.UpArrow))
            direction = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.S) ||
                 Input.GetKeyDown(KeyCode.DownArrow))
            direction = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.A) ||
                 Input.GetKeyDown(KeyCode.LeftArrow))
            direction = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.D) ||
                 Input.GetKeyDown(KeyCode.RightArrow))
            direction = Vector2.right;

        if (direction != Vector2.zero)
        {
            currentPath.Clear();
            TryMove(direction);
            return;
        }

        if (Input.GetMouseButtonDown(0))
            HandleMouseClick();
    }

    void HandleMouseClick()
    {
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(
            Input.mousePosition
        );

        Vector2 clickedTile = new Vector2(
            Mathf.Floor(mouseWorld.x) + 0.5f,
            Mathf.Floor(mouseWorld.y) + 0.5f
        );

        Vector2 playerTile = new Vector2(
            Mathf.Floor(transform.position.x) + 0.5f,
            Mathf.Floor(transform.position.y) + 0.5f
        );

        if (clickedTile == playerTile) return;

        // Check if clicked tile has a mob
        Collider2D mobHit = Physics2D.OverlapBox(
            clickedTile,
            Vector2.one * 0.9f,
            0f,
            LayerMask.GetMask("Mobs")
        );

        if (mobHit != null)
        {
            MobBase mob = mobHit.GetComponent<MobBase>();
            if (mob == null) return;

            // Check if already adjacent — attack immediately
            Vector2 difference = clickedTile - playerTile;
            float distance = Mathf.Abs(difference.x) +
                            Mathf.Abs(difference.y);

            if (distance <= 1f)
            {
                TryAttack(mob);
                return;
            }

            // Not adjacent — path to nearest tile beside the mob
            Vector2? approachTile =
                GetApproachTile(clickedTile, playerTile);

            if (approachTile != null)
            {
                // Store mob as target for when we arrive
                pendingAttackTarget = mob;

                List<Vector2> path = Pathfinder.Instance.FindPath(
                    playerTile,
                    approachTile.Value
                );

                if (path != null && path.Count > 0)
                    currentPath = path;
                    TakePathStep();
            }

            return;
        }

        // No mob — path to clicked tile normally
        List<Vector2> path2 = Pathfinder.Instance.FindPath(
            playerTile,
            clickedTile
        );

        if (path2 != null && path2.Count > 0)
        {
            pendingAttackTarget = null;
            currentPath = path2;
            TakePathStep();
        }
    }

    // Find the walkable tile closest to player adjacent to mob
    Vector2? GetApproachTile(Vector2 mobTile, Vector2 playerTile)
    {
        Vector2[] neighbors = new Vector2[]
        {
        new Vector2(mobTile.x + 1, mobTile.y),
        new Vector2(mobTile.x - 1, mobTile.y),
        new Vector2(mobTile.x, mobTile.y + 1),
        new Vector2(mobTile.x, mobTile.y - 1)
        };

        Vector2? closest = null;
        float closestDist = float.MaxValue;

        foreach (Vector2 neighbor in neighbors)
        {
            // Check if walkable
            Collider2D wallHit = Physics2D.OverlapBox(
                neighbor,
                Vector2.one * 0.9f,
                0f,
                LayerMask.GetMask("Walls")
            );

            Collider2D mobHit = Physics2D.OverlapBox(
                neighbor,
                Vector2.one * 0.9f,
                0f,
                LayerMask.GetMask("Mobs")
            );

            if (wallHit != null || mobHit != null) continue;

            float dist = Vector2.Distance(neighbor, playerTile);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = neighbor;
            }
        }

        return closest;
    }

    void TryAttack(MobBase target)
    {
        // Roll between 1 and 100
             int roll = Random.Range(1, 101);

        // Base hit chance is 70% modified by accuracy vs evasion
        int hitChance = 70 + (accuracy - target.mobData.evasion) * 5;

        // Clamp between 10% minimum and 95% maximum
        hitChance = Mathf.Clamp(hitChance, 10, 95);

        if (roll > hitChance)
        {
            Debug.Log("Player missed. Roll: " + roll +
                      " needed: " + hitChance + " or below");
            TurnQueue.Instance.OnPlayerTurnEnd();
            return;
        }

        int damage = Random.Range(damageMin, damageMax + 1);
        Debug.Log("Player hits for " + damage +
                  ". Roll: " + roll +
                  " needed: " + hitChance + " or below");
        target.TakeDamage(damage);

        TurnQueue.Instance.OnPlayerTurnEnd();
    }

    public void OnTurnReady()
    {
        if (pendingAttackTarget != null &&
            pendingAttackTarget.gameObject == null)
            pendingAttackTarget = null;

        // If path is complete and there is a pending attack
        if (currentPath.Count == 0 && pendingAttackTarget != null)
        {
            // Check if now adjacent to target
            Vector2 playerTile = new Vector2(
                Mathf.Floor(transform.position.x) + 0.5f,
                Mathf.Floor(transform.position.y) + 0.5f
            );

            Vector2 mobTile = new Vector2(
                Mathf.Floor(pendingAttackTarget.transform.position.x)
                + 0.5f,
                Mathf.Floor(pendingAttackTarget.transform.position.y)
                + 0.5f
            );

            float distance = Vector2.Distance(playerTile, mobTile);

            if (distance <= 1.5f)
            {
                MobBase target = pendingAttackTarget;
                pendingAttackTarget = null;
                TryAttack(target);
                return;
            }

            // Mob moved away while player was walking
            // Path to new approach tile
            Vector2? newApproach =
                GetApproachTile(mobTile, playerTile);

            if (newApproach != null)
            {
                List<Vector2> path = Pathfinder.Instance.FindPath(
                    playerTile,
                    newApproach.Value
                );

                if (path != null && path.Count > 0)
                {
                    currentPath = path;
                    TakePathStep();
                    return;
                }
            }

            // Cannot reach mob
            pendingAttackTarget = null;
            return;
        }

        // Continue path if steps remaining
        if (currentPath.Count > 0 && !isTakingStep)
            TakePathStep();
    }

    void TakePathStep()
    {
        if (currentPath.Count == 0) return;
        if (isTakingStep) return;

        StartCoroutine(TakePathStepCoroutine());
    }

    IEnumerator TakePathStepCoroutine()
    {
        isTakingStep = true;

        Vector2 nextStep = currentPath[0];

        // Check if mob is blocking
        Collider2D mobCheck = Physics2D.OverlapBox(
            nextStep,
            Vector2.one * 0.9f,
            0f,
            LayerMask.GetMask("Mobs")
        );

        if (mobCheck != null)
        {
            currentPath.Clear();
            isTakingStep = false;
            yield break;
        }

        currentPath.RemoveAt(0);

        // Check if wall is blocking
        Collider2D wallCheck = Physics2D.OverlapBox(
            nextStep,
            Vector2.one * 0.9f,
            0f,
            LayerMask.GetMask("Walls")
        );

        if (wallCheck != null)
        {
            currentPath.Clear();
            isTakingStep = false;
            yield break;
        }

        // Move player
        transform.position = new Vector3(
            nextStep.x,
            nextStep.y,
            0
        );

        // Delay BEFORE ending turn so movement is visible
        yield return new WaitForSeconds(0.08f);

        // Clear step lock before ending turn
        isTakingStep = false;

        // End turn — this triggers mob turns then OnTurnReady
        TurnQueue.Instance.OnPlayerTurnEnd();
    }

    void TryMove(Vector2 direction)
    {
        Vector2 origin = new Vector2(
            Mathf.Floor(transform.position.x) + 0.5f,
            Mathf.Floor(transform.position.y) + 0.5f
        );

        Vector2 target = origin + direction * moveDistance;

        Collider2D hit = Physics2D.OverlapBox(
            target,
            Vector2.one * 0.9f,
            0f,
            LayerMask.GetMask("Walls")
        );

        if (hit == null)
        {
            transform.position = new Vector3(
                target.x,
                target.y,
                0
            );

            TurnQueue.Instance.OnPlayerTurnEnd();
        }
    }

    void SnapToGrid()
    {
        transform.position = new Vector3(
            Mathf.Floor(transform.position.x) + 0.5f,
            Mathf.Floor(transform.position.y) + 0.5f,
            0
        );
    }

    public void ClearPath()
    {
        currentPath.Clear();
    }
}