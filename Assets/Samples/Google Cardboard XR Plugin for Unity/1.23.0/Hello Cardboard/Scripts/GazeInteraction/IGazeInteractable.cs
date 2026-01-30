using UnityEngine;

public interface IGazeInteractable
{
    bool CanGaze { get; }

    void OnGazeEnter();
    void OnGazeExit();
    void OnGazeActivate();
    void UpdateGazeProgress(float progress);
}

