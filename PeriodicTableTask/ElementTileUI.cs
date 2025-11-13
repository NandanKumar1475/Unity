
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ElementTileUI : MonoBehaviour
{
    public TMP_Text symbolText;
    public TMP_Text nameText;
    public TMP_Text atomicNumberText;
    public Image backgroundImage;
    public Button tileButton;
    public Button detailsButton;
    public Button addButton;
    public Button removeButton;
    public GameObject selectionBadgeGO;
    public TMP_Text selectionBadgeText;

    ElementData boundElement;

    public void Setup(ElementData e)
    {
        boundElement = e;
        if (symbolText != null) symbolText.text = e.symbol;
        if (nameText != null) nameText.text = e.name;
        if (atomicNumberText != null) atomicNumberText.text = e.atomicNumber.ToString();

        if (backgroundImage != null && !string.IsNullOrEmpty(e.uiColorHex))
        {
            if (ColorUtility.TryParseHtmlString(e.uiColorHex, out var c))
                backgroundImage.color = c;
        }

        if (tileButton != null)
        {
            tileButton.onClick.RemoveAllListeners();
            tileButton.onClick.AddListener(OnTileClicked_AddOne);
        }

        if (addButton != null)
        {
            addButton.onClick.RemoveAllListeners();
            addButton.onClick.AddListener(OnAddButton);
        }

        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(OnRemoveButton);
        }

        if (detailsButton != null)
        {
            detailsButton.onClick.RemoveAllListeners();
            detailsButton.onClick.AddListener(OnTileClicked_Details);
        }

        SelectionManager.Instance?.RegisterElementData(e);
        SetSelectionBadge(0);
    }

    public void OnTileClicked_AddOne()
    {
        if (boundElement == null) return;
        SelectionManager.Instance?.AddOne(boundElement, this);
    }

    public void OnAddButton()
    {
        OnTileClicked_AddOne();
    }

    public void OnRemoveButton()
    {
        if (boundElement == null) return;
        SelectionManager.Instance?.RemoveOne(boundElement, this);
    }

    public void OnTileClicked_Details()
    {
        if (ElementUIManager.Instance != null)
            ElementUIManager.Instance.ShowDetails(boundElement);
    }

    public void SetSelectionBadge(int count)
    {
        if (selectionBadgeGO != null) selectionBadgeGO.SetActive(count > 0);
        if (selectionBadgeText != null) selectionBadgeText.text = count > 0 ? count.ToString() : "";
    }
}
