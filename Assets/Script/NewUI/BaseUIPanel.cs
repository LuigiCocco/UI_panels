using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(VerticalLayoutGroup))]
public class BaseUIPanel : BaseUI
{
    #region Properties
    [Header("UI Prefabs")]
    public GameObject HeaderPrefab;
    public GameObject FooterPrefab;

    [NonSerialized]
    public List<GameObject> Content = new List<GameObject>();

    [Header("Content Settings")]
    public float contentHeight;
    public float contentWidth;

    [Header("Events")]
    public UnityEvent OnContentFinished;

    private GameObject _headerInstance;
    private GameObject _footerInstance;
    private GameObject _contentContainer;
    private int _currentContentIndex = -1;

    #endregion

    #region UnityLifecycle

    private void Awake()
    {
        FindOrCreateContentContainer();
        AutoLoadContentsFromChildren();
    }

    private void Start()
    {
        InstantiateSections();
    }

    #endregion

    #region Initialization
    private void FindOrCreateContentContainer()
    {
        Transform existingContent = transform.Find("Content");

        if (existingContent != null)
        {
            _contentContainer = existingContent.gameObject;
            RectTransform contentRect = _contentContainer.GetComponent<RectTransform>();
            contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
            contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
        }
        else
        {
            _contentContainer = new GameObject("Content");
            _contentContainer.transform.SetParent(transform, false);
            RectTransform contentRect = _contentContainer.AddComponent<RectTransform>();
            contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
            contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
        }
    }

    private void AutoLoadContentsFromChildren()
    {
        if (_contentContainer == null) return;

        foreach (Transform child in _contentContainer.transform)
        {
            if (!child.gameObject.activeSelf)
            {
                LoadContentInternal(child.gameObject);
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

        if (_contentContainer != null)
        {
            if (_headerInstance != null)
            {
                _contentContainer.transform.SetSiblingIndex(_headerInstance.transform.GetSiblingIndex() + 1);
            }
            else
            {
                _contentContainer.transform.SetAsFirstSibling();
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

    private void FitContentToContainer(GameObject content)
    {
        RectTransform containerRect = _contentContainer.GetComponent<RectTransform>();
        RectTransform contentRect = content.GetComponent<RectTransform>();

        if (containerRect == null || contentRect == null)
            return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

        float contentWidth = contentRect.rect.width;
        float contentHeight = contentRect.rect.height;

        float containerWidth = containerRect.rect.width;
        float containerHeight = containerRect.rect.height;

        float widthRatio = containerWidth / contentWidth;
        float heightRatio = containerHeight / contentHeight;
        float scale = Mathf.Min(1f, Mathf.Min(widthRatio, heightRatio));

        content.transform.localScale = new Vector3(scale, scale, 1);
    }
    #endregion

    #region Content Navigation
    private void ShowContentAtIndex(int index)
    {
        if (_contentContainer == null) return;

        // Nasconde tutti i contenuti attivi nel container
        foreach (Transform child in _contentContainer.transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
            }
        }

        // Mostra il contenuto all'indice specificato
        if (index >= 0 && index < Content.Count)
        {
            var content = Content[index];
            content.SetActive(true);
        }
        RefreshHeader();
        RefreshFooter();
    }

    public void ShowNextContent()
    {
        if (Content.Count == 0)
        {
            Debug.LogWarning("Nessun contenuto disponibile. Caricalo tramite LoadContent.");
            return;
        }

        if (_currentContentIndex < Content.Count - 1)
        {
            _currentContentIndex++;
            ShowContentAtIndex(_currentContentIndex);
            
        }
        else
        {
            Debug.Log("Ultimo contenuto raggiunto.");
            OnContentFinished?.Invoke();
        }
    }

    public void ShowPreviousContent()
    {
        if (Content.Count == 0)
        {
            Debug.LogWarning("Nessun contenuto disponibile.");
            return;
        }

        if (_currentContentIndex > 0)
        {
            _currentContentIndex--;
            ShowContentAtIndex(_currentContentIndex);
            RefreshHeader();
            RefreshFooter();
        }
        else
        {
            Debug.Log("Sei già al primo contenuto.");
        }
    }
    #endregion

    #region Content Loading 
    public void LoadContent(GameObject contentInstance)
    {
        if (contentInstance == null)
        {
            Debug.LogWarning("Content null.");
            return;
        }

        // Assicura che esista il container Content
        if (_contentContainer == null)
        {
            FindOrCreateContentContainer();
        }

        // Imposta il parent al container Content
        FitContentToContainer(contentInstance);
        contentInstance.transform.SetParent(_contentContainer.transform, false);

        // Configura posizione e stretch totale
        if (contentInstance.GetComponent<RectTransform>() != null)
        {
            RectTransform rect = contentInstance.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
        }

        contentInstance.SetActive(false);
        LoadContentInternal(contentInstance);
    }

    private void LoadContentInternal(GameObject contentInstance)
    {
        Content.Add(contentInstance);

        if (Content.Count == 1)
        {
            _currentContentIndex = 0;
            ShowContentAtIndex(_currentContentIndex);
        }
    }

    public void RemoveContent(int index)
    {
        if (index < 0 || index >= Content.Count) return;

        GameObject contentToRemove = Content[index];

        if (index == _currentContentIndex)
        {
            contentToRemove.SetActive(false);
        }

        Content.RemoveAt(index);

        if (contentToRemove != null)
        {
            DestroyImmediate(contentToRemove);
        }

        if (_currentContentIndex >= Content.Count)
        {
            _currentContentIndex = Content.Count - 1;
        }

        if (_currentContentIndex >= 0)
        {
            ShowContentAtIndex(_currentContentIndex);
        }
    }
    #endregion
}