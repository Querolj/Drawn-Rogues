using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CharacterCanvasDescDisplay : MonoBehaviour
{
    [System.Serializable]
    public class IconWithHeightAdjectiveAndDescription
    {
        public Sprite IconTex;
        public HeightAdjective HeightAdjective;

        [TextArea]
        public string Description;
    }

    [System.Serializable]
    public class IconWithWidthAdjectiveAndDescription
    {
        public Sprite IconTex;
        public WidthAdjective WidthAdjective;

        [TextArea]
        public string Description;

    }

    [System.Serializable]
    public class IconUnderNumberOfMembersAndDescription
    {
        public Sprite IconTex;
        public int NumberOfMembers;

        [TextArea]
        public string Description;

    }

    [SerializeField]
    private IconWithHeightAdjectiveAndDescription[] _heightAdjectiveIcons;

    [SerializeField]
    private IconWithWidthAdjectiveAndDescription[] _widthAdjectiveIcons;

    [SerializeField]
    private IconUnderNumberOfMembersAndDescription[] _numberOfArmsIcons;

    [SerializeField]
    private IconUnderNumberOfMembersAndDescription[] _numberOfLegsIcons;

    [SerializeField]
    private TMP_Text _statsDescription;

    [SerializeField]
    private CharacterCanvas _characterCanvas;

    [SerializeField]
    private Image _heightIcon;

    [SerializeField]
    private TMP_Text _heightDescription;

    [SerializeField]
    private Image _widthIcon;

    [SerializeField]
    private TMP_Text _widthDescription;

    [SerializeField]
    private Image _numberOfArmsIcon;

    [SerializeField]
    private TMP_Text _numberOfArmsText;

    [SerializeField]
    private TMP_Text _armsDescription;

    [SerializeField]
    private Image _numberOfLegsIcon;

    [SerializeField]
    private TMP_Text _numberOfLegsText;

    [SerializeField]
    private TMP_Text _legsDescription;

    [SerializeField]
    private TMP_Text _weightDescription;

    private Drawer _drawer;
    
    [Inject, UsedImplicitly]
    private void Init (Drawer drawer)
    {
        _drawer = drawer;
    }
    
    private void Awake ()
    {
        _characterCanvas.OnStatsChanged += () =>
        {
            _statsDescription.text = _characterCanvas.Stats.ToString ();
        };

        _characterCanvas.OnCharFormUpdated += () =>
        {
            UpdateFormDescription ();
        };

        System.Array.Sort (_numberOfArmsIcons, (a, b) => b.NumberOfMembers.CompareTo (a.NumberOfMembers));
        System.Array.Sort (_numberOfLegsIcons, (a, b) => b.NumberOfMembers.CompareTo (a.NumberOfMembers));

        _drawer.OnDraw += () =>
        {
            SetWeightDescription ();
        };
    }

    private void UpdateFormDescription ()
    {
        SetHeightAdjectiveIcon ();
        GetWidthAdjectiveIcon ();
        SetArmIcon ();
        _numberOfArmsText.text = _characterCanvas.DrawedCharacterFormDescription.NumberOfArms.ToString ();
        _numberOfArmsText.gameObject.SetActive (_characterCanvas.DrawedCharacterFormDescription.NumberOfArms != 0 &&
            _characterCanvas.DrawedCharacterFormDescription.NumberOfArms < 4);

        SetLegIcon ();
        _numberOfLegsText.text = _characterCanvas.DrawedCharacterFormDescription.NumberOfLegs.ToString ();
        _numberOfLegsText.gameObject.SetActive (_characterCanvas.DrawedCharacterFormDescription.NumberOfLegs != 0 &&
            _characterCanvas.DrawedCharacterFormDescription.NumberOfLegs < 4);
    }

    private void SetWeightDescription ()
    {
        _weightDescription.text = _characterCanvas.Stats.Kilogram + " Kg";
    }

    private void SetHeightAdjectiveIcon ()
    {
        foreach (IconWithHeightAdjectiveAndDescription icon in _heightAdjectiveIcons)
        {
            if (icon.HeightAdjective == _characterCanvas.DrawedCharacterFormDescription.HeightAdjective)
            {
                _heightIcon.sprite = icon.IconTex;
                _heightDescription.text = icon.Description;
                return;
            }
        }

        throw new System.Exception ("No icon found for height adjective " + _characterCanvas.DrawedCharacterFormDescription.HeightAdjective);
    }

    public void GetWidthAdjectiveIcon ()
    {
        foreach (IconWithWidthAdjectiveAndDescription icon in _widthAdjectiveIcons)
        {
            if (icon.WidthAdjective == _characterCanvas.DrawedCharacterFormDescription.WidthAdjective)
            {
                _widthIcon.sprite = icon.IconTex;
                _widthDescription.text = icon.Description;
                return;
            }
        }

        throw new System.Exception ("No icon found for width adjective " + _characterCanvas.DrawedCharacterFormDescription.WidthAdjective);
    }

    public void SetArmIcon ()
    {
        foreach (IconUnderNumberOfMembersAndDescription icon in _numberOfArmsIcons)
        {
            if (icon.NumberOfMembers <= _characterCanvas.DrawedCharacterFormDescription.NumberOfArms)
            {
                _numberOfArmsIcon.sprite = icon.IconTex;
                _armsDescription.text = icon.Description;
                return;
            }
        }

        throw new System.Exception ("No icon found for number of arms " + _characterCanvas.DrawedCharacterFormDescription.NumberOfArms);
    }

    public void SetLegIcon ()
    {
        foreach (IconUnderNumberOfMembersAndDescription icon in _numberOfLegsIcons)
        {
            if (icon.NumberOfMembers <= _characterCanvas.DrawedCharacterFormDescription.NumberOfLegs)
            {
                _numberOfLegsIcon.sprite = icon.IconTex;
                _legsDescription.text = icon.Description;
                return;
            }
        }

        throw new System.Exception ("No icon found for number of legs " + _characterCanvas.DrawedCharacterFormDescription.NumberOfLegs);
    }
}