using UnityEngine;

public class HitscanGun : MonoBehaviour
{
    public float range = 100f;          // maximum shooting distance
    public int damage = 20;             // amount of damage
    public Transform muzzle;            // gun muzzle transform (optional)
    public ParticleSystem muzzleFlash;  // optional VFX prefab

    Camera cam;

    void Awake() => cam = Camera.main;

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))   // left mouse / Ctrl
            Fire();
    }

    void Fire()
    {
        // visual flash
        if (muzzleFlash) muzzleFlash.Play();

        // create ray from camera centre (crosshair)
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f)); 
        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 0.2f);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log($"Hit {hit.collider.name} at {hit.point} (distance {hit.distance:F2})");

            // optional damage call
            var hp = hit.collider.GetComponent<Health>();
            if (hp != null)
                hp.ApplyDamage(damage);

            // simple impact mark
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = hit.point + hit.normal * 0.02f;
            sphere.transform.localScale = Vector3.one * 0.05f;
            Destroy(sphere, 0.3f);
        }
    }
}
