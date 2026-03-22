using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnQueue : MonoBehaviour
{
    public static TurnQueue Instance { get; private set; }

    public interface ITurnTaker
    {
        void TakeTurn();
    }

    private List<ITurnTaker> turnTakers =
        new List<ITurnTaker>();
    private bool isProcessing = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Register(ITurnTaker turnTaker)
    {
        if (!turnTakers.Contains(turnTaker))
            turnTakers.Add(turnTaker);
    }

    public void Unregister(ITurnTaker turnTaker)
    {
        turnTakers.Remove(turnTaker);
    }

    public void OnPlayerTurnEnd()
    {
        if (!isProcessing)
            StartCoroutine(ProcessTurns());
    }

    IEnumerator ProcessTurns()
    {
        isProcessing = true;

        yield return new WaitForSeconds(0.1f);

        // Process all mob turns
        List<ITurnTaker> currentTakers =
            new List<ITurnTaker>(turnTakers);

        foreach (ITurnTaker taker in currentTakers)
        {
            taker.TakeTurn();
            yield return new WaitForSeconds(0.1f);
        }

        isProcessing = false;

        // Tell player turn cycle is complete
        // Continue path if one is queued
        PlayerController player =
        FindFirstObjectByType<PlayerController>();
        if (player != null)
            player.OnTurnReady();
    }

    public bool IsPlayerTurn()
    {
        return !isProcessing;
    }
}