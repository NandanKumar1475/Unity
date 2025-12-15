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
