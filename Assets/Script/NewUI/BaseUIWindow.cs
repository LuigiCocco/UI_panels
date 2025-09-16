using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(VerticalLayoutGroup))]
public class BaseUIWindow : BaseUI
{
    #region Properties
    [Header("UI Prefabs")]
    public GameObject HeaderPrefab;
    public GameObject FooterPrefab;

    [NonSerialized]
    public List<GameObject> Panels = new List<GameObject>();

    [Header("Panel Settings")]
    public float panelHeight;
    public float panelWidth;

    [Header("Events")]
    public UnityEvent OnPanelsFinished;

    private GameObject _headerInstance;
    private GameObject _footerInstance;
    private GameObject _panelContainer;
    private int _currentPanelIndex = -1;

    #endregion

    #region UnityLifecycle

    private void Awake()
    {
        FindOrCreatePanelContainer();
        AutoLoadPanelsFromChildren();
    }

    private void Start()
    {
        InstantiateSections();
    }

    #endregion

    #region Initialization
    private void FindOrCreatePanelContainer()
    {
        Transform existingPanels = transform.Find("PanelContainer");

        if (existingPanels != null)
        {
            _panelContainer = existingPanels.gameObject;
            RectTransform panelsRect = _panelContainer.GetComponent<RectTransform>();
            panelsRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);
            panelsRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth);
        }
        else
        {
            _panelContainer = new GameObject("PanelContainer");
            _panelContainer.transform.SetParent(transform, false);
            RectTransform panelsRect = _panelContainer.AddComponent<RectTransform>();
            panelsRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);
            panelsRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth);
        }
    }

    private void AutoLoadPanelsFromChildren()
    {
        if (_panelContainer == null) return;

        foreach (Transform child in _panelContainer.transform)
        {
            if (!child.gameObject.activeSelf)
            {
                LoadPanelInternal(child.gameObject);
            }
        }
    }
    #endregion

    #region Layout 
    public void InstantiateSections()
    {
        _headerInstance = null;
        _footerInstance = null;

        if (HeaderPrefab != null)
        {
            _headerInstance = Instantiate(HeaderPrefab, transform);
            _headerInstance.name = "Header";
            _headerInstance.transform.SetAsFirstSibling();
            _headerInstance.SetActive(true);
        }

        if (_panelContainer != null)
        {
            if (_headerInstance != null)
            {
                _panelContainer.transform.SetSiblingIndex(_headerInstance.transform.GetSiblingIndex() + 1);
            }
            else
            {
                _panelContainer.transform.SetAsFirstSibling();
            }
        }

        if (FooterPrefab != null)
        {
            _footerInstance = Instantiate(FooterPrefab, transform);
            _footerInstance.name = "Footer";
            _footerInstance.transform.SetAsLastSibling();
            _footerInstance.SetActive(true);
        }
    }

    private void RefreshHeader()
    {
        if (_headerInstance != null)
        {
            _headerInstance.transform.SetAsFirstSibling();
        }
    }

    private void RefreshFooter()
    {
        if (_footerInstance != null)
        {
            _footerInstance.transform.SetAsLastSibling();
        }
    }

    private void FitPanelToContainer(GameObject panel)
    {
        RectTransform containerRect = _panelContainer.GetComponent<RectTransform>();
        RectTransform panelRect = panel.GetComponent<RectTransform>();

        if (containerRect == null || panelRect == null)
            return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);

        float panelWidth = panelRect.rect.width;
        float panelHeight = panelRect.rect.height;

        float containerWidth = containerRect.rect.width;
        float containerHeight = containerRect.rect.height;

        float widthRatio = containerWidth / panelWidth;
        float heightRatio = containerHeight / panelHeight;
        float scale = Mathf.Min(1f, Mathf.Min(widthRatio, heightRatio));

        panel.transform.localScale = new Vector3(scale, scale, 1);
    }
    #endregion

    #region Panel Navigation
    private void ShowPanelAtIndex(int index)
    {
        if (_panelContainer == null) return;

        // Nasconde tutti i contenuti attivi nel container
        foreach (Transform child in _panelContainer.transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
            }
        }

        // Mostra il contenuto all'indice specificato
        if (index >= 0 && index < Panels.Count)
        {
            var panel = Panels[index];
            panel.SetActive(true);
        }
        RefreshHeader();
        RefreshFooter();
    }

    public void ShowNextPanel()
    {
        if (Panels.Count == 0)
        {
            Debug.LogWarning("Nessun contenuto disponibile. Caricalo tramite LoadPanel.");
            return;
        }

        if (_currentPanelIndex < Panels.Count - 1)
        {
            _currentPanelIndex++;
            ShowPanelAtIndex(_currentPanelIndex);

        }
        else
        {
            Debug.Log("Ultimo contenuto raggiunto.");
            OnPanelsFinished?.Invoke();
        }
    }

    public void ShowPreviousPanel()
    {
        if (Panels.Count == 0)
        {
            Debug.LogWarning("Nessun contenuto disponibile.");
            return;
        }

        if (_currentPanelIndex > 0)
        {
            _currentPanelIndex--;
            ShowPanelAtIndex(_currentPanelIndex);
            RefreshHeader();
            RefreshFooter();
        }
        else
        {
            Debug.Log("Sei già al primo contenuto.");
        }
    }
    #endregion

    #region Panel Loading 
    public void LoadPanel(GameObject panelInstance)
    {
        if (panelInstance == null)
        {
            Debug.LogWarning("Panel null.");
            return;
        }

        // Assicura che esista il container PanelContainer
        if (_panelContainer == null)
        {
            FindOrCreatePanelContainer();
        }

        // Imposta il parent al container PanelContainer
        FitPanelToContainer(panelInstance);
        panelInstance.transform.SetParent(_panelContainer.transform, false);

        // Configura posizione e stretch totale
        if (panelInstance.GetComponent<RectTransform>() != null)
        {
            RectTransform rect = panelInstance.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
        }
        
        panelInstance.SetActive(false);
        LoadPanelInternal(panelInstance);
    }

    private void LoadPanelInternal(GameObject panelInstance)
    {
        Panels.Add(panelInstance);

        if (Panels.Count == 1)
        {
            _currentPanelIndex = 0;
            ShowPanelAtIndex(_currentPanelIndex);
        }
    }

    public void RemovePanel(int index)
    {
        if (index < 0 || index >= Panels.Count) return;

        GameObject panelToRemove = Panels[index];

        if (index == _currentPanelIndex)
        {
            panelToRemove.SetActive(false);
        }

        Panels.RemoveAt(index);

        if (panelToRemove != null)
        {
            DestroyImmediate(panelToRemove);
        }

        if (_currentPanelIndex >= Panels.Count)
        {
            _currentPanelIndex = Panels.Count - 1;
        }

        if (_currentPanelIndex >= 0)
        {
            ShowPanelAtIndex(_currentPanelIndex);
        }
    }
    #endregion
}