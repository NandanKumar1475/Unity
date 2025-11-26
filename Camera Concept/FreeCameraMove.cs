using UnityEngine;

public class FreeCameraMove : MonoBehaviour
{
    [Header("Rotation")]
    public float mouseSensitivity = 150f;
    float yaw;
    float pitch;

    [Header("Camera Movement")]
    public float normalSpeed = 2f;
    public float speedMultiplier = 2f;

    [Header("Sprint / Vertical Keys")]
    private KeyCode sprintKey = KeyCode.LeftShift;
    private KeyCode cameraUpKey = KeyCode.E;
    private KeyCode cameraDownKey = KeyCode.Q;

    [Header("Smoothing postion")]
    public float movementSmoothTime = 0.12f;
    Vector3 currentVelocity = Vector3.zero;
    Vector3 velocitySmoothRef = Vector3.zero;

    [Header("smoothing direction")]
    public float smoothDirectionTime = 0.08f;
    private Vector3 smoothedDir ;
    Vector3 dirvelocitySmoothRef  = Vector3.zero;

    [Header("FOV")]
    Camera cam;
    private float normalPov = 60f;
    public float sprintPov = 70f;
    public float smoothTimeFOrFov = 0.15f;
    float fovVelocity = 0f;

    [Header("ZoomInZoomOut")]
    float minZoom = 1f;
    float MaxZoom = 80f;
    float zoomSensitivity = 10f;
    float zoomSmoothTime = 0.12f;
    float targetZoomFov ;
    float zoomVelocity ;

    [Header("Tilt Angle While Movement")]
    float maxTilrAngle = 10f;
    float minTiltAngle = 0.1f;
    float currentTilt = 0f;
    float tiltVelocity = 0f;
    float tiltSmoothTime  = 0.02f;




    void Start()
    {
        if(cam == null)
        {
            cam = GetComponent<Camera>();
            normalPov = cam.fieldOfView;
        }
        targetZoomFov = cam.fieldOfView;
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x;
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            MouseRotationHandler();
        }

        CameraMovement();
        HandleFOv();
        ZoomInZommOut();    
    }

    void MouseRotationHandler()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void CameraMovement()
    {
        float right = Input.GetAxisRaw("Horizontal");
        float forward = Input.GetAxisRaw("Vertical");
        float up = 0f;

        if (Input.GetKey(cameraUpKey)) up += 1f;
        if (Input.GetKey(cameraDownKey)) up -= 1f;

        Vector3 inputDir = new Vector3(right, up, forward);
        if (inputDir.sqrMagnitude > 1f)
            inputDir.Normalize();

        // smooth Direction 
        smoothedDir = Vector3.SmoothDamp(
             smoothedDir,
             inputDir,
             ref dirvelocitySmoothRef,
             smoothDirectionTime
            );

        float speed = normalSpeed * (Input.GetKey(sprintKey) ? speedMultiplier : 1f);

        // velocity = diretion * speed ;
        Vector3 targetVelocity = transform.TransformDirection(smoothedDir) * speed;  //-> convert into worldspace
        // accelartion and deaccleartion 
        currentVelocity = Vector3.SmoothDamp(
            currentVelocity,
            targetVelocity,
            ref velocitySmoothRef,
            movementSmoothTime
        );

        transform.position += currentVelocity * Time.deltaTime;
    }

    public void HandleFOv()
    {
         float targetFov = Input.GetKey(sprintKey) ? sprintPov : normalPov;
        float newFov = Mathf.SmoothDamp(
            cam.fieldOfView,
            targetFov,
            ref fovVelocity,
            smoothTimeFOrFov
            );
        cam.fieldOfView = newFov;
    }

    public void ZoomInZommOut()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            targetZoomFov -= scroll * zoomSensitivity;
            targetZoomFov =  Mathf.Clamp( targetZoomFov,minZoom,MaxZoom);
        }

        cam.fieldOfView = Mathf.SmoothDamp(
             cam.fieldOfView,
             targetZoomFov,
             ref zoomVelocity,
             zoomSmoothTime
             );


    }

    public void HandleTIlt()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float targetTilt = horizontal * maxTilrAngle;

        currentTilt = Mathf.SmoothDamp(currentTilt, targetTilt, ref tiltVelocity, tiltSmoothTime);
        transform.rotation =  Quaternion.Euler(pitch, yaw, currentTilt);
    }

    
}
