using UnityEngine;
// file Handler
public class ElementUIManager : MonoBehaviour
{
    public static ElementUIManager Instance { get; private set; }

    [Header("References")]
    public GameObject elementTilePrefab; // ElementTile prefab
    public RectTransform contentParent;  // Content under ScrollView/Viewport
    public string jsonResourceName = "elements"; // Resources/elements.json

    [Header("Details")]
    public ElementDetailsPanel detailsPanel;

    ElementDatabase db;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        LoadJsonAndPopulate();
    }

    void LoadJsonAndPopulate()
    {
        var ta = Resources.Load<TextAsset>(jsonResourceName);
        if (ta == null)
        {
            Debug.LogError($"Could not find JSON at Resources/{jsonResourceName}.json");
            return;
        }

        db = JsonUtility.FromJson<ElementDatabase>(ta.text);
        if (db == null || db.elements == null || db.elements.Length == 0)
        {
            Debug.LogWarning("Element database empty or failed to parse.");
            return;
        }
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        foreach (var e in db.elements)
        {
            var go = Instantiate(elementTilePrefab, contentParent);
            var tile = go.GetComponent<ElementTileUI>();
            if (tile != null) tile.Setup(e);
            else Debug.LogError("ElementTilePrefab missing ElementTileUI script.");
        }
    }

    public void ShowDetails(ElementData e)
    {
        if (detailsPanel != null) detailsPanel.Show(e);
    }
}
