using System;
using Team17;
using UnityEngine;

namespace MyFirstPlugin;

public class ModPowerAngleDisplay : MonoBehaviour {
    public ModPowerAngleDisplay(IntPtr ptr) : base(ptr) {}

    // Private Static Variables
    private static Rect windowRect = new Rect(600, 600, 250, 75);

    void OnGUI() {
        if (State.isInGame) {
            windowRect = GUI.Window(1005, windowRect, (GUI.WindowFunction)DrawWindow, "Power+Angle Display");
        }
    }

    static void DrawWindow(int id) {
        
        float Power = Services.GameState.CurrentState.Cast<GameplayGameState>().m_PrimaryUser.m_hitForce;
        GUILayout.Label("Power: " + Power.ToString());

        Vector3 forward = Services.GameState.CurrentState.Cast<GameplayGameState>().m_PrimaryUser.PlayerCamera.transform.forward;
        float Angle = (Mathf.Atan2(forward.z, forward.x) * Mathf.Rad2Deg + 360.0f) % 360.0f;
        GUILayout.Label("Angle: " + Angle.ToString());
        GUI.DragWindow(new Rect(0, 0, 250, 20));
    }

}