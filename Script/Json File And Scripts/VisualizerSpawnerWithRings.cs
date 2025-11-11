using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VisualizerSpawnerWithRings : MonoBehaviour
{
    public GameObject spherePrefab;
    public GameObject ringPrefab;
    public GameObject electronPrefab;
    public GameObject protonPrefab;
    public GameObject neutronPrefab;
    public GameObject labelH2Prefab;

    public float desiredNucleusScale = 2.0f;
    public float nucleonScale = 0.45f;
    public int maxNucleonsToSpawn = 200;

    public float baseRingRadius = 2.0f;
    public float ringRadiusStep = 1.0f;
    public float elementSpacing = 6.0f;
    public float zSpacing = 4.0f;
    public bool orbitInXZPlane = false;

    public float ringStrokeSize = 0.12f;

    public bool randomizeRingTilt = true;
    public float maxTiltDegrees = 30f;
    public int seedForDeterminism = 0;

    public float desiredElectronWorldSize = 0.6f;

    [Header("Layout / Camera")]
    public int maxColumns = 8;
    public float spawnDistanceFromCamera = 6f;
    public bool autoFrameCamera = true;
    public float cameraFramePadding = 1.15f;

    private List<GameObject> spawnedAtoms = new List<GameObject>();

    void Start()
    {
        var selected = SelectionDataTransfer.selectedElements;
        if (selected == null || selected.Count == 0)
        {
            Debug.LogWarning("[VisualizerSpawner] No elements received from previous scene!");
            return;
        }

        if (seedForDeterminism != 0) Random.InitState(seedForDeterminism);

        Vector3 spawnCenter = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        if (Camera.main != null)
        {
            Camera cam = Camera.main;
            spawnCenter = cam.transform.position + cam.transform.forward * spawnDistanceFromCamera;
            spawnCenter.y = cam.transform.position.y - 0.5f;
            spawnRotation = Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f);
        }

        GameObject root = SpawnAtoms(selected, spawnCenter, spawnRotation);

        if (autoFrameCamera && Camera.main != null && root != null)
        {
            Bounds b = ComputeBoundsOfChildren(root.transform);
            AdjustCameraToFitBounds(Camera.main, b, cameraFramePadding);
        }

        SelectionDataTransfer.Clear();
    }

    GameObject SpawnAtoms(List<ElementData> elements, Vector3 center, Quaternion parentRotation)
    {
        int count = elements.Count;
        if (count == 0) return null;

        int cols = Mathf.CeilToInt(Mathf.Sqrt(count));
        if (maxColumns > 0) cols = Mathf.Min(cols, maxColumns);
        int rows = Mathf.CeilToInt((float)count / cols);

        float halfWidth = (cols - 1) * 0.5f * elementSpacing;
        float halfDepth = (rows - 1) * 0.5f * zSpacing;

        GameObject worldParent = new GameObject("SpawnedAtomsRoot");
        worldParent.transform.position = center;
        worldParent.transform.rotation = parentRotation;
        spawnedAtoms.Add(worldParent);

        for (int idx = 0; idx < count; idx++)
        {
            var element = elements[idx];
            int row = idx / cols;
            int col = idx % cols;
            float x = col * elementSpacing - halfWidth;
            float z = row * zSpacing - halfDepth;
            Vector3 localPos = new Vector3(x, 0f, z);

            GameObject atomGO = new GameObject(element.symbol + "_Atom");
            atomGO.transform.SetParent(worldParent.transform, false);
            atomGO.transform.localPosition = localPos;
            atomGO.transform.localRotation = Quaternion.identity;
            spawnedAtoms.Add(atomGO);

            GameObject nucleus;
            if (spherePrefab != null)
            {
                nucleus = Instantiate(spherePrefab, atomGO.transform);
                nucleus.name = "Nucleus";
                nucleus.transform.localPosition = Vector3.zero;
                nucleus.transform.localRotation = Quaternion.identity;
            }
            else
            {
                nucleus = new GameObject("Nucleus");
                nucleus.transform.SetParent(atomGO.transform, false);
            }

            EnsureNucleusScale(nucleus, desiredNucleusScale);

            Transform nucleusParent = nucleus.transform.Find("NucleusParent");
            if (nucleusParent == null)
            {
                GameObject newParent = new GameObject("NucleusParent");
                nucleusParent = newParent.transform;
                nucleusParent.SetParent(nucleus.transform, false);
                nucleusParent.localPosition = Vector3.zero;
            }

            float nucleusRadius = 0.5f * desiredNucleusScale;
            var rend = nucleus.GetComponentInChildren<Renderer>();
            if (rend != null)
                nucleusRadius = rend.bounds.extents.magnitude * 0.9f;

            int protonCount = Mathf.Max(0, element.atomicNumber);
            int neutronCount = Mathf.Max(0, element.neutrons);
            int totalNucleons = protonCount + neutronCount;

            if (totalNucleons > maxNucleonsToSpawn && totalNucleons > 0)
            {
                float scaleFactor = (float)maxNucleonsToSpawn / totalNucleons;
                protonCount = Mathf.RoundToInt(protonCount * scaleFactor);
                neutronCount = Mathf.RoundToInt(neutronCount * scaleFactor);
            }

            for (int p = 0; p < protonCount; p++)
                SpawnNucleonInside(nucleusParent, protonPrefab, nucleusRadius, nucleonScale);

            for (int n = 0; n < neutronCount; n++)
                SpawnNucleonInside(nucleusParent, neutronPrefab, nucleusRadius, nucleonScale);

            var controller = nucleus.GetComponent<MultiRingElectronController>();
            if (controller == null) controller = nucleus.AddComponent<MultiRingElectronController>();
            controller.orbitInXZPlane = orbitInXZPlane;
            controller.angularSpeedDegrees = 80f;

            if (element.electronShells != null)
            {
                for (int i = 0; i < element.electronShells.Length; i++)
                {
                    int electronCount = element.electronShells[i];
                    if (electronCount <= 0) continue;

                    GameObject ringGO = Instantiate(ringPrefab, nucleus.transform);
                    ringGO.name = $"Ring_{i}";
                    ringGO.transform.localPosition = Vector3.zero;
                    ringGO.transform.localRotation = Quaternion.identity;
                    ringGO.transform.localScale = Vector3.one;

                    var ps = ringGO.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        var shape = ps.shape;
                        shape.radius = baseRingRadius + i * ringRadiusStep;
                        var main = ps.main;
                        main.startSize = ringStrokeSize;
                        if (!ps.isPlaying) ps.Play();
                    }

                    if (randomizeRingTilt)
                    {
                        float tiltX = Random.Range(-maxTiltDegrees * 0.6f, maxTiltDegrees * 0.6f);
                        float tiltZ = Random.Range(-maxTiltDegrees * 0.6f, maxTiltDegrees * 0.6f);
                        ringGO.transform.rotation = Quaternion.Euler(tiltX, 0f, tiltZ);
                    }
                }

                controller.RefreshAllRings();

                for (int i = 0; i < element.electronShells.Length; i++)
                {
                    int electronCount = element.electronShells[i];
                    if (electronCount <= 0) continue;

                    var ringPS = controller.GetRingParticleSystem(i);
                    if (ringPS == null) continue;

                    float angleStep = 360f / Mathf.Max(1, electronCount);
                    for (int e = 0; e < electronCount; e++)
                    {
                        float angle = e * angleStep;
                        GameObject electron = Instantiate(electronPrefab);
                        controller.AttachElectronToRingAtAngle(ringPS, electron.transform, angle, 0f);
                        SetUniformWorldScale(electron.transform, desiredElectronWorldSize);
                    }
                }

                controller.RefreshAllRings();
            }

            if (labelH2Prefab != null)
            {
                GameObject labelGO = Instantiate(labelH2Prefab, atomGO.transform);
                labelGO.name = $"{element.symbol}_Label";
                var text = labelGO.GetComponentInChildren<TMP_Text>();
                if (text != null)
                    text.text = element.symbol;
            }
        }

        return worldParent;
    }

    void SpawnNucleonInside(Transform nucleusParent, GameObject prefab, float nucleusRadius, float visualScale)
    {
        if (prefab == null) return;
        GameObject go = Instantiate(prefab, nucleusParent);
        go.transform.localScale = Vector3.one * visualScale;
        Vector3 localPos = Random.insideUnitSphere * (nucleusRadius * 0.5f);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Random.rotation;
    }

    void EnsureNucleusScale(GameObject nucleus, float desiredScale)
    {
        if (nucleus == null) return;
        nucleus.transform.localScale = Vector3.one * desiredScale;
    }

    void SetUniformWorldScale(Transform child, float desiredWorldSize)
    {
        if (child == null) return;
        Transform parent = child.parent;
        if (parent == null)
        {
            child.localScale = Vector3.one * desiredWorldSize;
            return;
        }

        Vector3 lossy = parent.lossyScale;
        float maxParentScale = Mathf.Max(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y), Mathf.Abs(lossy.z));
        if (maxParentScale <= 1e-5f) maxParentScale = 1f;
        float local = desiredWorldSize / maxParentScale;
        child.localScale = Vector3.one * local;
    }

    Bounds ComputeBoundsOfChildren(Transform root)
    {
        var rends = root.GetComponentsInChildren<Renderer>();
        if (rends == null || rends.Length == 0) return new Bounds(root.position, Vector3.one * 0.1f);

        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        return b;
    }

    void AdjustCameraToFitBounds(Camera cam, Bounds bounds, float padding = 1.1f)
    {
        if (cam == null) return;
        if (bounds.size == Vector3.zero) return;

        float radius = bounds.extents.magnitude * padding;

        if (cam.orthographic)
        {
            cam.orthographicSize = radius;
            cam.transform.LookAt(bounds.center);
            return;
        }

        float fov = cam.fieldOfView * Mathf.Deg2Rad;
        float neededDistance = radius / Mathf.Sin(fov * 0.5f);
        Vector3 dirToCamera = (cam.transform.position - bounds.center).normalized;
        if (dirToCamera == Vector3.zero) dirToCamera = -cam.transform.forward;

        Vector3 newCamPos = bounds.center + dirToCamera * neededDistance;
        cam.transform.position = newCamPos;
        cam.transform.LookAt(bounds.center);
    }
}
