using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseUIWindow : MonoBehaviour
{
    [Header("Window UI References")]
    public RectTransform WindowHeader;
    public RectTransform WindowFooter;
    public RectTransform PanelContainer;

    [Header("Optional Prefabs")]
    public GameObject HeaderPrefab;
    public GameObject FooterPrefab;

    [Header("Available Panel Prefabs")]
    public List<GameObject> PanelPrefabs = new();

    private BaseUIPanel currentPanel;
    private int currentPanelIndex = -1;

    private void Start()
    {
        BuildWindowStructure();

        if (PanelPrefabs.Count > 0)
        {
            ShowPanel(0); // Mostra il primo pannello
        }
    }

    private void BuildWindowStructure()
    {
        if (HeaderPrefab != null)
            SetSection(HeaderPrefab, WindowHeader);

        if (FooterPrefab != null)
            SetSection(FooterPrefab, WindowFooter);
    }

    private void SetSection(GameObject prefab, RectTransform container)
    {
        if (prefab == null || container == null) return;

        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        GameObject instance = Instantiate(prefab, container);
        instance.transform.SetAsLastSibling();
    }

    /// <summary>
    /// Mostra il pannello all'indice specificato
    /// </summary>
    public void ShowPanel(int index)
    {
        if (PanelPrefabs == null || PanelPrefabs.Count == 0)
        {
            Debug.LogWarning("PanelPrefabs list is empty.");
            return;
        }

        if (index < 0 || index >= PanelPrefabs.Count)
        {
            Debug.LogWarning($"Index {index} is out of range.");
            return;
        }

        currentPanelIndex = index;

        // Rimuove pannello attuale
        if (currentPanel != null)
        {
            Destroy(currentPanel.gameObject);
            currentPanel = null;
        }

        // Instanzia nuovo pannello
        GameObject panelObj = Instantiate(PanelPrefabs[index], PanelContainer);
        currentPanel = panelObj.GetComponent<BaseUIPanel>();

        if (currentPanel != null)
        {
            currentPanel.InstantiateSections();
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(PanelContainer);
    }

    public void ShowNextPanel()
    {
        if (PanelPrefabs == null || PanelPrefabs.Count == 0) return;

        int nextIndex = (currentPanelIndex + 1) % PanelPrefabs.Count;
        ShowPanel(nextIndex);
    }

    public void ClearPanel()
    {
        if (currentPanel != null)
        {
            Destroy(currentPanel.gameObject);
            currentPanel = null;
        }
    }

    public BaseUIPanel GetCurrentPanel()
    {
        return currentPanel;
    }

    public int GetCurrentPanelIndex()
    {
        return currentPanelIndex;
    }
}
