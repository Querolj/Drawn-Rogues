using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FailMessage : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _failMessageText;

    [SerializeField]
    private Button _validateButton;

    private Action _onValidate;
    private void Awake ()
    {
        _validateButton.onClick.AddListener (() =>
        {
            gameObject.SetActive (false);
            _onValidate?.Invoke ();
        });
    }

    public void Display (string failMessage, Action onValidate)
    {
        _onValidate = onValidate;
        _failMessageText.text = failMessage;
        gameObject.SetActive (true);
    }
}