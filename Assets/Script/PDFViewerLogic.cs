using Paroxe.PdfRenderer; // Namespace dell'asset PDF Renderer
using System; // Per IntPtr, se necessario
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PDFViewerLogic : BaseUIPanel
{
    [SerializeField] private PDFAsset pdfAsset; // Seleziona il PDFAsset da Inspector
    private RawImage _pdfDisplay; // RawImage UI per mostrare la pagina
    private Button _prevButton; // Bottone "Pagina Precedente"
    private Button _nextButton; // Bottone "Pagina Successiva"
    private TMP_InputField _pageInput; // InputField per inserire il numero pagina (1-based)

    private PDFDocument pdfDocument; // Il documento PDF caricato
    private int currentPageIndex = 0; // Indice pagina corrente (0-based)
    private int totalPages = 0; // Numero totale pagine

    private void Awake()
    {
        AutoBindComponents();
    }

    void Start()
    {
        _pdfDisplay = Content[0].GetComponent<RawImage>();
        // Carica il PDF dal PDFAsset selezionato in Inspector
        if (pdfAsset == null)
        {
            Debug.LogError("Nessun PDFAsset selezionato!");
            return;
        }

        try
        {
            // Correzione: Usa la proprietà m_FileContent di PDFAsset per ottenere i byte del PDF
            byte[] pdfBytes = pdfAsset.m_FileContent; // Accede ai byte del contenuto del PDF

            // Usa il costruttore con bytes per evitare il cast a IntPtr
            pdfDocument = new PDFDocument(pdfBytes); // Questo dovrebbe funzionare senza errore IntPtr

            // Alternativa se l'asset usa un metodo statico: pdfDocument = PDFDocument.Load(pdfAsset);
            // O se richiede path: string pdfPath = UnityEditor.AssetDatabase.GetAssetPath(pdfAsset); pdfDocument = new PDFDocument(pdfPath);

            totalPages = pdfDocument.GetPageCount();

            if (totalPages == 0)
            {
                Debug.LogError("PDF vuoto o non valido!");
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Errore nel caricamento del PDF: " + e.Message + "\nStack: " + e.StackTrace);
            return;
        }

        // Setup listener per i bottoni
        if (_prevButton != null) _prevButton.onClick.AddListener(OnPreviousPage);
        if (_nextButton != null) _nextButton.onClick.AddListener(OnNextPage);
        if (_pageInput != null) _pageInput.onEndEdit.AddListener(OnGoToPage);

        // Mostra la prima pagina e aggiorna i bottoni
        RenderPage(currentPageIndex);
        UpdateButtons();
    }

    // Funzione per renderizzare una pagina specifica
    private void RenderPage(int pageIndex)
    {
        if (pdfDocument == null || pageIndex < 0 || pageIndex >= totalPages) return;

        try
        {
            PDFPage pdfPage = pdfDocument.GetPage(pageIndex);

            // Replace the following line:
            // PxSize pageSize = pdfPage.GetSize(); // PxSize è il tipo dell'asset per width/height

            // With this corrected code:
            Vector2 pageSize = pdfPage.GetPageSize(); // GetPageSize() restituisce un Vector2 con width e height
            int renderWidth = Mathf.RoundToInt(pageSize.x * 1.5f); // Scala per qualità (DPI approssimativo)
            int renderHeight = Mathf.RoundToInt(pageSize.y * 1.5f);

            // Crea la texture
            Texture2D pageTexture = new Texture2D(renderWidth, renderHeight, TextureFormat.RGBA32, false);

            // Usa il renderer dell'asset (metodo corretto: RenderPage o RenderToTexture)
            PDFRenderer renderer = new PDFRenderer();
            renderer.RenderPageToExistingTexture(pdfPage, pageTexture);

            // Applica al RawImage
            _pdfDisplay.texture = pageTexture;

            // Aggiorna UI
            currentPageIndex = pageIndex;
            if (_pageInput != null) _pageInput.text = (currentPageIndex + 1).ToString();

            UpdateButtons();
        }
        catch (Exception e)
        {
            Debug.LogError("Errore nel rendering pagina " + pageIndex + ": " + e.Message);
        }
    }

    // Aggiorna lo stato dei bottoni
    private void UpdateButtons()
    {
        if (_prevButton != null) _prevButton.interactable = currentPageIndex > 0;
        if (_nextButton != null) _nextButton.interactable = currentPageIndex < totalPages - 1;
    }

    // Logica bottone "Pagina Precedente"
    private void OnPreviousPage()
    {
        if (currentPageIndex > 0)
        {
            RenderPage(currentPageIndex - 1);
        }
    }

    // Logica bottone "Pagina Successiva"
    private void OnNextPage()
    {
        if (currentPageIndex < totalPages - 1)
        {
            RenderPage(currentPageIndex + 1);
        }
    }

    // Logica per "Vai a pagina"
    private void OnGoToPage(string inputText)
    {
        if (int.TryParse(inputText, out int pageNumber) && pageNumber >= 1 && pageNumber <= totalPages)
        {
            RenderPage(pageNumber - 1);
        }
        else
        {
            if (_pageInput != null) _pageInput.text = (currentPageIndex + 1).ToString();
            Debug.LogWarning("Numero pagina non valido! Deve essere tra 1 e " + totalPages);
        }
    }

    // Pulizia
    void OnDestroy()
    {
        if (pdfDocument != null)
        {
            pdfDocument.Dispose();
            pdfDocument = null;
        }
    }



    private void AutoBindComponents()
    {
        // Cerca il pulsante "Precedente/Left"
        Transform left = transform.Find("Precedente/Left");
        if (left != null)
        {
            _prevButton = left.GetComponent<Button>();
        }
        else
        {
            Debug.LogWarning("PrevButton (Precedente/Left) non trovato.");
        }

        // Cerca il pulsante "Successivo/Right"
        Transform right = transform.Find("Successivo/Right");
        if (right != null)
        {
            _nextButton = right.GetComponent<Button>();
        }
        else
        {
            Debug.LogWarning("NextButton (Successivo/Right) non trovato.");
        }

        // Cerca il campo input "Index/Page"
        Transform page = transform.Find("Index/Page");
        if (page != null)
        {
            _pageInput = page.GetComponent<TMP_InputField>();
        }
        else
        {
            Debug.LogWarning("PageInput (Index/Page) non trovato.");
        }
    }
}