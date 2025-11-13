using UnityEngine;

// Simple rotation of a transform to simulate orbit
public class SimpleOrbit : MonoBehaviour
{
    public float rotationSpeed = 30f;
    void Update()
    {
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}
