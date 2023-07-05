using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuickBookModifier : MonoBehaviour
{
    [SerializeField]
    private ModifierSlot _modifierSlotTemplate;

    [SerializeField]
    private ModifierPlacer _modifierPlacer;

    [SerializeField]
    private ModifierSlot _selectedModifierSlot;

    [SerializeField]
    private ModifierLoader _modifierLoader;

    [SerializeField]
    private Transform _slotContainer;

    [SerializeField]
    private TMP_Text _title;

    [SerializeField]
    private Bookmark _bookmarkEyes;

    [SerializeField]
    private Bookmark _bookmarkMouths;

    private Modifier _selectedModifier;
    private List<ModifierSlot> _slotsCreated = new List<ModifierSlot> ();
    private Dictionary<ModifierType, List<Modifier>> _modifiersByType = new Dictionary<ModifierType, List<Modifier>> ();

    private void Start ()
    {
        _selectedModifierSlot.gameObject.SetActive (false);
        for (int i = 0; i < _modifierLoader.GetMaxModifierList (); i++)
        {
            ModifierSlot slot = Instantiate (_modifierSlotTemplate, _slotContainer).GetComponent<ModifierSlot> ();
            ClickableImage clickableImage = slot.GetComponentInChildren<ClickableImage> ();
            if (clickableImage == null)
                throw new System.Exception ("No clickable image found in " + slot.name);

            clickableImage.OnClick += () =>
            {
                if (!_modifierPlacer.TrySetModifier (slot.Modifier, slot.ModifierImage))
                    return;

                if (_selectedModifier == slot.Modifier)
                    return;

                _selectedModifier = slot.Modifier;
                if (!_selectedModifierSlot.gameObject.activeSelf)
                    _selectedModifierSlot.gameObject.SetActive (true);

                _selectedModifierSlot.Display (_selectedModifier);
            };

            _slotsCreated.Add (slot);
            slot.gameObject.SetActive (false);
        }

        _modifiersByType = _modifierLoader.GetModifiers ();
        SetBookmarks ();
        Display (ModifierType.Eye);
    }

    private void SetBookmarks ()
    {
        _bookmarkEyes.Init (() =>
        {
            _bookmarkEyes.SetSelected (true);
            _bookmarkMouths.SetSelected (false);
            Display (ModifierType.Eye);
        });

        _bookmarkMouths.Init (() =>
        {
            _bookmarkEyes.SetSelected (false);
            _bookmarkMouths.SetSelected (true);
            Display (ModifierType.Mouth);
        });
    }

    public void Display (ModifierType type)
    {
        if (!_modifiersByType.ContainsKey (type))
            return;

        _title.text = type.ToString ();
        List<Modifier> modifiers = _modifiersByType[type];
        for (int i = 0; i < _slotsCreated.Count; i++)
        {
            if (i < modifiers.Count)
            {
                _slotsCreated[i].gameObject.SetActive (true);
                _slotsCreated[i].Display (modifiers[i]);
            }
            else
            {
                _slotsCreated[i].gameObject.SetActive (false);
            }
        }
    }
}