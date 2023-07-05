using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChararacterCreationSetup : MonoBehaviour
{
    [SerializeField, Tooltip ("Allow to uncomplete the drawing of the character for testing purpose")]
    private bool _allowUncompleteDrawing = false;

    [SerializeField]
    private Button _validateCharColorSelectionButton;

    [SerializeField]
    private Button _endCharacterCreationButton;

    [SerializeField]
    private TMP_Text _characterCreationInfoText;
    private const string _characterCreationInfoTextColor = "- Use all colors to valid the character";
    private const string _characterCreationInfoTextName = "- Name the character";

    [SerializeField]
    private TMP_InputField _characterNameInputField;

    [SerializeField]
    private SelectedColors _selectedColors;

    [SerializeField]
    private BaseColorPalette _palette;

    [SerializeField]
    private GameObject _characterColorSelectionsPanel;

    [SerializeField]
    private DrawedCharacter _drawedCharacter;
    private Frame2D _characterFrame;

    [SerializeField]
    private CharacterPivot _playerPivot;

    [SerializeField]
    private QuickBookColouring _quickBook;

    private const int BASE_COLOR_AMOUNT = 200;

    private void Awake ()
    {
        _quickBook.gameObject.SetActive (true);
        InitPalette ();
        _characterFrame = _drawedCharacter.GetComponent<Frame2D> ();

        _characterCreationInfoText.text = _characterCreationInfoTextColor + "\n" + _characterCreationInfoTextName;
        _endCharacterCreationButton.onClick.AddListener (() =>
        {
            _drawedCharacter.Name = _characterNameInputField.text;

            // DrawedCharacterInfos charInfos = new DrawedCharacterInfos (_characterFrame.GetTextureInfos (), _characterNameInputField.text, _drawedCharacter.ModifierAdded);
            // Saver.SaveDrawedCharacterInfos (charInfos);

            _playerPivot.InitForMap ();

            SceneManager.LoadScene ("Map");
        });
    }

    private void InitPalette ()
    {
        List<ColouringInstance> colouringInstances = new List<ColouringInstance> ();
        Dictionary<BaseColor, int> colorDropsAvailable = new Dictionary<BaseColor, int> ();

        foreach (BaseColor bc in System.Enum.GetValues (typeof (BaseColor)))
        {
            colorDropsAvailable.Add (bc, BASE_COLOR_AMOUNT);
        }

        _palette.ColorDropsAvailable = colorDropsAvailable;
        _characterColorSelectionsPanel.SetActive (false);
    }

    private bool validateCreation = false;
    private void Update ()
    {
        if (_characterColorSelectionsPanel.activeInHierarchy)
            return;

        UpdateCharacterCreationInfoText ();
        UpdateValidateCreationButton ();
    }

    private void UpdateCharacterCreationInfoText ()
    {
        _characterCreationInfoText.text = string.Empty;

        if (_palette.TotalColorQuantityLeft () != 0)
        {
            _characterCreationInfoText.text = _characterCreationInfoTextColor;
        }
        if (string.IsNullOrEmpty (_characterNameInputField.text) || _characterNameInputField.text.Length < 2)
        {
            if (_characterCreationInfoText.text != string.Empty)
                _characterCreationInfoText.text += "\n";

            _characterCreationInfoText.text += _characterCreationInfoTextName;
        }
    }

    private void UpdateValidateCreationButton ()
    {
        if (_allowUncompleteDrawing && !string.IsNullOrEmpty (_characterCreationInfoText.text) && !_endCharacterCreationButton.gameObject.activeInHierarchy)
        {
            _endCharacterCreationButton.gameObject.SetActive (true);
            _characterCreationInfoText.gameObject.SetActive (false);
            return;
        }

        if (_allowUncompleteDrawing)
            return;

        if (_endCharacterCreationButton.gameObject.activeInHierarchy && !string.IsNullOrEmpty (_characterCreationInfoText.text))
        {
            _endCharacterCreationButton.gameObject.SetActive (false);
            _characterCreationInfoText.gameObject.SetActive (true);
        }
        else if (!_endCharacterCreationButton.gameObject.activeInHierarchy && string.IsNullOrEmpty (_characterCreationInfoText.text))
        {
            _endCharacterCreationButton.gameObject.SetActive (true);
            _characterCreationInfoText.gameObject.SetActive (false);
        }
    }
}