using UnityEngine;

[RequireComponent(typeof(Transform))]
public class CubeMoverSmooth : MonoBehaviour
{
    [Header("Movement (tuning)")]
    [Tooltip("Base speed used when moving toward target (units/sec).")]
    public float baseSpeed = 4f;

    [Tooltip("How quickly velocity changes (smaller = snappier).")]
    public float smoothTime = 0.18f;

    [Tooltip("Max distance to consider we've 'arrived'.")]
    public float arriveThreshold = 0.02f;

    [Header("Step distances")]
    [Tooltip("Horizontal step applied when command left/right is given.")]
    public float horizontalStep = 2.5f;

    [Tooltip("Vertical step applied when command up/down is given.")]
    public float verticalStep = 1.0f;

    [Header("Bobbing (while moving right)")]
    [Tooltip("Enable bobbing when moving horizontally to the right.")]
    public bool enableRightBobbing = true;
    public float bobAmplitude = 0.25f;
    public float bobFrequency = 2.2f;

    [Header("Constraints (optional)")]
    public bool clampPosition = true;
    public Vector2 minXZ = new Vector2(-6f, -6f); // (xMin, zMin)
    public Vector2 maxXZ = new Vector2(6f, 6f);   // (xMax, zMax)

    // internals
    private Vector3 _target;
    private Vector3 _velocity = Vector3.zero;
    private bool _isMoving = false;
    private bool _useBobbing = false;
    private float _bobTimer = 0f;

    void Start()
    {
        _target = transform.position;
    }

    void Update()
    {
        // Smoothly move toward target
        // We smooth each axis independently using SmoothDamp to preserve stable motion.
        Vector3 current = transform.position;
        Vector3 targetNoBob = _target;

        // apply bobbing offset (only affects y)
        float bobOffset = 0f;
        if (_useBobbing && enableRightBobbing)
        {
            _bobTimer += Time.deltaTime * bobFrequency;
            bobOffset = Mathf.Sin(_bobTimer) * bobAmplitude;
            targetNoBob.y = _target.y + bobOffset;
        }

        // SmoothDamp for position
        float maxSpeed = Mathf.Infinity;
        Vector3 newPos = new Vector3(
            Mathf.SmoothDamp(current.x, targetNoBob.x, ref _velocity.x, smoothTime, maxSpeed, Time.deltaTime),
            Mathf.SmoothDamp(current.y, targetNoBob.y, ref _velocity.y, smoothTime, maxSpeed, Time.deltaTime),
            Mathf.SmoothDamp(current.z, targetNoBob.z, ref _velocity.z, smoothTime, maxSpeed, Time.deltaTime)
        );

        // clamp optional
        if (clampPosition)
        {
            newPos.x = Mathf.Clamp(newPos.x, minXZ.x, maxXZ.x);
            newPos.z = Mathf.Clamp(newPos.z, minXZ.y, maxXZ.y);
        }

        transform.position = newPos;

        // arrival check
        if ((_target - transform.position).magnitude <= arriveThreshold)
        {
            _isMoving = false;
            _useBobbing = false;
            _bobTimer = 0f;
            // zero velocity to avoid micro-oscillations
            _velocity = Vector3.zero;
        }
        else
        {
            _isMoving = true;
        }
    }

    // Public commands ------------------------------------------------------

    /// <summary>Start a smooth move to the left by horizontalStep.</summary>
    public void CommandLeft()
    {
        _target += Vector3.left * horizontalStep;
        _useBobbing = false;
    }

    /// <summary>Start a smooth move to the right by horizontalStep (and enable bobbing).</summary>
    public void CommandRight()
    {
        _target += Vector3.right * horizontalStep;
        _useBobbing = true;
        // ensure bob starts in-phase
        _bobTimer = 0f;
    }

    /// <summary>Nudge up by verticalStep.</summary>
    public void CommandUp()
    {
        _target += Vector3.up * verticalStep;
    }

    /// <summary>Nudge down by verticalStep.</summary>
    public void CommandDown()
    {
        _target += Vector3.down * verticalStep;
    }

    /// <summary>Stop the cube smoothly by setting target to current position (it will decelerate).</summary>
    public void StopSmooth()
    {
        _target = transform.position;
        _useBobbing = false;
        _velocity = Vector3.zero;
    }

    /// <summary>Optionally move to absolute X,Z position (for 'snap left side' behavior if needed).</summary>
    public void MoveTo(Vector3 absolutePos)
    {
        _target = absolutePos;
    }

    // Helper for debug / UI:
    public Vector3 GetTarget() => _target;
    public bool IsMoving() => _isMoving;
}
