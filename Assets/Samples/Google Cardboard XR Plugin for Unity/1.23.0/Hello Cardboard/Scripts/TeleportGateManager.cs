using UnityEngine;
using System.Collections.Generic;

public class TeleportGateManager : MonoBehaviour
{
    public static TeleportGateManager Instance { get; private set; }

    [SerializeField] private List<TeleportGate> allGates = new List<TeleportGate>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        // Auto-find all gates in the scene if list is empty
        if (allGates.Count == 0)
        {
            allGates.AddRange(FindObjectsOfType<TeleportGate>());
            Debug.Log($"Found {allGates.Count} teleport gates in scene");
        }
    }

    public void ShowOutlinesForAllGates(TeleportGate excludeGate)
    {
        foreach (TeleportGate gate in allGates)
        {
            if (gate != null && gate != excludeGate)
            {
                gate.ShowOutline(true);
            }
        }
    }

    public void HideAllOutlines()
    {
        foreach (TeleportGate gate in allGates)
        {
            if (gate != null)
            {
                gate.ShowOutline(false);
            }
        }
    }

    public void RegisterGate(TeleportGate gate)
    {
        if (!allGates.Contains(gate))
        {
            allGates.Add(gate);
            Debug.Log($"Registered gate: {gate.GateID}");
        }
    }

    public void UnregisterGate(TeleportGate gate)
    {
        if (allGates.Contains(gate))
        {
            allGates.Remove(gate);
            Debug.Log($"Unregistered gate: {gate.GateID}");
        }
    }
}