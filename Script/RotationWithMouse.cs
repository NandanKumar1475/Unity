using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Selectable + Manipulable object controller using the new Input System.
/// Attach this script to an empty GameObject (manager) in the scene.
/// Set the input actions in the inspector:
///   - pointerPositionAction -> Pointer/position (Vector2)
///   - pointerDeltaAction    -> Pointer/delta    (Vector2)
///   - leftPressAction       -> <Mouse>/leftButton (Button)
///   - rightPressAction      -> <Mouse>/rightButton (Button)
///
/// Usage:
///   - Click (left) on a collider to select that object (it will become target).
///   - Left-drag -> move selected object.
///   - Right-drag -> rotate selected object.
/// Selected objects should have a Collider (any) and a Transform. Optionally give them tag "Selectable".
/// </summary>
public class SelectableRotatable_InputSystem : MonoBehaviour
{
    [Header("Input Actions (assign from Input Actions asset)")]
    public InputActionReference pointerPositionAction; // Vector2 screen pos
    public InputActionReference pointerDeltaAction;    // Vector2 delta
    public InputActionReference leftPressAction;       // Button
    public InputActionReference rightPressAction;      // Button

    [Header("Raycast / Selection")]
    [Tooltip("Camera used for raycasting. If null, Camera.main will be used.")]
    public Camera raycastCamera;
    [Tooltip("Optional LayerMask to limit raycast to selectable layers (default: Everything)")]
    public LayerMask selectableLayers = ~0;
    [Tooltip("Only select objects with this tag if not empty. Leave blank to select any collider.")]
    public string selectableTag = "Selectable";

    [Header("Movement Settings")]
    public float moveSensitivity = 0.01f;
    public bool verticalControlsForward = false;
    public float moveSmoothTime = 0.03f;
    public float forwardClamp = 5f;

    [Header("Rotation Settings")]
    public float rotateSensitivity = 0.2f;
    public bool invertY = false;
    public float rotateSmoothTime = 0.03f;
    public Vector2 pitchClamp = new Vector2(-80f, 80f);

    [Header("Selection Visual (optional)")]
    [Tooltip("Optional: if assigned, will enable/disable this GameObject as a visual marker when selection changes.")]
    public GameObject selectionMarkerPrefab;

    // internal state
    private Transform selected;             // currently selected object's transform
    private GameObject selectionMarkerInst; // instance of marker prefab
    private Vector2 pointerPosition;        // screen space pointer position
    private Vector2 pointerDelta;           // pointer delta
    private bool leftDown = false;
    private bool rightDown = false;
    private Coroutine moveCoroutine;
    private Coroutine rotateCoroutine;

    // per-object stored start values for move clamping
    private Vector3 selectedStartLocalPosition;
    private float selectedStartForward;
    private Vector3 moveVelocity = Vector3.zero;
    private Vector2 rotateVelocity = Vector2.zero;
    private Vector2 rotateTargetEuler = Vector2.zero;
    private Vector2 currentEuler = Vector2.zero;

    private void Awake()
    {
        if (raycastCamera == null) raycastCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (pointerPositionAction != null) { pointerPositionAction.action.Enable(); pointerPositionAction.action.performed += OnPointerPosition; }
        if (pointerDeltaAction != null) { pointerDeltaAction.action.Enable(); pointerDeltaAction.action.performed += OnPointerDelta; }
        if (leftPressAction != null) { leftPressAction.action.Enable(); leftPressAction.action.started += OnLeftPressed; leftPressAction.action.canceled += OnLeftReleased; }
        if (rightPressAction != null) { rightPressAction.action.Enable(); rightPressAction.action.started += OnRightPressed; rightPressAction.action.canceled += OnRightReleased; }
    }

    private void OnDisable()
    {
        if (pointerPositionAction != null) { pointerPositionAction.action.performed -= OnPointerPosition; pointerPositionAction.action.Disable(); }
        if (pointerDeltaAction != null) { pointerDeltaAction.action.performed -= OnPointerDelta; pointerDeltaAction.action.Disable(); }
        if (leftPressAction != null) { leftPressAction.action.started -= OnLeftPressed; leftPressAction.action.canceled -= OnLeftReleased; leftPressAction.action.Disable(); }
        if (rightPressAction != null) { rightPressAction.action.started -= OnRightPressed; rightPressAction.action.canceled -= OnRightReleased; rightPressAction.action.Disable(); }
    }

    // Input callbacks
    private void OnPointerPosition(InputAction.CallbackContext ctx)
    {
        pointerPosition = ctx.ReadValue<Vector2>();
    }

    private void OnPointerDelta(InputAction.CallbackContext ctx)
    {
        pointerDelta = ctx.ReadValue<Vector2>();
    }

    private void OnLeftPressed(InputAction.CallbackContext ctx)
    {
        leftDown = true;

        // Selection happens on press: perform a raycast from the pointer position into the world
        TrySelectAtPointer();

        // If we have a selected object start move coroutine
        if (selected != null)
        {
            // store starting values for clamping
            selectedStartLocalPosition = selected.localPosition;
            selectedStartForward = selectedStartLocalPosition.z;
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(MoveSelectedCoroutine());
        }
    }

    private void OnLeftReleased(InputAction.CallbackContext ctx)
    {
        leftDown = false;
        pointerDelta = Vector2.zero;
        if (moveCoroutine != null) { StopCoroutine(moveCoroutine); moveCoroutine = null; }
    }

    private void OnRightPressed(InputAction.CallbackContext ctx)
    {
        rightDown = true;

        // Only start rotation if an object is selected
        if (selected != null)
        {
            // initialize rotation targets from current rotation
            Vector3 e = selected.localEulerAngles;
            e.x = NormalizeAngle(e.x); // normalize to -180..180 so clamping works
            rotateTargetEuler = new Vector2(e.x, e.y);
            currentEuler = rotateTargetEuler;
            if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
            rotateCoroutine = StartCoroutine(RotateSelectedCoroutine());
        }
    }

    private void OnRightReleased(InputAction.CallbackContext ctx)
    {
        rightDown = false;
        pointerDelta = Vector2.zero;
        if (rotateCoroutine != null) { StopCoroutine(rotateCoroutine); rotateCoroutine = null; }
    }

    /// <summary>
    /// Raycast from pointerPosition and select the hit object (if allowed).
    /// </summary>
    private void TrySelectAtPointer()
    {
        if (raycastCamera == null) return;

        // convert screen pos (Vector2) to Ray
        Ray ray = raycastCamera.ScreenPointToRay(pointerPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayers, QueryTriggerInteraction.Ignore))
        {
            // If selectableTag is set, only accept objects with that tag. Otherwise accept any collider.
            Transform hitTransform = hit.transform;
            if (!string.IsNullOrEmpty(selectableTag))
            {
                if (hitTransform.CompareTag(selectableTag))
                {
                    SetSelected(hitTransform);
                }
                else
                {
                    // clicked something not selectable: deselect
                    ClearSelection();
                }
            }
            else
            {
                SetSelected(hitTransform);
            }
        }
        else
        {
            // nothing hit: clear selection
            ClearSelection();
        }
    }

    /// <summary>
    /// Mark a transform as selected (stores state and shows marker).
    /// </summary>
    private void SetSelected(Transform t)
    {
        if (selected == t) return; // already selected

        selected = t;

        // instantiate or move marker
        if (selectionMarkerPrefab != null)
        {
            if (selectionMarkerInst == null)
            {
                selectionMarkerInst = Instantiate(selectionMarkerPrefab, selected.position, Quaternion.identity);
            }
            selectionMarkerInst.transform.SetParent(selected, true); // keep marker linked to selected
            selectionMarkerInst.SetActive(true);
        }

        // record starting position for movement clamping
        selectedStartLocalPosition = selected.localPosition;
        selectedStartForward = selectedStartLocalPosition.z;

        // prepare rotation state
        Vector3 e = selected.localEulerAngles;
        e.x = NormalizeAngle(e.x);
        currentEuler = new Vector2(e.x, e.y);
    }

    private void ClearSelection()
    {
        selected = null;
        if (selectionMarkerInst != null) selectionMarkerInst.SetActive(false);
    }

    // Coroutine that applies movement to the selected object while left mouse is held
    private IEnumerator MoveSelectedCoroutine()
    {
        moveVelocity = Vector3.zero;
        Vector3 targetPos = selected.localPosition;

        while (leftDown && selected != null)
        {
            // compute local delta from pointer movement
            Vector3 localDelta = Vector3.zero;
            // use pointerDelta.x -> left/right, pointerDelta.y -> up/down or forward/back depending on flag
            localDelta.x = pointerDelta.x * moveSensitivity * Time.deltaTime * 100f;
            if (verticalControlsForward)
                localDelta.z = pointerDelta.y * moveSensitivity * Time.deltaTime * 100f;
            else
                localDelta.y = pointerDelta.y * moveSensitivity * Time.deltaTime * 100f;

            // apply local delta in parent's space so localPosition update is consistent
            targetPos += localDelta;

            // clamp forward distance relative to start
            targetPos.z = Mathf.Clamp(targetPos.z, selectedStartForward - forwardClamp, selectedStartForward + forwardClamp);

            if (moveSmoothTime > 0f)
            {
                selected.localPosition = Vector3.SmoothDamp(selected.localPosition, targetPos, ref moveVelocity, moveSmoothTime, Mathf.Infinity, Time.deltaTime);
            }
            else
            {
                selected.localPosition = targetPos;
            }

            yield return null;
        }
    }

    // Coroutine that applies rotation to the selected object while right mouse is held
    private IEnumerator RotateSelectedCoroutine()
    {
        rotateVelocity = Vector2.zero;

        while (rightDown && selected != null)
        {
            float yawDelta = pointerDelta.x * rotateSensitivity * Time.deltaTime * 100f;
            float pitchDelta = (invertY ? 1f : -1f) * pointerDelta.y * rotateSensitivity * Time.deltaTime * 100f;

            rotateTargetEuler.y += yawDelta;
            rotateTargetEuler.x += pitchDelta;
            rotateTargetEuler.x = Mathf.Clamp(rotateTargetEuler.x, pitchClamp.x, pitchClamp.y);

            if (rotateSmoothTime > 0f)
            {
                float newX = Mathf.SmoothDamp(currentEuler.x, rotateTargetEuler.x, ref rotateVelocity.x, rotateSmoothTime, Mathf.Infinity, Time.deltaTime);
                float newY = Mathf.SmoothDamp(currentEuler.y, rotateTargetEuler.y, ref rotateVelocity.y, rotateSmoothTime, Mathf.Infinity, Time.deltaTime);
                currentEuler.x = newX;
                currentEuler.y = newY;
            }
            else
            {
                currentEuler = rotateTargetEuler;
            }

            selected.localEulerAngles = new Vector3(currentEuler.x, currentEuler.y, 0f);

            yield return null;
        }
    }

    // Helpers
    private float NormalizeAngle(float a)
    {
        a %= 360f;
        if (a > 180f) a -= 360f;
        return a;
    }
}
