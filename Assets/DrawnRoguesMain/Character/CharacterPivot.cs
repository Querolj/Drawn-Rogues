using System.Collections;
using UnityEngine;

public class CharacterPivot : MonoBehaviour
{
    [SerializeField]
    private Character _character;

    private void Awake ()
    {
        DontDestroyOnLoad (gameObject);
    }

    private IEnumerator Start ()
    {
        yield return new WaitForEndOfFrame (); // wait for drawed character to have his collider generated
        yield return new WaitForEndOfFrame ();

        Init ();
        DrawedCharacter drawedCharacter = _character as DrawedCharacter;
        if (drawedCharacter != null)
            drawedCharacter.OnDrawedCharacterUpdate += UpdateCharacterLocalPos;
    }

    public void Init ()
    {
        UpdateCharacterLocalPos ();
    }

    private void UpdateCharacterLocalPos ()
    {
        _character.transform.localPosition = Vector3.zero;

        Bounds bounds = _character.GetSpriteBounds ();
        if (bounds == null)
            throw new System.Exception ("Bounds null, not yet updated?");
        Vector2 offset = Vector2.zero;
        offset.x = _character.GetXOffSetFromRenderer ();
        offset.y = _character.Renderer.bounds.extents.y;
        _character.transform.localPosition = offset;
    }
}