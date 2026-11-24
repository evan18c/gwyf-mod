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
    private static int totalShots;
    private static int maxShots;

    void Update() {
        if (State.isInGame) {
            if (initialized) {
                for (int i=0; i<120; i++) {

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
                            totalShots++;

                            // we ran out
                            if (totalShots == maxShots) {
                                for (int j=0; j<120; j++) {
                                    Destroy(ghosts[j].GhostBall);
                                }
                                initialized = false;
                                break;
                            }

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

    void FixedUpdate() {
        if (State.isInGame && initialized) {
            for (int i=0; i<120; i++) {
                UpdateGhostBall(ghosts[i].GhostBall);
            }
        }
    }

    void OnGUI() {
        if (State.isInGame && State.isPaused) {
            windowRect = GUI.Window(1004, windowRect, (GUI.WindowFunction)DrawWindow, "Hole In One Finder");
        }
    }

    static void DrawWindow(int id) {
        if (!initialized && GUILayout.Button("Search")) {
            Initialize();
        }
        if (initialized) {
            GUILayout.Label($"Shots {totalShots}/{maxShots}");
        }
        if (winner != null) {
            GUILayout.Label("Winner found!");
            GUILayout.Label($"Power: {winner.Power}");
            GUILayout.Label($"Angle: {winner.Angle}");
            if (GUILayout.Button("Showcase")) {
                ModReuseLastShot.Shoot(winner.Power, new Vector3(Mathf.Cos(winner.Angle * Mathf.Deg2Rad), 0.0f, Mathf.Sin(winner.Angle * Mathf.Deg2Rad)));
            }
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
        totalShots = 0;
        maxShots = 16 * 120;
        initialized = true;
    }

    // Creates a GhostBall
    // maybe tag this as "Player" ?
    // maybe sc stuff changes ?
    static GameObject CreateGhostBall() {

        // Create
        GameObject GhostBall = new GameObject("GhostBall");
        GhostBall.layer = LayerMask.NameToLayer("IgnoreBallCollision");

        // Transform
        GhostBall.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);

        // Rigidbody
        Rigidbody rb = GhostBall.AddComponent<Rigidbody>();
        rb.angularDrag = 1000.0f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.drag = 0.5f;
        rb.inertiaTensor = new Vector3(0.002f, 0.002f, 0.002f);
        rb.interpolation = RigidbodyInterpolation.Extrapolate;
        rb.maxAngularVelocity = 10000.0f;

        // SphereCollider
        SphereCollider sc = GhostBall.AddComponent<SphereCollider>();
        sc.radius = 1.0f;
        sc.contactOffset = 0.0005f;
        sc.material.bounciness = 0.8f;
        sc.material.bouncyness = 0.8f;
        sc.material.dynamicFriction = 0.01f;
        sc.material.staticFriction = 0.0f;

        // Return
        return GhostBall;
    }

    // Shoots a GhostBall
    static void ShootGhostBall(GameObject ghost, float power, float angle) {

        // Rigidbody
        Rigidbody rb = ghost.GetComponent<Rigidbody>();

        // Angular Drag
        rb.angularDrag = 1.0f;

        // Magnitude (this appears to just be the power)
        float magnitude = power;

        // Shot
        Vector3 force = new Vector3(0.0f, 0.0f, 0.0f);
        force.x = Mathf.Cos(angle * Mathf.Deg2Rad);
        force.z = Mathf.Sin(angle * Mathf.Deg2Rad);
        force *= magnitude + 1.0f;
        rb.AddForce(force, ForceMode.Force);
    }

    // Updates a GhostBall
    static void UpdateGhostBall(GameObject ghost) {
        
        // Components
        Rigidbody rb = ghost.GetComponent<Rigidbody>();

        // Angular Drag Update
        float speed = rb.velocity.magnitude;
        if (speed <= 1.0f) {
            rb.angularDrag = (1.0f - speed) * (1.0f - speed) * 40.0f;
        }
    }

    // Helper Function
    static bool IsGhostBallIdle(GameObject ghost) {
        return (ghost.GetComponent<Rigidbody>().velocity.magnitude <= 0.01f);
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