using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuickBookColouring : MonoBehaviour
{
    [SerializeField]
    private GameObject _smallSlotTemplate;

    [SerializeField]
    private Transform _slotContainer;

    [SerializeField]
    private TMP_Text _title;

    [SerializeField]
    private BookmarkColouring _bookmarkTemplate;

    [SerializeField]
    private Transform _bookmarkContainer;

    [SerializeField]
    private ColouringSlot _selectedSlot;

    [SerializeField]
    private ColouringLoader _colouringLoader;

    [SerializeField]
    private ColouringType _colouringType;

    [SerializeField]
    private Drawer _drawer;

    private BaseColorPalette _baseColorPalette;

    private List<BookmarkColouring> _bookmarksCreated = new List<BookmarkColouring> ();

    private List<ColouringSlot> _slotsCreated = new List<ColouringSlot> ();
    private List<ClickableImage> _clickableImagesCreated = new List<ClickableImage> ();
    private BaseColor _currentBaseColor = BaseColor.Brown;
    private Colouring _selectedColouring;

    private Dictionary<BaseColor, List<Colouring>> _colouringsByBaseColor;
    public event Action<Colouring> OnColouringSelectionChanged;

    private void Awake ()
    {
        _baseColorPalette = FindObjectOfType<BaseColorPalette> (); // TODO: inject
    }

    private void Start ()
    {
        _selectedSlot.gameObject.SetActive (false);

        _colouringsByBaseColor = _baseColorPalette.ColouringAvailableByBaseColor; // _colouringLoader.GetColourings (_colouringType);

        for (int i = 0; i < _colouringLoader.GetMaxColouringList (_colouringType); i++)
            CreateSlotColouring ();

        // Generate bookmarks
        foreach (BaseColor bc in System.Enum.GetValues (typeof (BaseColor)))
        {
            CreateBookmark (bc);
        }

        Display (BaseColor.Brown);
        _baseColorPalette.OnColouringAdded += OnColouringAdded;
    }

    private void OnColouringAdded (Colouring colouring)
    {
        BaseColor bc = colouring.BaseColorsUsedPerPixel[0].BaseColor;

        if (!_colouringsByBaseColor.ContainsKey (bc))
        {
            _colouringsByBaseColor.Add (bc, new List<Colouring> ());
            CreateBookmark (bc);
        }
        else if (_colouringsByBaseColor[bc].Contains (colouring))
        {
            Debug.LogWarning ("Colouring " + colouring.name + " already added");
            return;
        }
        _colouringsByBaseColor[bc].Add (colouring);
    }

    private void CreateBookmark (BaseColor bc)
    {
        if (!_colouringsByBaseColor.ContainsKey (bc))
            return;

        BookmarkColouring bookmark = Instantiate (_bookmarkTemplate, _bookmarkContainer);
        bookmark.transform.localRotation = Quaternion.Euler (0, 0, 180);
        bookmark.Init (bc, () =>
        {
            if (_currentBaseColor == bc)
                return;

            Display (bc);
            foreach (BookmarkColouring b in _bookmarksCreated)
            {
                if (b == bookmark)
                    bookmark.SetSelected (true);
                else
                    b.SetSelected (false);
            }
        });

        _bookmarksCreated.Add (bookmark);
    }

    private void CreateSlotColouring ()
    {
        ColouringSlot slot = Instantiate (_smallSlotTemplate, _slotContainer).GetComponent<ColouringSlot> ();
        ClickableImage clickableImage = slot.GetComponentInChildren<ClickableImage> ();
        if (clickableImage == null)
            throw new System.Exception ("No clickable image found in " + slot.name);

        clickableImage.OnClick += () =>
        {
            if (_selectedColouring == slot.Colouring)
                return;

            foreach (ClickableImage ci in _clickableImagesCreated)
            {
                if (ci != clickableImage)
                    ci.ActivateIdleImage ();
            }

            _selectedColouring = slot.Colouring;
            _drawer.SetSelectedColouring (slot.Colouring);

            if (!_selectedSlot.gameObject.activeSelf)
                _selectedSlot.gameObject.SetActive (true);

            _selectedSlot.Display (_selectedColouring);
            OnColouringSelectionChanged?.Invoke (_selectedColouring);
        };

        _slotsCreated.Add (slot);
        _clickableImagesCreated.Add (clickableImage);
        slot.gameObject.SetActive (false);
    }

    private void Display (BaseColor bc)
    {
        if (!_colouringsByBaseColor.ContainsKey (bc))
            throw new System.Exception ("No colourings for base color " + bc.ToString ());

        _currentBaseColor = bc;

        _title.text = BaseColorToColor.GetColoredColorName (bc);
        List<Colouring> colourings = _colouringsByBaseColor[bc];

        for (int i = 0; i < _slotsCreated.Count; i++)
        {
            if (i < colourings.Count)
            {
                if (_selectedColouring != null && _selectedColouring == colourings[i])
                {
                    _clickableImagesCreated[i].ActivateClickedImage ();
                }
                else
                {
                    _clickableImagesCreated[i].ActivateIdleImage ();
                }

                _slotsCreated[i].gameObject.SetActive (true);
                _slotsCreated[i].Display (colourings[i]);
            }
            else
            {
                _slotsCreated[i].gameObject.SetActive (false);
            }
        }
    }
}