using System;
using Team17;
using UnityEngine;

namespace MyFirstPlugin;

public class State : MonoBehaviour {
    public State(IntPtr ptr) : base(ptr) {}

    // Public
    public static bool isInGame = false;
    public static bool isPaused = false;

    void Update() {

        isInGame = Services.GameState.CurrentState.TryCast<GameplayGameState>() != null;
        
        if (isInGame) {
            isPaused = Services.GameState.CurrentState.Cast<GameplayGameState>().m_InGameMenuFlow.m_IsRequestingPause;
        }
    }

}