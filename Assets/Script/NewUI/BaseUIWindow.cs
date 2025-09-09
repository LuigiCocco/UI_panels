using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Windows
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class BaseUIWindow : MonoBehaviour
    {
        private RectTransform windowContainer;
        private RectTransform headerContainer;
        private RectTransform panelContainer;
        private RectTransform footerContainer;

        [Header("Prefabs")]
        [SerializeField] private GameObject headerPrefab;
        [SerializeField] private GameObject footerPrefab;

        private List<GameObject> panels = new();
        private int currentPanelIndex = -1;
        private GameObject headerInstance;
        private GameObject footerInstance;
        private GameObject currentPanelInstance;

        public void LoadPanel(GameObject panel)
        {
            if (panel == null) return;
            panel.SetActive(false);
            panels.Add(panel);
            if (panels.Count == 1)
            {
                currentPanelIndex = 0;
                ShowPanel(currentPanelIndex);
            }
        }

        private void Start()
        {
            InstantiateSections();
        }



        public void RemovePanel(int index)
        {
            if (index < 0 || index >= panels.Count) return;
            if (index == currentPanelIndex)
            {
                DestroyCurrentPanelInstance();
            }
            panels.RemoveAt(index);
            if (currentPanelIndex >= panels.Count)
                currentPanelIndex = panels.Count - 1;
            if (currentPanelIndex >= 0)
                ShowPanel(currentPanelIndex);
        }

        private void InstantiateSections()
        {
            DestroyImmediate(headerInstance);
            DestroyImmediate(footerInstance);
            DestroyCurrentPanelInstance();

            if (headerPrefab != null && headerContainer != null)
            {
                headerInstance = Instantiate(headerPrefab, headerContainer);
                Stretch(headerInstance.GetComponent<RectTransform>());
            }

            if (footerPrefab != null && footerContainer != null)
            {
                footerInstance = Instantiate(footerPrefab, footerContainer);
                Stretch(footerInstance.GetComponent<RectTransform>());
            }

            if (currentPanelIndex >= 0 && currentPanelIndex < panels.Count)
            {
                ShowPanel(currentPanelIndex);
            }
        }

        public void ShowPanel(int index)
        {
            if (index < 0 || index >= panels.Count) return;

            DestroyCurrentPanelInstance();

            currentPanelIndex = index;
            var panel = panels[index];

            panel.transform.SetParent(panelContainer, false);
            panel.SetActive(true);
            currentPanelInstance = panel;
        }

        public void ShowNextPanel()
        {
            if (panels.Count == 0) return;
            int next = (currentPanelIndex + 1) % panels.Count;
            ShowPanel(next);
        }

        public void ShowPreviousPanel()
        {
            if (panels.Count == 0) return;
            int prev = (currentPanelIndex - 1 + panels.Count) % panels.Count;
            ShowPanel(prev);
        }

        private void DestroyCurrentPanelInstance()
        {
            if (currentPanelInstance != null)
            {
                DestroyImmediate(currentPanelInstance);
                currentPanelInstance = null;
            }
        }

        private void Stretch(RectTransform rect)
        {
            if (rect == null) return;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }
    }
}
