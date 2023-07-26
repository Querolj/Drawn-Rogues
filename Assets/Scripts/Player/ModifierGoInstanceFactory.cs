using System;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

public class ModifierGoInstanceFactory : MonoBehaviour
{
    [SerializeField]
    private Drawer _drawer;

    private ModeSwitcher _modeSwitcher;
    private ActionDelayer _actionDelayer;

    [Inject, UsedImplicitly]
    private void Init (ModeSwitcher modeSwitcher, ActionDelayer actionDelayer)
    {
        _modeSwitcher = modeSwitcher;
        _actionDelayer = actionDelayer;
    }

    public GameObject CreateForCanvas (Bounds renderBounds, Transform modifierLayer, Modifier modifier, Vector3 localPos, bool isFlipped, float delayLimitCalculInSec = 0f, Action<Modifier> onModifierDeleted = null)
    {
        GameObject go = new GameObject ("Modifier_" + modifier.Name);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer> ();
        MoveModifier moveModifier = go.AddComponent<MoveModifier> ();

        // To wait for a camera to do a transition, since we have limits in screen space 
        _actionDelayer.ExecuteInSeconds (delayLimitCalculInSec, () =>
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
        sr.transform.SetParent (modifierLayer);
        localPos.z = 0.0001f;
        sr.transform.localPosition = localPos;

        return go;
    }

    public GameObject CreateForCharacter (Transform modifierLayer, Modifier modifier, Vector3 localPos, bool isFlipped, Transform charTransform)
    {
        GameObject go = new GameObject ("Modifier_" + modifier.Name);
        SpriteDepthAjustToRotation spriteDepthAjuster = go.AddComponent<SpriteDepthAjustToRotation> ();
        spriteDepthAjuster.Init (charTransform);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer> ();
        sr.sprite = modifier.Sprite;
        sr.flipX = isFlipped;
        sr.transform.SetParent (modifierLayer);
        localPos.z = 0.0001f;
        sr.transform.localPosition = localPos;

        return go;
    }
}