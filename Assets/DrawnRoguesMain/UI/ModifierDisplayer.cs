using UnityEngine;
using UnityEngine.EventSystems;

public class ModifierDisplayer : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Modifier _modifier;
    private ModifierPlacer _modifierPlacer;

    private ModifierInfoDisplayer _modifierInfoDisplayer;
    private CursorModeSwitcher _modeSwitcher;

    public void Init (Modifier modifier, ModifierPlacer modifierPlacer, ModifierInfoDisplayer modifierInfoDisplayer, CursorModeSwitcher modeSwitcher)
    {
        _modifierInfoDisplayer = modifierInfoDisplayer ??
            throw new System.ArgumentNullException (nameof (modifierInfoDisplayer));
        _modifier = modifier ??
            throw new System.ArgumentNullException (nameof (modifier));
        _modifierPlacer = modifierPlacer ??
            throw new System.ArgumentNullException (nameof (modifierPlacer));
        _modeSwitcher = modeSwitcher ??
            throw new System.ArgumentNullException (nameof (modeSwitcher));
    }

    public void OnPointerDown (PointerEventData pointerEventData)
    {
        _modeSwitcher.ChangeMode (CursorModeSwitcher.Mode.Selection);
        // _modifierPlacer.SetModifier (_modifier);
    }

    public void OnPointerEnter (PointerEventData pointerEventData)
    {
        _modifierInfoDisplayer.SetModifierInfo (_modifier);
    }

    public void OnPointerExit (PointerEventData pointerEventData)
    {
        _modifierInfoDisplayer.RemoveModifierInfo ();
    }
}