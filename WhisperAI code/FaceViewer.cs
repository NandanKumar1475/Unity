void FaceViewer()
{
    if (Camera.main == null) return;

    Vector3 dir = Camera.main.transform.position - target.position;
    dir.y = 0f; // remove this line if you want full 3D facing

    if (dir.sqrMagnitude < 0.001f) return;

    Quaternion lookRot = Quaternion.LookRotation(dir);
    target.rotation = Quaternion.Slerp(
        target.rotation,
        lookRot,
        Time.deltaTime * 10f
    );
}

--------------------------------------------
    ZoomIn and Zoom out
    [Header("Viewer Facing")]
     public bool faceViewer = true;


if (faceViewer)
    FaceViewer();

[Header("Zoom")]
public Camera cam;
public float zoomSpeed = 4f;
public float zoomSmooth = 6f;

Vector3 targetZoomVelocity;
Vector3 currentZoomVelocity;
float zoomTimer;


if (text.Contains("zoom"))
{
    float duration = ExtractDuration(text);

    if (text.Contains("in"))
    {
        targetZoomVelocity = cam.transform.forward * zoomSpeed;
        zoomTimer = duration;
    }
    else if (text.Contains("out"))
    {
        targetZoomVelocity = -cam.transform.forward * zoomSpeed;
        zoomTimer = duration;
    }
}

HANDLE ZOOM SPEED BY VOICE

if (text.Contains("zoom speed"))
{
    var m = System.Text.RegularExpressions.Regex.Match(text, @"zoom speed\s*(\d+(\.\d+)?)");
    if (m.Success)
        zoomSpeed = float.Parse(m.Groups[1].Value);
}






==========================================================
    step by step

   1.Add VARIABLES (at top of your script)
    [Header("Zoom")]
public Camera cam;
public float zoomSpeed = 4f;
public float zoomSmooth = 6f;

Vector3 targetZoomVelocity;
Vector3 currentZoomVelocity;
float zoomTimer;
2.
    STEP 2: Initialize camera (in Awake())


3.ADD ZOOM COMMAND PARSING
(inside your HandleCommand(string text) method)

        // ===== ZOOM =====
if (text.Contains("zoom"))
{
    float duration = ExtractDuration(text);

    if (text.Contains("in"))
    {
        targetZoomVelocity = cam.transform.forward * zoomSpeed;
        zoomTimer = duration;
    }
    else if (text.Contains("out"))
    {
        targetZoomVelocity = -cam.transform.forward * zoomSpeed;
        zoomTimer = duration;
    }
}
This enables:
zoom in
zoom out
zoom in for 2 seconds

STEP 4: ADD ZOOM UPDATE METHOD (Paste anywhere in class)

void HandleZoom()
{
    if (zoomTimer <= 0f)
    {
        targetZoomVelocity = Vector3.zero;
        return;
    }

    zoomTimer -= Time.deltaTime;

    currentZoomVelocity = Vector3.Lerp(
        currentZoomVelocity,
        targetZoomVelocity,
        zoomSmooth * Time.deltaTime
    );

    cam.transform.position += currentZoomVelocity * Time.deltaTime;
}

STEP 5: CALL ZOOM IN Update()
    HandleZoom();

STEP 6: STOP SHOULD ALSO STOP ZOOM
targetZoomVelocity = Vector3.zero;
currentZoomVelocity = Vector3.zero;
zoomTimer = 0f;

STEP 7: SPEED CONTROL (OPTIONAL BUT RECOMMENDED)
    Inside your existing speed handler, add:
    if (text.Contains("zoom speed"))
{
    var m = System.Text.RegularExpressions.Regex.Match(text, @"zoom speed\s*(\d+(\.\d+)?)");
    if (m.Success)
        zoomSpeed = float.Parse(m.Groups[1].Value);
}



