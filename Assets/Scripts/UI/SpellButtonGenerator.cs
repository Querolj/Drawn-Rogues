using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

public class SpellButtonGenerator : MonoBehaviour
{
    [SerializeField]
    private SpellButton _spellButtonTemplate;

    [SerializeField]
    private Drawer _drawer;

    private Character _lastPlayerCharacter = null;

    private ModeSwitcher _modeSwitcher;

    [Inject, UsedImplicitly]
    private void Init (ModeSwitcher modeSwitcher)
    {
        _modeSwitcher = modeSwitcher;
    }

    public void GenerateButtons (DrawedCharacter playerCharacter)
    {
        List<GameObject> toDelete = new List<GameObject> ();
        for (int i = 0; i < transform.childCount; i++)
        {
            toDelete.Add (transform.GetChild (i).gameObject);
        }

        foreach (GameObject go in toDelete)
            GameObject.DestroyImmediate (go);

        _lastPlayerCharacter = playerCharacter ??
            throw new ArgumentNullException (nameof (playerCharacter));

        foreach (ColouringSpell spell in playerCharacter.ColouringSpells)
        {
            SpellButton spellButton = Instantiate<SpellButton> (_spellButtonTemplate);
            spellButton.Display (spell);

            spellButton.transform.SetParent (transform);
            spellButton.transform.localScale = Vector3.one;

            spellButton.Button.onClick.AddListener (
                () =>
                {
                    _modeSwitcher.ChangeMode (ModeSwitcher.Mode.Draw);
                    _drawer.SetSelectedColouring (spell);
                }
            );
        }
    }
}