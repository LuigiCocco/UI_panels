using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(VerticalLayoutGroup))]
public class BaseUIPanel : MonoBehaviour
{
    [Header("UI Prefabs")]
    public GameObject HeaderPrefab;
    public GameObject ContentPrefab;
    public GameObject FooterPrefab;

    private void Start()
    {
        InstantiateSections();
    }

    public void InstantiateSections()
    {
        if (HeaderPrefab != null)
            Instantiate(HeaderPrefab, transform).name = "Header";

        if (ContentPrefab != null)
            Instantiate(ContentPrefab, transform).name = "Content";

        if (FooterPrefab != null)
            Instantiate(FooterPrefab, transform).name = "Footer";
    }
}
