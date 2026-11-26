using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Team17;
using UnityEngine;

namespace MyFirstPlugin;

// This mod is experimental
public class ModHoleInOneBruteforcer : MonoBehaviour {
    public ModHoleInOneBruteforcer(IntPtr ptr) : base(ptr) {}

    // Mod
    private static bool initialized = false;
    private static Rect windowRect = new Rect(400, 600, 250, 600);

    // Objects
    private static GhostBallManager[] ghosts;
    private static Vector3 resetPosition;
    private static GameObject[] holes;

    // Results
    private static GhostBallManager winner = null;
    private static int totalShots;
    private static int maxShots;

    // Config Fields
    private static string totalGhostsField = "120";
    private static string minAngleField = "0";
    private static string maxAngleField = "360";
    private static string powerStepsField = "16";
    private static string minPowerField = "0";
    private static string maxPowerField = "10500";

    // Config
    private static int totalGhosts = 120;
    private static float minAngle = 0.0f;
    private static float maxAngle = 360.0f;
    private static int powerSteps = 16;
    private static float minPower = 0.0f;
    private static float maxPower = 10500.0f;

    void Update() {
        if (State.isInGame) {
            if (initialized) {
                for (int i=0; i<totalGhosts; i++) {

                    // If in hole, set winner and cleanup
                    if (IsGhostBallInHole(ghosts[i])) {
                        winner = ghosts[i];
                        initialized = false;
                        for (int j=0; j<totalGhosts; j++) {
                            Destroy(ghosts[j].GhostBall);
                        }
                        break;
                    }

                    // If idle, reset and shoot again with more power (first trying 16ths)
                    if (IsGhostBallIdle(ghosts[i])) {
                        ghosts[i].GhostBall.transform.position = resetPosition;
                        ghosts[i].Power += (maxPower - minPower) / (float)powerSteps;
                        ghosts[i].rb.velocity = Vector3.zero;
                        ghosts[i].rb.angularVelocity = Vector3.zero;
                        if (ghosts[i].Power <= maxPower) {
                            ShootGhostBall(ghosts[i], ghosts[i].Power, ghosts[i].Angle);
                            totalShots++;
                        } else {
                            if (totalShots == maxShots) {
                                for (int j=0; j<totalGhosts; j++) {
                                    Destroy(ghosts[j].GhostBall);
                                }
                                initialized = false;
                                break;
                            }
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
            for (int i=0; i<totalGhosts; i++) {
                UpdateGhostBall(ghosts[i]);
            }
        }
    }

    void OnGUI() {
        if (State.isInGame && State.isPaused) {
            windowRect = GUI.Window(1004, windowRect, (GUI.WindowFunction)DrawWindow, "Hole In One Finder");
        }
    }

    static void DrawWindow(int id) {
        if (!initialized) {
            GUILayout.Label("Total Ghosts:");
            totalGhostsField = GUILayout.TextField(totalGhostsField);
            GUILayout.Label("Angle Bound Min:");
            minAngleField = GUILayout.TextField(minAngleField);
            GUILayout.Label("Angle Bound Max:");
            maxAngleField = GUILayout.TextField(maxAngleField);
            GUILayout.Label("Power Steps:");
            powerStepsField = GUILayout.TextField(powerStepsField);
            GUILayout.Label("Power Bound Min:");
            minPowerField = GUILayout.TextField(minPowerField);
            GUILayout.Label("Power Bound Max:");
            maxPowerField = GUILayout.TextField(maxPowerField);
        }
        if (!initialized && GUILayout.Button("Search")) {
            totalGhosts = int.Parse(totalGhostsField);
            minAngle = float.Parse(minAngleField);
            maxAngle = float.Parse(maxAngleField);
            powerSteps = int.Parse(powerStepsField);
            minPower = float.Parse(minPowerField);
            maxPower = float.Parse(maxPowerField);
            Initialize();
        }
        if (initialized) {
            GUILayout.Label($"Shots {totalShots}/{maxShots}");
            if (GUILayout.Button("Stop")) {
                initialized = false;
                for (int i=0; i<totalGhosts; i++) {
                    Destroy(ghosts[i].GhostBall);
                }
            }
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
        ghosts = new GhostBallManager[totalGhosts];
        for (int i=0; i<totalGhosts; i++) {
            ghosts[i] = new GhostBallManager(CreateGhostBall(), minPower, minAngle + i * ((maxAngle - minAngle) / totalGhosts));
        }
        holes = GameObject.FindGameObjectsWithTag("Hole").ToArray<GameObject>();
        resetPosition = Services.GameState.CurrentState.Cast<GameplayGameState>().m_PrimaryUser.Ball.gameObject.transform.position;
        winner = null;
        totalShots = 0;
        maxShots = powerSteps * totalGhosts;
        initialized = true;
    }

    // Creates a GhostBall
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
        rb.inertiaTensor = new Vector3(0.00196f, 0.00196f, 0.00196f);
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
    // NOTE: When this function shoots, take note of animation frame of all objects in the world to recreate it.
    static void ShootGhostBall(GhostBallManager ghost, float power, float angle) {

        // Contact Offset
        ghost.sc.contactOffset = 0.000005f;

        // Angular Drag
        ghost.rb.angularDrag = 1.0f;

        // Magnitude (this appears to just be the power)
        float magnitude = power;

        // Shot
        Vector3 force = new Vector3(0.0f, 0.0f, 0.0f);
        force.x = Mathf.Cos(angle * Mathf.Deg2Rad);
        force.z = Mathf.Sin(angle * Mathf.Deg2Rad);
        force *= magnitude + 1.0f;
        ghost.rb.AddForce(force, ForceMode.Force);
    }

    // Updates a GhostBall
    // TODO: If the ball collides with a reset trigger, flag it for reset.
    static void UpdateGhostBall(GhostBallManager ghost) {
        float speed = ghost.rb.velocity.magnitude;
        if (speed <= 1.0f) {
            ghost.rb.angularDrag = (1.0f - speed) * (1.0f - speed) * 40.0f;
            ghost.sc.contactOffset = 0.0005f;
        } else {
            ghost.rb.angularDrag = 1.0f;
            ghost.sc.contactOffset = 0.000005f;
        }
    }

    // Helper Function
    static bool IsGhostBallIdle(GhostBallManager ghost) {
        return (ghost.rb.velocity.magnitude <= 0.01f);
    }

    // Helper Function
    static bool IsGhostBallInHole(GhostBallManager ghost) {
        for (int i=0; i<holes.Length; i++) {
            if (holes[i].GetComponent<Collider>().bounds.Contains(ghost.GhostBall.transform.position)) {
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

    // Components
    public Rigidbody rb;
    public SphereCollider sc;

    public GhostBallManager(GameObject GhostBall, float Power, float Angle) {
        this.GhostBall = GhostBall;
        this.Power = Power;
        this.Angle = Angle;
        this.rb = GhostBall.GetComponent<Rigidbody>();
        this.sc = GhostBall.GetComponent<SphereCollider>();
    }

}