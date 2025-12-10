using UnityEngine;

public class RotationOfGameObject : MonoBehaviour
{
    public float rotateSpeed = 5f;

    private Quaternion targetRotation;

    private void Start()
    {
        targetRotation = transform.rotation;
    }

    void Update()
    {
        // 1. Change target rotation based on keys
        if (Input.GetKey(KeyCode.W))
            targetRotation *= Quaternion.Euler(45f * Time.deltaTime, 0, 0);

        if (Input.GetKey(KeyCode.S))
            targetRotation *= Quaternion.Euler(-45f * Time.deltaTime, 0, 0);

        if (Input.GetKey(KeyCode.A))
            targetRotation *= Quaternion.Euler(0, -45f * Time.deltaTime, 0);

        if (Input.GetKey(KeyCode.D))
            targetRotation *= Quaternion.Euler(0, 45f * Time.deltaTime, 0);

        // 2. Smoothly rotate towards the target
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );
    }
}
