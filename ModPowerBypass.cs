using System;
using Team17;
using UnityEngine;

namespace MyFirstPlugin;

public class ModPowerBypass : MonoBehaviour {
    public ModPowerBypass(IntPtr ptr) : base(ptr) {}

    // Private Static Variables
    private static Rect windowRect = new Rect(400, 400, 250, 75);
    private static float sliderValue = 1.0f;

    void OnGUI() {
        if (State.isInGame && State.isPaused) {
            windowRect = GUI.Window(1001, windowRect, (GUI.WindowFunction)DrawWindow, "Power Bypass");
        }
    }

    void Update() {
        if (State.isInGame) {
            Services.GameState.CurrentState.Cast<GameplayGameState>().m_PrimaryUser.Ball.m_rigidBody.mass = 1.0f / sliderValue;
        }
    }

    static void DrawWindow(int id) {
        GUILayout.Label("Power Multipler: " + sliderValue.ToString("F1") + "x");
        float raw = GUILayout.HorizontalSlider(sliderValue, 1f, 10f);
        sliderValue = Mathf.Round(raw * 10f) / 10f;
        GUI.DragWindow(new Rect(0, 0, 250, 20));
    }
}