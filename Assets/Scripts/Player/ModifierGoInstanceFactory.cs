using System;
using UnityEngine;

public class ModifierGoInstanceFactory : MonoBehaviour
{
    [SerializeField]
    private Drawer _drawer;

    [SerializeField]
    private ModeSwitcher _modeSwitcher;

    public GameObject Create (Bounds renderBounds, Transform modifierLayer, Modifier modifier, Vector3 localPos, bool isFlipped, float delayLimitCalculInSec = 0f, Action<Modifier> onModifierDeleted = null)
    {
        GameObject go = new GameObject ("Modifier_" + modifier.Name);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer> ();
        MoveModifier moveModifier = go.AddComponent<MoveModifier> ();

        // To wait for a camera to do a transition, since we have limits in screen space 
        ActionDelayer.Instance.ExecuteInSeconds (delayLimitCalculInSec, () =>
        {
            Vector3 bottomLeftLimit = renderBounds.center - renderBounds.extents;

            bottomLeftLimit += modifier.Sprite.bounds.extents.x * modifierLayer.right;
            bottomLeftLimit += modifier.Sprite.bounds.extents.y * modifierLayer.up;

            bottomLeftLimit = Camera.main.WorldToScreenPoint (bottomLeftLimit);

            Vector3 topRightLimit = renderBounds.center + renderBounds.extents;

            topRightLimit -= modifier.Sprite.bounds.extents.x * modifierLayer.right;
            topRightLimit -= modifier.Sprite.bounds.extents.y * modifierLayer.up;

            topRightLimit = Camera.main.WorldToScreenPoint (topRightLimit);

            moveModifier.Init (bottomLeftLimit, topRightLimit, _modeSwitcher, modifier, onModifierDeleted);
        });

        BoxCollider2D box = go.AddComponent<BoxCollider2D> ();
        Rigidbody2D rb = go.AddComponent<Rigidbody2D> ();
        rb.bodyType = RigidbodyType2D.Kinematic;
        box.size = modifier.Sprite.bounds.size;
        box.isTrigger = true;

        _drawer.OnDrawStrokeStart += () =>
        {
            if (box != null)
                box.enabled = false;
        };

        _drawer.OnDrawStrokeEnd += (lc, si) =>
        {
            if (box != null)
                box.enabled = true;
        };

        sr.sprite = modifier.Sprite;
        sr.flipX = isFlipped;
        sr.sortingOrder = 1;
        sr.transform.SetParent (modifierLayer);
        sr.transform.localPosition = localPos;

        return go;
    }
}