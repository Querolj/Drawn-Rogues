using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModifierSlot : MonoBehaviour
{

    [SerializeField]
    private TMP_Text _title;

    [SerializeField]
    private TMP_Text _description;

    [SerializeField]
    private Image _modifierImage;
    public Image ModifierImage
    {
        get
        {
            return _modifierImage;
        }
    }

    private Modifier _modifier;
    public Modifier Modifier
    {
        get
        {
            return _modifier;
        }
    }

    public void Display (Modifier modifier)
    {
        _modifier = modifier ??
            throw new System.ArgumentNullException (nameof (modifier));

        _title.text = modifier.Name;
        _description.text = modifier.Description;
        _modifierImage.sprite = modifier.Sprite;
        CorrectImageDimension (modifier, ref _modifierImage);
    }

    private void CorrectImageDimension (Modifier modifier, ref Image image)
    {
        // (Vector3 topRight, Vector3 bottomLeft) = GetTopRightBottomLeftScreenPointToWorld ();
        // float width = topRight.x - bottomLeft.x;
        // float height = topRight.y - bottomLeft.y;
        // Debug.Log (topRight.ToString("F3") + " " + bottomLeft.ToString("F3"));
        // Debug.Log (width + " " + height);

        // float ratioWidth = Screen.width / width;
        // float ratioHeight = Screen.height / height;

        // Vector2 imageDimWorld = new Vector2 (modifier.Sprite.rect.width / modifier.Sprite.pixelsPerUnit, modifier.Sprite.rect.height / modifier.Sprite.pixelsPerUnit);
        // Debug.Log (imageDimWorld.ToString("F3"));
        // // image.rectTransform.sizeDelta = new Vector2 (imageDimWorld.x * ratioWidth, imageDimWorld.y * ratioHeight);
        // Vector3 canvasDimension = _modifierImage.canvas.GetComponent<RectTransform> ().sizeDelta;
        Vector2 newDimensions = new Vector2 (modifier.Sprite.rect.width, modifier.Sprite.rect.height);
        float clampValue = modifier.Sprite.rect.width > modifier.Sprite.rect.height ? modifier.Sprite.rect.width : modifier.Sprite.rect.height;
        float upscaleValue = _modifierImage.rectTransform.sizeDelta.x / clampValue;
        upscaleValue = Mathf.Clamp (upscaleValue, 0f, 3f);
        newDimensions *= upscaleValue;

        image.rectTransform.sizeDelta = newDimensions;

    }

    private (Vector3, Vector3) GetTopRightBottomLeftScreenPointToWorld ()
    {
        Vector3 topRight = Camera.main.ScreenToWorldPoint (new Vector3 (Screen.width, Screen.height, 0));
        Vector3 bottomLeft = Camera.main.ScreenToWorldPoint (new Vector3 (0, 0, 0));
        return (topRight, bottomLeft);
    }
}