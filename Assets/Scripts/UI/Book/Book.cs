using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BookType
{
    CharColouring,
    Modifier
}

public class Book : MonoBehaviour
{
    [SerializeField]
    private BookmarkColouring _bookmarkTemplate;

    [SerializeField]
    private Transform _bookmarkContainer;

    [SerializeField]
    private Button _turnRightButton;

    [SerializeField]
    private Button _turnLeftButton;

    [SerializeField]
    private Sprite _bookTurnRightOrLeft;

    [SerializeField]
    private Sprite _bookTurnRight;

    [SerializeField]
    private Sprite _bookTurnLeft;

    [SerializeField]
    private Sprite _bookIdle;

    [SerializeField]
    private GameObject _bookContent;

    [SerializeField]
    private Page _pageWithTitleTemplate;

    [SerializeField]
    private Page _pageRightTemplate;

    [SerializeField]
    private Page _pageLeftTemplate;

    [SerializeField]
    private ColouringType _colouringType;

    [SerializeField]
    private ColouringLoader _colouringLoader;

    private Image _bookImage;
    private int _currentDoublePageIndex = 0;
    private BaseColor _currentPageBaseColor = BaseColor.Brown;

    private Dictionary<BaseColor, List<Page>> _pagesByBaseColor = new Dictionary<BaseColor, List<Page>> ();
    private Dictionary<BaseColor, BookmarkColouring> _bookmarksByBaseColor = new Dictionary<BaseColor, BookmarkColouring> ();
    private Dictionary<BaseColor, List<Colouring>> _colouringsByBaseColor = new Dictionary<BaseColor, List<Colouring>> ();

    private void Awake ()
    {
        _bookImage = GetComponent<Image> ();

        _turnRightButton.onClick.AddListener (() =>
        {
            ChangePage (_currentPageBaseColor, ++_currentDoublePageIndex);
        });

        _turnLeftButton.onClick.AddListener (() =>
        {
            ChangePage (_currentPageBaseColor, --_currentDoublePageIndex);
        });

        _colouringsByBaseColor = _colouringLoader.GetColourings (_colouringType);

        foreach (List<Colouring> colouringList in _colouringsByBaseColor.Values)
        {
            colouringList.Sort ((a, b) => a.Id.CompareTo (b.Id));
        }

        // create title pages and pages for each base color and add the pages to a dictionary. Take into account the available slot for each pages
        foreach (BaseColor baseColor in _colouringsByBaseColor.Keys)
        {
            List<Colouring> colouringsForBaseColor = _colouringsByBaseColor[baseColor];

            // create the title page first
            Page titlePage = Instantiate (_pageWithTitleTemplate, _bookContent.transform);
            titlePage.name += "_" + baseColor;

            if (colouringsForBaseColor.Count == 0)
                continue;
            int colouringOnFirstPage = Mathf.Min (_pageWithTitleTemplate.AvailableColouringSlot, colouringsForBaseColor.Count);
            titlePage.Display (colouringsForBaseColor.GetRange (0, colouringOnFirstPage));
            _pagesByBaseColor.Add (baseColor, new List<Page> { titlePage });

            int pageCount = Mathf.CeilToInt ((float) (colouringsForBaseColor.Count - colouringOnFirstPage) / _pageLeftTemplate.AvailableColouringSlot);
            if (pageCount == 0)
                continue;

            List<Page> pages = new List<Page> ();
            for (int i = 0; i < pageCount; i++)
            {
                Page page;
                if (i % 2 == 0)
                    page = Instantiate (_pageRightTemplate, _bookContent.transform);
                else
                    page = Instantiate (_pageLeftTemplate, _bookContent.transform);

                page.name += "_" + baseColor + "_" + i;
                int index = _pageWithTitleTemplate.AvailableColouringSlot + (i * _pageLeftTemplate.AvailableColouringSlot);
                int count = Mathf.Min (_pageLeftTemplate.AvailableColouringSlot, colouringsForBaseColor.Count - index);
                page.Display (colouringsForBaseColor.GetRange (index, count));
                pages.Add (page);
            }

            _pagesByBaseColor[baseColor].AddRange (pages);
        }

        // create and init bookmarks
        foreach (BaseColor baseColor in _pagesByBaseColor.Keys)
        {
            BookmarkColouring bookmark = Instantiate (_bookmarkTemplate, _bookmarkContainer);
            bookmark.Init (baseColor, () =>
            {
                MoveToBookmark (baseColor);
            });

            _bookmarksByBaseColor.Add (baseColor, bookmark);
        }

        MoveToBookmark (_currentPageBaseColor);
    }

    private void MoveToBookmark (BaseColor baseColor)
    {
        _currentDoublePageIndex = 0;
        _currentPageBaseColor = baseColor;
        ChangePage (_currentPageBaseColor, _currentDoublePageIndex);
        foreach (var kvp in _bookmarksByBaseColor)
        {
            kvp.Value.SetSelected (kvp.Key == baseColor);
        }
    }

    public void ChangePage (BaseColor baseColor, int doublePageNumber)
    {
        if (!_pagesByBaseColor.ContainsKey (baseColor))
            throw new System.ArgumentException ("No colourings for base color " + baseColor, nameof (baseColor));

        int leftPageNumber = doublePageNumber * 2;

        if (leftPageNumber < 0 || leftPageNumber >= _pagesByBaseColor[baseColor].Count)
            throw new System.ArgumentOutOfRangeException (nameof (doublePageNumber), doublePageNumber, "Double page number must be between 0 and " + (_pagesByBaseColor[baseColor].Count / 2));

        int rightPageNumber = leftPageNumber + 1;

        foreach (var kvp in _pagesByBaseColor)
        {
            List<Page> pages = kvp.Value;
            BaseColor currentBaseColor = kvp.Key;
            for (int i = 0; i < _pagesByBaseColor[currentBaseColor].Count; i++)
            {
                if (currentBaseColor == baseColor)
                {
                    _pagesByBaseColor[currentBaseColor][i].gameObject.SetActive (i == leftPageNumber || i == rightPageNumber);
                }
                else
                {
                    _pagesByBaseColor[currentBaseColor][i].gameObject.SetActive (false);
                }
            }
        }

        bool canTurnRight = rightPageNumber < _pagesByBaseColor[baseColor].Count - 1;
        bool canTurnLeft = leftPageNumber != 0;

        _turnRightButton.gameObject.SetActive (canTurnRight);
        _turnLeftButton.gameObject.SetActive (canTurnLeft);

        if (canTurnRight && canTurnLeft)
            _bookImage.sprite = _bookTurnRightOrLeft;
        else if (canTurnRight)
            _bookImage.sprite = _bookTurnRight;
        else if (canTurnLeft)
            _bookImage.sprite = _bookTurnLeft;
        else
            _bookImage.sprite = _bookIdle;

    }
}