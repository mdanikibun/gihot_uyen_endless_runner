using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameFlowController gameFlow;
    [SerializeField] TMP_InputField playerNameInput;
    [SerializeField] Button[] characterSlots;
    [SerializeField] Button nextButton;
    [SerializeField] CharacterOption[] characters;
    [SerializeField] GameSettings settings;

    int selectedIndex = -1;

    public int SelectedIndex => selectedIndex;
    public string PlayerName {
        get {
            return playerNameInput.text.Trim();
        }
    }

    bool HasValidName => !string.IsNullOrWhiteSpace(PlayerName);

    void Awake() {
        playerNameInput.onValueChanged.AddListener(OnPlayerNameChanged);
        SetupSlotListeners();
        nextButton.onClick.AddListener(OnNextClicked);
        RefreshNextButton();
    }

    void OnEnable() {
        selectedIndex = -1;
        RefreshSlotVisuals();
        RefreshNextButton();
    }

    public void OnCharacterClicked(int index) {
        if (index < 0 || index >= characterSlots.Length) return;

        selectedIndex = index;
        RefreshSlotVisuals();
        RefreshNextButton();

        gameFlow.SetSelectedCharacter(index, GetSelectedOption());
    }

    public void OnNextClicked() {
        if (!CanGoNext()) return;

        gameFlow.SetPlayerName(PlayerName);
        gameFlow.ShowHowToPlay();
    }

    void OnPlayerNameChanged(string _) {
        RefreshNextButton();
    }

    bool CanGoNext() {
        return selectedIndex >= 0 && HasValidName;
    }

    void RefreshNextButton() {
        nextButton.interactable = CanGoNext();
    }

    CharacterOption GetSelectedOption() {
        if (selectedIndex < 0 || selectedIndex >= characters.Length) {
            return null;
        }
        return characters[selectedIndex];
    }

    void RefreshSlotVisuals() {

        for (int i = 0; i < characterSlots.Length; i++) {
            Image image = characterSlots[i].targetGraphic as Image;
            image.color = i == selectedIndex ? settings.characterSelect.selectedSlotColor : settings.characterSelect.normalSlotColor;
        }
    }

    void SetupSlotListeners() {
        for (int i = 0; i < characterSlots.Length; i++) {
            int index = i;
            characterSlots[i].onClick.AddListener(() => OnCharacterClicked(index));
        }
    }
}
