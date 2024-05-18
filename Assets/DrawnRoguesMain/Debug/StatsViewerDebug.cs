using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class StatsViewerDebug : MonoBehaviour
{
    [SerializeField]
    private InputActionReference _displayStatsInput;

    [SerializeField]
    private TMP_Text _statsDescriptionText;

    [SerializeField]
    private RectTransform _statsTooltip;

    private bool _activated = false;

    void Start ()
    {
        _statsTooltip.gameObject.SetActive (false);
        _displayStatsInput.action.performed += _ => Activate ();
    }

    private void Activate ()
    {
        _activated = !_activated;
        if (!_activated)
            _statsTooltip.gameObject.SetActive (false);
    }

    private void Update ()
    {
        if (_activated)
            TryDisplayStats ();
    }

    private void TryDisplayStats ()
    {
        Ray ray = Camera.main.ScreenPointToRay (Mouse.current.position.ReadValue ());
        RaycastHit[] hits = Physics.RaycastAll (ray);
        foreach (RaycastHit hit in hits)
        {
            Attackable attackable = hit.collider.GetComponent<Attackable> ();
            if (attackable != null)
            {
                _statsDescriptionText.text = attackable.Stats.ToString ();
                _statsTooltip.gameObject.SetActive (true);
                _statsTooltip.position = Mouse.current.position.ReadValue ();
                return;
            }
        }

        _statsTooltip.gameObject.SetActive (false);
    }
}