using System;
using Team17;
using UnityEngine;

namespace MyFirstPlugin;

public class ModResetBypass : MonoBehaviour {
    public ModResetBypass(IntPtr ptr) : base(ptr) {}

    // Private Static Variables
    private static Rect windowRect = new Rect(700, 400, 250, 75);

    void OnGUI() {
        if (State.isInGame && State.isPaused) {
            windowRect = GUI.Window(1002, windowRect, (GUI.WindowFunction)DrawWindow, "Reset Bypass");
        }
    }

    static void DrawWindow(int id) {

        if (GUILayout.Button("Disable Reset")) {
            Services.GameState.CurrentState.Cast<GameplayGameState>().m_PrimaryUser.Ball.m_collisionResetController.m_willReset = null;
            Services.GameState.CurrentState.Cast<GameplayGameState>().m_PrimaryUser.Ball.m_collisionResetController.m_mightReset = null;
        }

        if (GUILayout.Button("Reset To Last Shot")) {
            Services.GameState.CurrentState.Cast<GameplayGameState>().m_PrimaryUser.Ball.ResetToLastShot();
        }

        GUI.DragWindow(new Rect(0, 0, 250, 20));
    }
}