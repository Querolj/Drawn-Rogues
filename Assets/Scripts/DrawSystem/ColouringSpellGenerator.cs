using UnityEngine;
// using JetBrains.Annotations;
// using Zenject;

public class ColouringSpellGenerator : MonoBehaviour
{
    private Drawer _drawer;
    private TurnManager _turnManager;
    private PlayerController _playerController;

    // [Inject, UsedImplicitly]
    // private void Init () { }

    private void Awake ()
    {
        _drawer = FindAnyObjectByType<Drawer> (); // TODO : inject
        _turnManager = FindAnyObjectByType<TurnManager> (); // TODO : inject
        _playerController = FindAnyObjectByType<PlayerController> (); // TODO : inject

        _drawer.OnDrawStrokeEnd += (c, si) =>
        {
            FrameDecor frameDecor = si.FrameTouched as FrameDecor;
            if (frameDecor == null)
            {
                Debug.LogError ("Frame touched is not a FrameDecor");
                _turnManager.EndTurn (_playerController.ControlledCharacter);
                return;
            }

            ColouringSpell colouringSpell = c as ColouringSpell;
            if (colouringSpell == null)
                return;

            if (_turnManager.InCombat)
                frameDecor.InitColouringSpell (colouringSpell, _drawer.LastStrokeDrawUVs, () =>
                {
                    _turnManager.EndTurn (_playerController.ControlledCharacter);
                });
            else
                frameDecor.InitColouringSpell (colouringSpell, _drawer.LastStrokeDrawUVs);

            frameDecor.ClearDrawTexture ();
            frameDecor.ClearMetadata ();
        };
    }
}