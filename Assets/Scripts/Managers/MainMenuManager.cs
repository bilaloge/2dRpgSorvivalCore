using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using Zenject;
public class MainMenuManager : MonoBehaviour
{
    [Inject] private GameDataManager _gameDataManager;

    [Header("Core Panels")]
    [SerializeField] private RectTransform mainMenuPanel;
    [SerializeField] private RectTransform newGamePanel;
    [SerializeField] private RectTransform loadGamePanel;
    [SerializeField] private RectTransform settingsPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button NewGame;
    [SerializeField] private Button Continue;
    [SerializeField] private Button LoadGame;
    [SerializeField] private Button Settings;
    [SerializeField] private Button Exit;

    [Header("New Game Panel UI")]
    [SerializeField] private TMP_InputField characterNameInput;
    [SerializeField] private Toggle fastGameToggle;
    [SerializeField] private Button StartNewGame;

    [SerializeField] private Button BackToMainMenuBttn;

    [Header("Difficulty Buttons")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color defaultColor = Color.white;

    [Header("Animation Settings")]
    [SerializeField] private float transitionSpeed = 10f;
    private Vector2 screenHiddenPosition; // Saðda bekleme noktasý
    private Vector2 screenHiddenLeft;    // Solda bekleme noktasý
    private Vector2 screenCenter = Vector2.zero;
    private RectTransform currentActivePanel; // Ekranda o an hangi alt panelin açýk olduðunu tutar

    private int selectedDifficulty = 1;
    [SerializeField] private string firstLevelName = "StartZone";

    private void Start()
    {
        //Canvas'ýn gerçek geniþliðini aldýk sonrada 
        // RectTransform olan bir panelin içinden(main menu gibi) canvas geniþliðine ulaþtýk
        Canvas canvas = mainMenuPanel.GetComponentInParent<Canvas>();
        float canvasWidth = canvas.GetComponent<RectTransform>().rect.width;

        // Paneli ekran geniþliðinin tam 1.5 katý uzaða atttým. kafam rahat
        float offScreenValue = canvasWidth * 1.5f;

        screenHiddenPosition = new Vector2(offScreenValue, 0);
        screenHiddenLeft = new Vector2(-offScreenValue, 0);

        SetupInitialPositions(); // Bu deðerler hesaplandýktan sonra çaðrýlmalý
        SetupButtonListeners();
        SetDifficulty(1);
        CheckContinueButton();
    }
    private void SetupInitialPositions()
    {
        newGamePanel.anchoredPosition = screenHiddenPosition;
        loadGamePanel.anchoredPosition = screenHiddenPosition;
        settingsPanel.anchoredPosition = screenHiddenPosition;
        mainMenuPanel.anchoredPosition = screenCenter;
    }
    private void SetupButtonListeners()
    {
        NewGame.onClick.AddListener(() => TransitionToPanel(newGamePanel));
        LoadGame.onClick.AddListener(() => TransitionToPanel(loadGamePanel));
        Settings.onClick.AddListener(() => TransitionToPanel(settingsPanel));
        Exit.onClick.AddListener(QuitGame);
        Continue.onClick.AddListener(OnContinueButtonClicked);

        BackToMainMenuBttn.onClick.AddListener(BackToMainMenu);

        StartNewGame.onClick.AddListener(StartActualGame);

        //zorluk seçenekleri
        easyButton.onClick.AddListener(() => SetDifficulty(0));
        normalButton.onClick.AddListener(() => SetDifficulty(1));
        hardButton.onClick.AddListener(() => SetDifficulty(2));
    }
    private void CheckContinueButton()
    {
        string worldPath = Path.Combine(Application.persistentDataPath, "World_Main.json");
        string charPath = Path.Combine(Application.persistentDataPath, "Character_Hero.json");

        // Eðer her iki dosya da (Dünya ve Karakter) diskte varsa kayýt vardýr.
        bool hasSaveData = File.Exists(worldPath) && File.Exists(charPath);

        if (Continue != null)
        {
            Continue.interactable = hasSaveData;
        }
    }

    #region Difficulty Zýmbýrtýlarý
    public void SetDifficulty(int level)
    {
        selectedDifficulty = level;
        // Tüm butonlarýn rengini varsayýlana çek
        ResetDifficultyButtons();

        // Seçilen butonu renklendir (Görsel geri bildirim)
        switch (level)
        {
            case 0: ChangeButtonColor(easyButton, selectedColor); break;
            case 1: ChangeButtonColor(normalButton, selectedColor); break;
            case 2: ChangeButtonColor(hardButton, selectedColor); break;
        }
    }
    private void ResetDifficultyButtons()
    {
        ChangeButtonColor(easyButton, defaultColor);
        ChangeButtonColor(normalButton, defaultColor);
        ChangeButtonColor(hardButton, defaultColor);
    }
    private void ChangeButtonColor(Button btn, Color color)
    {
        ColorBlock cb = btn.colors;
        cb.normalColor = color;
        cb.selectedColor = color; // Týklandýktan sonra da o renkte kalsýn
        btn.colors = cb;
    }
    #endregion
    //Panel Navigation(Dinamik Geçiþ Sistemi)
    private void TransitionToPanel(RectTransform targetPanel)
    {
        StopAllCoroutines();

        // Ana menüyü sola kaydýr
        StartCoroutine(MovePanel(mainMenuPanel, screenHiddenLeft));

        // Ýstenen hedef paneli (New Game, Settings vs.) merkeze getir
        StartCoroutine(MovePanel(targetPanel, screenCenter));

        currentActivePanel = targetPanel; // Hangi panelde olduðumuzu hafýzaya al
    }
    public void BackToMainMenu()
    {
        if (currentActivePanel == null) return;
        StopAllCoroutines();
        // Açýk olan alt paneli tekrar saða (gizli konuma) kaydýr
        StartCoroutine(MovePanel(currentActivePanel, screenHiddenPosition));
        // Ana menüyü soldan tekrar merkeze getir
        StartCoroutine(MovePanel(mainMenuPanel, screenCenter));

        currentActivePanel = null;
    }
    private IEnumerator MovePanel(RectTransform panel, Vector2 targetPos)
    {
        while (Vector2.Distance(panel.anchoredPosition, targetPos) > 0.5f)
        {
            panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, targetPos, Time.deltaTime * transitionSpeed);
            yield return null;
        }
        panel.anchoredPosition = targetPos; // Tam hedef noktaya oturt
    }
    //Game Launch Logic
    public void StartActualGame()
    {
        string playerName = characterNameInput.text;
        bool isFastGame = fastGameToggle.isOn;

        if (string.IsNullOrWhiteSpace(playerName)) return;

        _gameDataManager.CreateNewWorld(playerName, selectedDifficulty);

        // Sahneyi yükle
        SceneManager.LoadScene(firstLevelName);
    }
    public void OnContinueButtonClicked()
    {
        _gameDataManager.ContinueLatestGame();
    }
    private void QuitGame()
    {
        Debug.Log("Çýkýþ yapýldý.");
        Application.Quit();
    }
}