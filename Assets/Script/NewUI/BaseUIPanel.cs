using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(VerticalLayoutGroup))]
public class BaseUIPanel : BaseUI
{
    [Header("UI Prefabs")]
    public GameObject HeaderPrefab;
    public List<GameObject> ContentPrefab;
    public GameObject FooterPrefab;

    [Header("Impostazioni Iniziali")]
    public bool InizializzaAllAvvio = true;
    [Header("Events")]
    public UnityEvent OnContentFinished;

    private void Start()
    {
        if (InizializzaAllAvvio)
        {
            InstantiateSections();
        }
        else
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void InstantiateSections()
    {
        if (HeaderPrefab != null)
        {
            var header = Instantiate(HeaderPrefab, transform);
            header.name = "Header";
            header.SetActive(true);
        }

        if (ContentPrefab != null)
        {
            foreach (var contentObj in ContentPrefab)
            {
                if (contentObj != null)
                {
                    var content = Instantiate(contentObj, transform);
                    content.name = "Content";
                    content.SetActive(true);
                }
            }
        }

        if (FooterPrefab != null)
        {
            var footer = Instantiate(FooterPrefab, transform);
            footer.name = "Footer";
            footer.SetActive(true);
        }
    }

    public void LoadContent(GameObject contentPrefab)
    {
        if (contentPrefab != null)
        {
            ContentPrefab.Add(contentPrefab);
            var content = Instantiate(contentPrefab, transform);
            content.name = "Content";
            content.SetActive(true);
        }
    }

    public void RemoveContent(int index)
    {
        if (ContentPrefab != null && index >= 0 && index < ContentPrefab.Count)
        {       
            int contentInstanceIndex = 0;
            foreach (Transform child in transform)
            {
                if (child.name == "Content")
                {
                    if (contentInstanceIndex == index)
                    {
                        Destroy(child.gameObject);
                        break;
                    }
                    contentInstanceIndex++;
                }
            }
            ContentPrefab.RemoveAt(index);
        }
    }
}
