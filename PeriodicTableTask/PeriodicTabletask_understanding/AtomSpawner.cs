using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// AtomSpawner: modular spawner for one or many atoms (nucleus, nucleons, rings, electrons, labels).
/// Designed to replace the large SpawnAtoms logic and be called from a higher-level scene controller.
/// </summary>
public class AtomSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject spherePrefab;      // visual nucleus (optional)
    public GameObject ringPrefab;        // particle-system ring prefab (optional)
    public GameObject electronPrefab;
    public GameObject protonPrefab;
    public GameObject neutronPrefab;
    public GameObject labelH2Prefab;

    [Header("Nucleus/Nucleon")]
    public float desiredNucleusScale = 2.0f;
    public float nucleonScale = 0.45f;
    public int maxNucleonsToSpawn = 200;

    [Header("Rings/Electrons")]
    public float baseRingRadius = 2.0f;
    public float ringRadiusStep = 1.0f;
    public float ringStrokeSize = 0.12f;
    public float desiredElectronWorldSize = 0.6f;
    public bool orbitInXZPlane = false;
    public bool randomizeRingTilt = true;
    public float maxTiltDegrees = 30f;

    [Header("Layout")]
    public float elementSpacing = 6.0f;
    public float zSpacing = 4.0f;
    public int maxColumns = 8;

    [Header("Pooling")]
    public bool usePooling = true;
    private readonly Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>();

    // Small internal caches of spawned roots so the caller can later clean up if needed
    private readonly List<GameObject> spawnedRoots = new List<GameObject>();

    // Public API -------------------------------------------------

    /// <summary>
    /// Spawn a grid of atoms based on the elements list, centered at 'center'. Returns the root container.
    /// </summary>
    public GameObject SpawnAtoms(List<ElementData> elements, Vector3 center, Quaternion parentRotation)
    {
        if (elements == null || elements.Count == 0) return null;

        int count = elements.Count;
        int cols = Mathf.CeilToInt(Mathf.Sqrt(count));
        if (maxColumns > 0) cols = Mathf.Min(cols, maxColumns);
        int rows = Mathf.CeilToInt((float)count / cols);

        float halfWidth = (cols - 1) * 0.5f * elementSpacing;
        float halfDepth = (rows - 1) * 0.5f * zSpacing;

        GameObject worldParent = CreateContainer("SpawnedAtomsRoot");
        worldParent.transform.position = center;
        worldParent.transform.rotation = parentRotation;
        spawnedRoots.Add(worldParent);

        for (int idx = 0; idx < count; idx++)
        {
            var element = elements[idx];
            int row = idx / cols;
            int col = idx % cols;
            float x = col * elementSpacing - halfWidth;
            float z = row * zSpacing - halfDepth;
            Vector3 localPos = new Vector3(x, 0f, z);

            GameObject atomGO = CreateContainer(element.symbol + "_Atom");
            atomGO.transform.SetParent(worldParent.transform, false);
            atomGO.transform.localPosition = localPos;
            atomGO.transform.localRotation = Quaternion.identity;

            // spawn nucleus (visual)
            GameObject nucleus = SpawnNucleus(atomGO.transform);

            // ensure nucleus scale
            EnsureNucleusScale(nucleus, desiredNucleusScale);

            // spawn nucleons inside nucleus
            SpawnNucleons(nucleus.transform, element.atomicNumber, element.neutrons);

            // add or get MultiRingElectronController
            var controller = EnsureElectronController(nucleus);

            // create rings and electrons if electronShells present
            if (element.electronShells != null && ringPrefab != null)
            {
                CreateRingsAndElectrons(element, nucleus.transform, controller);
            }

            // floating symbol label
            if (labelH2Prefab != null)
            {
                GameObject labelGO = CreateFromPool("LabelPrefab", labelH2Prefab, atomGO.transform);
                labelGO.name = $"{element.symbol}_Label";
                var text = labelGO.GetComponentInChildren<TMP_Text>();
                if (text != null) text.text = element.symbol;
                labelGO.transform.localPosition = new Vector3(0f, desiredNucleusScale + 0.25f, 0f);
            }
        }

        return worldParent;
    }

    // Helper: spawn nucleus visual (uses spherePrefab if provided)
    GameObject SpawnNucleus(Transform parent)
    {
        GameObject nucleus;
        if (spherePrefab != null)
        {
            nucleus = CreateFromPool("SpherePrefab", spherePrefab, parent);
            nucleus.name = "Nucleus";
            nucleus.transform.localPosition = Vector3.zero;
            nucleus.transform.localRotation = Quaternion.identity;
        }
        else
        {
            nucleus = CreateContainer("Nucleus");
            nucleus.transform.SetParent(parent, false);
            nucleus.transform.localPosition = Vector3.zero;
        }

        // ensure there is a NucleusParent container for nucleons (same convention as original)
        Transform nucleusParent = nucleus.transform.Find("NucleusParent");
        if (nucleusParent == null)
        {
            GameObject newParent = CreateContainer("NucleusParent");
            nucleusParent = newParent.transform;
            nucleusParent.SetParent(nucleus.transform, false);
            nucleusParent.localPosition = Vector3.zero;
        }

        return nucleus;
    }

    // Spawn protons + neutrons, with scaling and random placement inside nucleus radius
    void SpawnNucleons(Transform nucleus, int protonCount, int neutronCount)
    {
        protonCount = Mathf.Max(0, protonCount);
        neutronCount = Mathf.Max(0, neutronCount);
        int total = protonCount + neutronCount;
        if (total == 0) return;

        if (total > maxNucleonsToSpawn)
        {
            float scaleFactor = (float)maxNucleonsToSpawn / total;
            protonCount = Mathf.RoundToInt(protonCount * scaleFactor);
            neutronCount = Mathf.RoundToInt(neutronCount * scaleFactor);
        }

        float nucleusRadius = 0.5f * desiredNucleusScale;
        var rend = nucleus.GetComponentInChildren<Renderer>();
        if (rend != null) nucleusRadius = Mathf.Max(0.01f, rend.bounds.extents.magnitude * 0.9f);

        Transform container = nucleus.Find("NucleusParent");
        if (container == null) container = nucleus; // fallback

        for (int p = 0; p < protonCount; p++)
            SpawnNucleonInside(container, protonPrefab, nucleusRadius, nucleonScale);

        for (int n = 0; n < neutronCount; n++)
            SpawnNucleonInside(container, neutronPrefab, nucleusRadius, nucleonScale);
    }

    // Create rings and spawn electrons attached to them.
    void CreateRingsAndElectrons(ElementData element, Transform nucleusTransform, MultiRingElectronController controller)
    {
        // create rings (particle systems) per shell
        for (int s = 0; s < element.electronShells.Length; s++)
        {
            int electronCount = element.electronShells[s];
            if (electronCount <= 0) continue;

            GameObject ringGO = CreateFromPool("RingPrefab", ringPrefab, nucleusTransform);
            ringGO.name = $"Ring_{s}";
            ringGO.transform.localPosition = Vector3.zero;
            ringGO.transform.localRotation = Quaternion.identity;
            ringGO.transform.localScale = Vector3.one;

            var ps = ringGO.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                try
                {
                    var shape = ps.shape;
                    shape.radius = baseRingRadius + s * ringRadiusStep;
                    var main = ps.main;
                    main.startSize = ringStrokeSize;
                    if (!ps.isPlaying) ps.Play();
                }
                catch { }
            }

            if (randomizeRingTilt)
            {
                float tiltX = UnityEngine.Random.Range(-maxTiltDegrees * 0.6f, maxTiltDegrees * 0.6f);
                float tiltZ = UnityEngine.Random.Range(-maxTiltDegrees * 0.6f, maxTiltDegrees * 0.6f);
                ringGO.transform.rotation = Quaternion.Euler(tiltX, 0f, tiltZ);
            }
        }

        // refresh controller so it can detect rings
        controller.RefreshAllRings();

        // attach electrons to rings
        for (int s = 0; s < element.electronShells.Length; s++)
        {
            int electronCount = element.electronShells[s];
            if (electronCount <= 0) continue;

            var ringPS = controller.GetRingParticleSystem(s);
            if (ringPS == null) continue;

            float angleStep = 360f / Mathf.Max(1, electronCount);
            for (int e = 0; e < electronCount; e++)
            {
                float angle = e * angleStep;
                GameObject electron = CreateFromPool("ElectronPrefab", electronPrefab);
                if (electron == null) continue;

                var marker = electron.GetComponent<ElectronMarker>() ?? electron.AddComponent<ElectronMarker>();
                marker.elementSymbol = element.symbol;
                marker.shellIndex = s;
                marker.angleDeg = angle % 360f;
                marker.transferred = false;
                marker.inTransfer = false;

                controller.AttachElectronToRingAtAngle(ringPS, electron.transform, angle, 0f);
                SetUniformWorldScale(electron.transform, desiredElectronWorldSize);
            }
        }

        controller.RefreshAllRings();
    }

    // ------------------------
    // SMALL UTILITIES (copied & simplified from original)
    // ------------------------

    GameObject CreateFromPool(string key, GameObject prefab = null, Transform parent = null)
    {
        if (!usePooling || prefab == null)
        {
            if (prefab == null)
            {
                var go = new GameObject(key);
                if (parent != null) go.transform.SetParent(parent, false);
                return go;
            }
            var inst = Instantiate(prefab, parent);
            inst.name = prefab.name;
            return inst;
        }

        if (!pool.ContainsKey(key)) pool[key] = new Queue<GameObject>();

        var q = pool[key];
        if (q.Count > 0)
        {
            var go = q.Dequeue();
            if (go == null) return CreateFromPool(key, prefab, parent);
            if (parent != null) go.transform.SetParent(parent, false);
            else go.transform.SetParent(this.transform, false);
            go.SetActive(true);
            var ps = go.GetComponentInChildren<ParticleSystem>(true);
            if (ps != null)
            {
                try { ps.Clear(true); ps.Play(true); }
                catch { }
            }
            return go;
        }
        else
        {
            var inst = Instantiate(prefab, parent);
            inst.name = prefab.name;
            if (inst.GetComponent<PoolMarker>() == null) inst.AddComponent<PoolMarker>();
            var ps = inst.GetComponentInChildren<ParticleSystem>(true);
            if (ps != null)
            {
                try { ps.Clear(true); ps.Play(true); }
                catch { }
            }
            return inst;
        }
    }

    void ReturnToPool(string key, GameObject go)
    {
        if (go == null) return;
        go.SetActive(false);
        if (!pool.ContainsKey(key)) pool[key] = new Queue<GameObject>();
        pool[key].Enqueue(go);
    }

    GameObject CreateContainer(string name)
    {
        var go = new GameObject(name);
        return go;
    }

    void SpawnNucleonInside(Transform nucleusParent, GameObject prefab, float nucleusRadius, float visualScale)
    {
        if (prefab == null || nucleusParent == null) return;
        GameObject go = CreateFromPool(prefab.name, prefab, nucleusParent);
        go.transform.localScale = Vector3.one * visualScale;
        Vector3 localPos = UnityEngine.Random.insideUnitSphere * (nucleusRadius * 0.5f);
        go.transform.localPosition = localPos;
        go.transform.localRotation = UnityEngine.Random.rotation;
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

    MultiRingElectronController EnsureElectronController(GameObject nucleus)
    {
        var controller = nucleus.GetComponent<MultiRingElectronController>();
        if (controller == null) controller = nucleus.AddComponent<MultiRingElectronController>();
        controller.orbitInXZPlane = orbitInXZPlane;
        controller.angularSpeedDegrees = 80f;
        return controller;
    }

    MultiRingElectronController EnsureElectronController(Transform nucleusTransform)
    {
        return EnsureElectronController(nucleusTransform.gameObject);
    }

    // Optional: call this if you want to clean up everything spawned by this spawner
    public void ReturnAllSpawnedToPoolOrDestroy()
    {
        foreach (var root in spawnedRoots)
        {
            if (root == null) continue;
            // naive teardown: walk children and return known keys to pool if we find PoolMarker
            var poolMarkers = root.GetComponentsInChildren<PoolMarker>(true);
            foreach (var pm in poolMarkers)
            {
                if (pm == null) continue;
                var go = pm.gameObject;
                ReturnToPool(go.name, go);
            }
            try { Destroy(root); } catch { }
        }
        spawnedRoots.Clear();
    }
}
