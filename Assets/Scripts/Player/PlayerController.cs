using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveDistance = 1f;
    private Camera mainCamera;
    private List<Vector2> currentPath = new List<Vector2>();

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

        // Check if clicked tile has a mob on it
        Collider2D mobHit = Physics2D.OverlapBox(
            clickedTile,
            Vector2.one * 0.9f,
            0f,
            LayerMask.GetMask("Mobs")
        );

        if (mobHit != null)
        {
            // Check if mob is adjacent before attacking
            Vector2 difference = clickedTile - playerTile;
            float distance = Mathf.Abs(difference.x) +
                           Mathf.Abs(difference.y);

            if (distance <= 1f)
            {
                // Attack the mob
                MobBase mob = mobHit.GetComponent<MobBase>();
                if (mob != null)
                    TryAttack(mob);
            }
            return;
        }

        // No mob on tile — try to move there
        List<Vector2> path = Pathfinder.Instance.FindPath(
            playerTile,
            clickedTile
        );

        if (path != null && path.Count > 0)
        {
            currentPath = path;
            TakePathStep();
        }
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
        if (currentPath.Count > 0)
            TakePathStep();
    }

    void TakePathStep()
    {
        if (currentPath.Count == 0) return;

        Vector2 nextStep = currentPath[0];

        // mobBlock check
        Collider2D mobCheck = Physics2D.OverlapBox(
            nextStep,
            Vector2.one * 0.9f,
            0f,
            LayerMask.GetMask("Mobs")
        );

        if (mobCheck != null)
        {
            // Mob is blocking path — stop walking
            currentPath.Clear();
            return;
        }

        currentPath.RemoveAt(0);

        Collider2D wallCheck = Physics2D.OverlapBox(
            nextStep,
            Vector2.one * 0.9f,
            0f,
            LayerMask.GetMask("Walls")
        );

        if (wallCheck != null)
        {
            currentPath.Clear();
            return;
        }

        transform.position = new Vector3(
            nextStep.x,
            nextStep.y,
            0
        );

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