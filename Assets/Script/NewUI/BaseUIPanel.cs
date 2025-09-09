using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(VerticalLayoutGroup))]
public class BaseUIPanel : BaseUI
{
    [Header("UI Prefabs")]
    public GameObject HeaderPrefab;
    public GameObject FooterPrefab;

    [Header("Runtime Content")]
    public List<GameObject> Content = new List<GameObject>();

    [Header("Events")]
    public UnityEvent OnContentFinished;

    private GameObject _headerInstance;
    private GameObject _footerInstance;
    private int _currentContentIndex = -1;

    private void Start()
    {
        InstantiateSections();
    }

    public void InstantiateSections()
    {

        _headerInstance = null;
        _footerInstance = null;

        if (HeaderPrefab != null)
        {
            _headerInstance = Instantiate(HeaderPrefab, transform);
            _headerInstance.name = "Header";
            _headerInstance.SetActive(true);
        }

        if (Content.Count > 0)
        {
            _currentContentIndex = 0;
            ShowContentAtIndex(_currentContentIndex);
        }

        if (FooterPrefab != null)
        {
            _footerInstance = Instantiate(FooterPrefab, transform);
            _footerInstance.name = "Footer";
            _footerInstance.SetActive(true);
        }
    }

    private void Awake()
    {
        AutoLoadContentsFromChildren();
    }

    private void AutoLoadContentsFromChildren()
    {
        foreach (Transform child in transform)
        {
            if (!child.gameObject.activeSelf)
            {
                LoadContent(child.gameObject);
            }
        }
    }

    private void ShowContentAtIndex(int index)
    {


        if (index >= 0 && index < Content.Count)
        {
            var content = Content[index];
            content.transform.SetParent(transform, false);
            content.name = "Content";
            content.SetActive(true);
        }
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
        }
        else
        {
            Debug.Log("Sei già al primo contenuto.");
        }
    }

    public void LoadContent(GameObject contentInstance)
    {
        if (contentInstance == null)
        {
            Debug.LogWarning("Content null.");
            return;
        }

        contentInstance.SetActive(false);
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

        if (index == _currentContentIndex)
        {
            foreach (Transform child in transform)
            {
                if (child.name == "Content")
                {
                    Destroy(child.gameObject);
                    break;
                }
            }
        }

        Content.RemoveAt(index);

        if (_currentContentIndex >= Content.Count)
        {
            _currentContentIndex = Content.Count - 1;
        }

        if (_currentContentIndex >= 0)
        {
            ShowContentAtIndex(_currentContentIndex);
        }
    }
}
