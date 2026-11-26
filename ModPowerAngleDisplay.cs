using System;
using Team17;
using UnityEngine;

namespace MyFirstPlugin;

public class ModPowerAngleDisplay : MonoBehaviour {
    public ModPowerAngleDisplay(IntPtr ptr) : base(ptr) {}

    // Private Static Variables
    private static Rect windowRect = new Rect(600, 600, 250, 75);
    private static bool initialized = false;
    private static User user;

    void Update() {
        if (State.isInGame) {
            if (!initialized) {
                user = Services.GameState.CurrentState.Cast<GameplayGameState>().m_PrimaryUser;
                initialized = true;
            }
        } else {
            initialized = false;
        }
    }

    void OnGUI() {
        if (initialized) {
            windowRect = GUI.Window(1005, windowRect, (GUI.WindowFunction)DrawWindow, "Shot Display");
        }
    }

    static void DrawWindow(int id) {
        float power = user.m_hitForce;
        GUILayout.Label($"Power: {power:F0}");

        Vector3 forward = user.PlayerCamera.transform.forward;
        float angle = (Mathf.Atan2(forward.z, forward.x) * Mathf.Rad2Deg + 360.0f) % 360.0f;
        GUILayout.Label($"Angle: {angle:F1}°");

        GUI.DragWindow(new Rect(0, 0, 250, 20));
    }

}