using System;
using Team17;
using UnityEngine;
using HarmonyLib;

namespace MyFirstPlugin;

public class ModReuseLastShot : MonoBehaviour {
    public ModReuseLastShot(IntPtr ptr) : base(ptr) {}

    // Private Static Variables
    private static Rect windowRect = new Rect(1000, 400, 250, 150);
    
    // Public Static Variables
    public static float lastPower = 0f;
    public static bool lockPower = false;
    public static Vector3 lastAngle = new Vector3(0f, 0f, 0f);
    public static bool lockAngle = false;

    void OnGUI() {
        if (State.isInGame && State.isPaused) {
            windowRect = GUI.Window(1003, windowRect, (GUI.WindowFunction)DrawWindow, "Reuse Last Shot");
        }
    }

    static void DrawWindow(int id) {
        GUILayout.Label("Last Power: " + lastPower.ToString());
        if (GUILayout.Button("Lock Power: " + lockPower.ToString())) {
            lockPower = !lockPower;
        }
        GUILayout.Label("Last Angle: " + lastAngle.ToString());
        if (GUILayout.Button("Lock Angle: " + lockAngle.ToString())) {
            lockAngle = !lockAngle;
        }
        if (GUILayout.Button("Shoot")) {
            Shoot(lastPower, lastAngle);
        }
        GUI.DragWindow(new Rect(0, 0, 250, 20));
    }

    // Checks if CheckForBallHit will run
    public static bool CheckForBallHitCheck() {
        User user = Services.GameState.CurrentState.Cast<GameplayGameState>().m_PrimaryUser;
        BallMovement ball = user.Ball;
        return (
            user.m_hitForce != 0f
            && ball.m_shootButton.m_prevState
            && !ball.m_shootButton.m_state
            && !ball.HasTakenShot
            && ball.IsBallIdle
            && ball.HitGround
        );
    }

    // Performs a shot
    public static void Shoot(float force, Vector3 angle) {
        User user = Services.GameState.CurrentState.Cast<GameplayGameState>().m_PrimaryUser;
        user.HitForce = force;
        user.PlayerCamera.transform.forward = angle;

        BallMovement ball = user.Ball;
        ball.m_shootButton.m_prevState = true;
        ball.m_shootButton.m_state = false;
        ball.HasTakenShot = false;
        ball.IsBallIdle = true;
        ball.HitGround = true;
        ball.CheckForBallHit();
    }
}

[HarmonyPatch(typeof(BallMovement), nameof(BallMovement.CheckForBallHit))]
public static class CheckForBallHitPatch {
    
    [HarmonyPrefix]
    public static bool Prefix(BallMovement __instance) {
        if (ModReuseLastShot.CheckForBallHitCheck()) {

            if (ModReuseLastShot.lockPower) {
                Services.GameState.CurrentState.Cast<GameplayGameState>().m_PrimaryUser.HitForce = ModReuseLastShot.lastPower;
            }
            ModReuseLastShot.lastPower = Services.GameState.CurrentState.Cast<GameplayGameState>().m_PrimaryUser.HitForce;

            if (ModReuseLastShot.lockAngle) {
                Services.GameState.CurrentState.Cast<GameplayGameState>().m_PrimaryUser.PlayerCamera.transform.forward = ModReuseLastShot.lastAngle;
            }
            ModReuseLastShot.lastAngle = Services.GameState.CurrentState.Cast<GameplayGameState>().m_PrimaryUser.PlayerCamera.transform.forward;

        }
        return true;
    }

    [HarmonyPostfix]
    public static void Postfix(BallMovement __instance) {
        
    }
}