using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Team17;
using UnityEngine;

namespace MyFirstPlugin;

// This mod is experimental
public class ModHoleInOneBruteforcer : MonoBehaviour {
    public ModHoleInOneBruteforcer(IntPtr ptr) : base(ptr) {}

    // Private Static Variables
    private static bool initialized = false;
    private static Rect windowRect = new Rect(400, 600, 250, 150);
    private static GhostBallManager[] ghosts = new GhostBallManager[120];
    private static GameObject[] holes;
    private static GhostBallManager winner = null;
    private static Vector3 resetPosition;

    void Update() {
        if (State.isInGame) {
            if (initialized) {

                for (int i=0; i<120; i++) {

                    // Update physics on GameObject
                    UpdateGhostBall(ghosts[i].GhostBall);

                    // If in hole, set winner and cleanup
                    if (IsGhostBallInHole(ghosts[i].GhostBall)) {
                        winner = ghosts[i];
                        initialized = false;
                        for (int j=0; j<120; j++) {
                            Destroy(ghosts[j].GhostBall);
                        }
                        break;
                    }

                    // If idle, reset and shoot again with more power (first trying 16ths)
                    if (IsGhostBallIdle(ghosts[i].GhostBall)) {
                        ghosts[i].GhostBall.transform.position = resetPosition;
                        ghosts[i].Power += 10500.0f / 16.0f;
                        if (ghosts[i].Power <= 10500.0f) {
                            ShootGhostBall(ghosts[i].GhostBall, ghosts[i].Power, ghosts[i].Angle);
                        } else {
                            // this angle failed, do nothing for now
                        }
                    }


                }

            }
        } else {
            initialized = false;
        }
    }

    void OnGUI() {
        if (State.isInGame && State.isPaused) {
            windowRect = GUI.Window(1004, windowRect, (GUI.WindowFunction)DrawWindow, "Hole In One Finder");
        }
    }

    static void DrawWindow(int id) {
        if (!initialized && GUILayout.Button("Initialize")) {
            Initialize();
        }
        if (winner != null) {
            GUILayout.Label("Winner found!");
            GUILayout.Label("Power: " + winner.Power.ToString());
            GUILayout.Label("Angle: " + winner.Angle.ToString());
        }
        GUI.DragWindow(new Rect(0, 0, 250, 20));
    }

    static void Initialize() {
        for (int i=0; i<120; i++) {
            ghosts[i] = new GhostBallManager(CreateGhostBall(), 0.0f, 3.0f * i);
        }
        holes = GameObject.FindGameObjectsWithTag("Hole").ToArray<GameObject>();
        resetPosition = Services.GameState.CurrentState.Cast<GameplayGameState>().m_PrimaryUser.Ball.gameObject.transform.position;
        winner = null;
        initialized = true;
    }

    // Creates a GhostBall
    // TODO: Create a new ball instead of cloning the player's
    static GameObject CreateGhostBall() {

        // Inactive parent
        GameObject inactive = new GameObject("inactive");
        inactive.SetActive(false);

        // Clone real ball
        GameObject real = Services.GameState.CurrentState.Cast<GameplayGameState>().m_PrimaryUser.Ball.gameObject;
        GameObject clone = Instantiate<GameObject>(real, inactive.transform);

        // Strip away features from ghost
        DestroyImmediate(clone.GetComponent<NetworkBallSync>());
        DestroyImmediate(clone.GetComponent<NetworkInventoryVerifier>());
        DestroyImmediate(clone.GetComponent<PhotonView>());

        // Release clone
        clone.transform.SetParent(null, true);
        Destroy(inactive);

        // Return
        return clone;
    }

    // Shoots a GhostBall
    // void BallMovement$$CheckForBallHit(BallMovement_o *__this,MethodInfo *method)
    static void ShootGhostBall(GameObject ghost, float power, float angle) {

        // Components
        BallMovement bm = ghost.GetComponent<BallMovement>();
        Rigidbody rb = ghost.GetComponent<Rigidbody>();

        // Drag
        rb.angularDrag = 1.0f;
        rb.drag = bm.initialDrag + bm.m_environmentalDragToApply;

        // Calculate magnitude
        // ((pUVar29->fields).m_hitForce - (pTVar16->fields).m_value) * (1.0 / ((pTVar15->fields).m_value - (pTVar16->fields).m_value))
        // ((pUVar29->fields).m_hitForce is power
        // (pTVar16->fields).m_value) is minForce, can be 0
        // (pTVar15->fields).m_value is maxForce, can be 10500
        float magnitude = power * bm.m_PowerCurve.Evaluate( (power - 0.0f) * (1.0f / (10500.0f - 0.0f)) );

        // Shot
        Vector3 force = new Vector3(0.0f, 0.0f, 0.0f);
        force.x = Mathf.Cos(angle * Mathf.Deg2Rad);
        force.z = Mathf.Sin(angle * Mathf.Deg2Rad);
        force *= magnitude + 1.0f;
        rb.AddForce(force, ForceMode.Force);
    }

    // Updates a GhostBall
    // Found through Ghidra
    // More stuff is done here but idk if it's necessary...
    static void UpdateGhostBall(GameObject ghost) {
        
        // Components
        BallMovement bm = ghost.GetComponent<BallMovement>();
        Rigidbody rb = ghost.GetComponent<Rigidbody>();

        // Angular Drag Update
        float speed = bm.currentSpeed;
        if (speed <= 1.0f) {
            rb.angularDrag = (1.0f - speed) * (1.0f - speed) * bm.anglularDragToSlow;
        }
    }

    // Helper Function
    static bool IsGhostBallIdle(GameObject ghost) {
        return (ghost.GetComponent<BallMovement>().currentSpeed <= 0.01f);
    }

    // Helper Function
    static bool IsGhostBallInHole(GameObject ghost) {
        for (int i=0; i<holes.Length; i++) {
            if (holes[i].GetComponent<Collider>().bounds.Contains(ghost.transform.position)) {
                return true;
            }
        }
        return false;
    }
}

// GhostBall Manager
// Container for GhostBall vars
public class GhostBallManager {

    // Vars
    public GameObject GhostBall;
    public float Power;
    public float Angle;

    public GhostBallManager(GameObject GhostBall, float Power, float Angle) {
        this.GhostBall = GhostBall;
        this.Power = Power;
        this.Angle = Angle;
    }

}