using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ModifiersDisplayer : MonoBehaviour
{
    [SerializeField]
    private ModifierType _typeToDisplay;

    [SerializeField]
    private ModifierPlacer _modifierPlacer;

    [SerializeField]
    private ModifierInfoDisplayer _modifierInfoDisplayer;

    private List<Modifier> _modifierInstances = new List<Modifier> ();

    private ModeSwitcher _modeSwitcher;

    [Inject, UsedImplicitly]
    private void Init (ModeSwitcher modeSwitcher)
    {
        _modeSwitcher = modeSwitcher;
    }

    private void Awake ()
    {
        Modifier[] modifiers = Resources.LoadAll<Modifier> ("Modifier/" + _typeToDisplay);
        foreach (Modifier modifier in modifiers)
        {
            _modifierInstances.Add (Instantiate<Modifier> (modifier));
        }

        _modifierInstances.Sort ((x, y) => x.OrderInUI.CompareTo (y.OrderInUI));
        foreach (Modifier modifier in _modifierInstances)
        {
            GameObject go = new GameObject ("Modifier_" + modifier.Name);
            Image image = go.AddComponent<Image> ();
            image.sprite = modifier.Sprite;
            image.preserveAspect = true;
            image.rectTransform.SetParent (transform);
            image.rectTransform.localScale = Vector3.one;
            image.rectTransform.sizeDelta = new Vector2 (modifier.Sprite.rect.width, modifier.Sprite.rect.height);

            go.AddComponent<ModifierDisplayer> ().Init (modifier, _modifierPlacer, _modifierInfoDisplayer, _modeSwitcher);
        }
    }
}