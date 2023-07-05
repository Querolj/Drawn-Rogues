using UnityEngine;
using UnityEngine.EventSystems;

public class ShowOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private GameObject _objectToShow;

    private void Awake ()
    {
        _objectToShow.SetActive (false);
    }

    public void OnPointerEnter (PointerEventData eventData)
    {
        _objectToShow.SetActive (true);
    }

    public void OnPointerExit (PointerEventData eventData)
    {
        _objectToShow.SetActive (false);
    }

}