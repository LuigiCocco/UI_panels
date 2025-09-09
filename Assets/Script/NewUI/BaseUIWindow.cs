using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Windows
{
    /// <summary>
    /// Gestore principale per finestre UI con supporto per header, footer e pannelli dinamici.
    /// Richiede un Canvas per funzionare correttamente.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class BaseUIWindow : MonoBehaviour
    {
        #region Fields & Properties

        [Header("Window Configuration")]
        [SerializeField] private string windowId = "MainWindow";
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool destroyPanelsOnSwitch = true;

        [Header("Layout References")]
        [SerializeField] private RectTransform windowContainer;
        [SerializeField] private RectTransform headerContainer;
        [SerializeField] private RectTransform panelContainer;
        [SerializeField] private RectTransform footerContainer;

        [Header("Prefabs")]
        [SerializeField] private GameObject headerPrefab;
        [SerializeField] private GameObject footerPrefab;
        [SerializeField] private List<BaseUIPanel> panelPrefabs = new();

        [Header("Layout Settings")]
        [SerializeField] private bool useVerticalLayout = true;
        [SerializeField] private float layoutSpacing = 10f;
        [SerializeField] private RectOffset layoutPadding = new RectOffset(10, 10, 10, 10);

        // Private fields
        private Canvas windowCanvas;
        private CanvasScaler canvasScaler;
        private Dictionary<int, BaseUIPanel> cachedPanels = new();
        private BaseUIPanel currentPanel;
        private int currentPanelIndex = -1;
        private GameObject headerInstance;
        private GameObject footerInstance;

        // Properties
        public string WindowId => windowId;
        public BaseUIPanel CurrentPanel => currentPanel;
        public int CurrentPanelIndex => currentPanelIndex;
        public int PanelCount => panelPrefabs?.Count ?? 0;
        public bool HasPanels => PanelCount > 0;
        public Canvas WindowCanvas => windowCanvas;

        // Events
        public event Action<BaseUIPanel, int> OnPanelChanged;
        public event Action<BaseUIPanel> OnPanelDestroyed;
        public event Action OnWindowInitialized;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            ValidateComponents();

            if (autoInitialize)
            {
                Initialize();
            }
        }

        private void Start()
        {
            if (autoInitialize && HasPanels)
            {
                ShowPanel(0);
            }
        }

        private void OnDestroy()
        {
            ClearAllPanels();
            DestroyHeaderAndFooter();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Inizializza la finestra UI e costruisce la struttura base
        /// </summary>
        public void Initialize()
        {
            SetupCanvas();
            CreateWindowStructure();
            BuildHeaderAndFooter();
            OnWindowInitialized?.Invoke();
        }

        private void ValidateComponents()
        {
            windowCanvas = GetComponent<Canvas>();
            canvasScaler = GetComponent<CanvasScaler>();

            if (windowCanvas == null)
            {
                Debug.LogError($"[{windowId}] Canvas component is missing!");
                return;
            }

            // Se non è specificato un container principale, usa il RectTransform del GameObject
            if (windowContainer == null)
            {
                windowContainer = GetComponent<RectTransform>();
            }
        }

        private void SetupCanvas()
        {
            // Configura il Canvas se necessario
            if (windowCanvas.renderMode == RenderMode.WorldSpace)
            {
                Debug.LogWarning($"[{windowId}] Canvas is set to WorldSpace. Consider using ScreenSpace for UI windows.");
            }

            // Assicurati che il sorting order sia appropriato
            if (windowCanvas.overrideSorting == false)
            {
                windowCanvas.overrideSorting = true;
                windowCanvas.sortingOrder = 10;
            }
        }

        private void CreateWindowStructure()
        {
            // Crea la struttura base se non esiste
            if (headerContainer == null)
            {
                headerContainer = CreateContainer("Header", windowContainer);
                SetupLayoutElement(headerContainer, preferredHeight: 60f);
            }

            if (panelContainer == null)
            {
                panelContainer = CreateContainer("Content", windowContainer);
                SetupLayoutElement(panelContainer, flexibleHeight: 1f);
            }

            if (footerContainer == null)
            {
                footerContainer = CreateContainer("Footer", windowContainer);
                SetupLayoutElement(footerContainer, preferredHeight: 50f);
            }

            // Configura il layout principale
            SetupMainLayout();
        }

        private RectTransform CreateContainer(string name, Transform parent)
        {
            GameObject container = new GameObject(name, typeof(RectTransform));
            RectTransform rect = container.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            // Configura l'ancoraggio per riempire il parent
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            return rect;
        }

        private void SetupMainLayout()
        {
            if (!windowContainer.GetComponent<VerticalLayoutGroup>() && useVerticalLayout)
            {
                VerticalLayoutGroup vlg = windowContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                vlg.spacing = layoutSpacing;
                vlg.padding = layoutPadding;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
            }
        }

        private void SetupLayoutElement(RectTransform rect, float preferredHeight = -1, float flexibleHeight = -1)
        {
            LayoutElement layoutElement = rect.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = rect.gameObject.AddComponent<LayoutElement>();
            }

            if (preferredHeight > 0)
                layoutElement.preferredHeight = preferredHeight;

            if (flexibleHeight > 0)
                layoutElement.flexibleHeight = flexibleHeight;
        }

        #endregion

        #region Header & Footer Management

        private void BuildHeaderAndFooter()
        {
            if (headerPrefab != null && headerContainer != null)
            {
                SetSection(headerPrefab, headerContainer, ref headerInstance);
            }

            if (footerPrefab != null && footerContainer != null)
            {
                SetSection(footerPrefab, footerContainer, ref footerInstance);
            }
        }

        private void SetSection(GameObject prefab, RectTransform container, ref GameObject instance)
        {
            if (prefab == null || container == null) return;

            // Distruggi l'istanza precedente se esiste
            if (instance != null)
            {
                DestroyImmediate(instance);
            }

            // Pulisci il container
            ClearContainer(container);

            // Istanzia il nuovo prefab
            instance = Instantiate(prefab, container);
            instance.name = prefab.name;

            // Configura il RectTransform per riempire il container
            RectTransform rect = instance.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;
            }
        }

        public void UpdateHeader(GameObject newHeaderPrefab)
        {
            headerPrefab = newHeaderPrefab;
            SetSection(headerPrefab, headerContainer, ref headerInstance);
        }

        public void UpdateFooter(GameObject newFooterPrefab)
        {
            footerPrefab = newFooterPrefab;
            SetSection(footerPrefab, footerContainer, ref footerInstance);
        }

        private void DestroyHeaderAndFooter()
        {
            if (headerInstance != null)
            {
                DestroyImmediate(headerInstance);
                headerInstance = null;
            }

            if (footerInstance != null)
            {
                DestroyImmediate(footerInstance);
                footerInstance = null;
            }
        }

        #endregion

        #region Panel Management

        /// <summary>
        /// Mostra un pannello specifico tramite indice
        /// </summary>
        public void ShowPanel(int index)
        {
            if (!ValidatePanelIndex(index)) return;

            // Se è lo stesso pannello, non fare nulla
            if (index == currentPanelIndex && currentPanel != null) return;

            // Gestisci il pannello precedente
            HandlePreviousPanel();

            // Aggiorna l'indice corrente
            currentPanelIndex = index;

            // Controlla se abbiamo già il pannello in cache
            if (!destroyPanelsOnSwitch && cachedPanels.TryGetValue(index, out BaseUIPanel cached))
            {
                currentPanel = cached;
                currentPanel.gameObject.SetActive(true);
            }
            else
            {
                // Crea un nuovo pannello
                CreateNewPanel(index);
            }

            // Notifica il cambio di pannello
            OnPanelChanged?.Invoke(currentPanel, index);

            // Forza l'aggiornamento del layout
            ForceLayoutUpdate();
        }

        /// <summary>
        /// Mostra un pannello tramite tipo
        /// </summary>
        public void ShowPanel<T>() where T : BaseUIPanel
        {
            int index = panelPrefabs.FindIndex(p => p is T);
            if (index >= 0)
            {
                ShowPanel(index);
            }
            else
            {
                Debug.LogWarning($"[{windowId}] Panel of type {typeof(T).Name} not found in prefabs list");
            }
        }

        /// <summary>
        /// Mostra il pannello successivo nella lista
        /// </summary>
        public void ShowNextPanel()
        {
            if (!HasPanels) return;
            int nextIndex = (currentPanelIndex + 1) % PanelCount;
            ShowPanel(nextIndex);
        }

        /// <summary>
        /// Mostra il pannello precedente nella lista
        /// </summary>
        public void ShowPreviousPanel()
        {
            if (!HasPanels) return;
            int prevIndex = currentPanelIndex - 1;
            if (prevIndex < 0) prevIndex = PanelCount - 1;
            ShowPanel(prevIndex);
        }

        private void HandlePreviousPanel()
        {
            if (currentPanel == null) return;

            if (destroyPanelsOnSwitch)
            {
                OnPanelDestroyed?.Invoke(currentPanel);
                DestroyImmediate(currentPanel.gameObject);
                currentPanel = null;
            }
            else
            {
                // Nasconde il pannello invece di distruggerlo
                currentPanel.gameObject.SetActive(false);
            }
        }

        private void CreateNewPanel(int index)
        {
            BaseUIPanel prefab = panelPrefabs[index];
            if (prefab == null)
            {
                Debug.LogError($"[{windowId}] Panel prefab at index {index} is null!");
                return;
            }

            currentPanel = Instantiate(prefab, panelContainer);
            currentPanel.name = $"{prefab.name}_Instance";

            // Configura il RectTransform
            RectTransform panelRect = currentPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.sizeDelta = Vector2.zero;
                panelRect.anchoredPosition = Vector2.zero;
            }

            
            if (!destroyPanelsOnSwitch)
            {
                cachedPanels[index] = currentPanel;
            }
            
        }

        /// <summary>
        /// Pulisce il pannello corrente
        /// </summary>
        public void ClearCurrentPanel()
        {
            if (currentPanel != null)
            {
                OnPanelDestroyed?.Invoke(currentPanel);
                DestroyImmediate(currentPanel.gameObject);
                currentPanel = null;
                currentPanelIndex = -1;
            }
        }

        /// <summary>
        /// Pulisce tutti i pannelli cached
        /// </summary>
        public void ClearAllPanels()
        {
            ClearCurrentPanel();

            foreach (var panel in cachedPanels.Values)
            {
                if (panel != null)
                {
                    OnPanelDestroyed?.Invoke(panel);
                    DestroyImmediate(panel.gameObject);
                }
            }

            cachedPanels.Clear();
        }

        #endregion

        #region Utility Methods

        private bool ValidatePanelIndex(int index)
        {
            if (panelPrefabs == null || panelPrefabs.Count == 0)
            {
                Debug.LogWarning($"[{windowId}] No panel prefabs configured!");
                return false;
            }

            if (index < 0 || index >= panelPrefabs.Count)
            {
                Debug.LogWarning($"[{windowId}] Panel index {index} is out of range (0-{panelPrefabs.Count - 1})");
                return false;
            }

            if (panelPrefabs[index] == null)
            {
                Debug.LogError($"[{windowId}] Panel prefab at index {index} is null!");
                return false;
            }

            return true;
        }

        private void ClearContainer(Transform container)
        {
            if (container == null) return;

            for (int i = container.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(container.GetChild(i).gameObject);
            }
        }

        private void ForceLayoutUpdate()
        {
            Canvas.ForceUpdateCanvases();

            if (panelContainer != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(panelContainer);
            }

            if (windowContainer != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(windowContainer);
            }
        }

        /// <summary>
        /// Ottiene un pannello specifico dalla cache (se esiste)
        /// </summary>
        public T GetPanel<T>() where T : BaseUIPanel
        {
            return cachedPanels.Values.FirstOrDefault(p => p is T) as T;
        }

        /// <summary>
        /// Verifica se un pannello è attualmente visibile
        /// </summary>
        public bool IsPanelActive(int index)
        {
            return currentPanelIndex == index && currentPanel != null && currentPanel.gameObject.activeSelf;
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        [ContextMenu("Force Initialize Window")]
        private void ForceInitialize()
        {
            ValidateComponents();
            Initialize();
        }

        [ContextMenu("Clear All Panels")]
        private void EditorClearAllPanels()
        {
            ClearAllPanels();
        }

        private void OnValidate()
        {
            if (windowContainer == null)
            {
                windowContainer = GetComponent<RectTransform>();
            }
        }
#endif

        #endregion
    }
}