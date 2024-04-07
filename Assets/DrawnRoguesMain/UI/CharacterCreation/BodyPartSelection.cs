using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class BodyPartSelectionToggle : MonoBehaviour
{
    [SerializeField]
    private Toggle _headToggle;

    [SerializeField]
    private Toggle _bodyToggle;

    [SerializeField]
    private Toggle _armToggle;

    [SerializeField]
    private Toggle _legToggle;

    private Drawer _drawer;

    [Inject, UsedImplicitly]
    private void Init (Drawer drawer)
    {
        _drawer = drawer;
        _headToggle.onValueChanged.AddListener (UpdatePixelUsageFromSelectedBodyPart);
        _bodyToggle.onValueChanged.AddListener (UpdatePixelUsageFromSelectedBodyPart);
        _armToggle.onValueChanged.AddListener (UpdatePixelUsageFromSelectedBodyPart);
        _legToggle.onValueChanged.AddListener (UpdatePixelUsageFromSelectedBodyPart);
    }

    private void OnDestroy ()
    {
        _headToggle.onValueChanged.RemoveListener (UpdatePixelUsageFromSelectedBodyPart);
        _bodyToggle.onValueChanged.RemoveListener (UpdatePixelUsageFromSelectedBodyPart);
        _armToggle.onValueChanged.RemoveListener (UpdatePixelUsageFromSelectedBodyPart);
        _legToggle.onValueChanged.RemoveListener (UpdatePixelUsageFromSelectedBodyPart);
    }

    private void UpdatePixelUsageFromSelectedBodyPart (bool toggle)
    {
        if (_headToggle.isOn)
        {
            _drawer.CurrentPixelUsage = PixelUsage.Head;
        }
        else if (_bodyToggle.isOn)
        {
            _drawer.CurrentPixelUsage = PixelUsage.Body;
        }
        else if (_armToggle.isOn)
        {
            _drawer.CurrentPixelUsage = PixelUsage.Arm;
        }
        else if (_legToggle.isOn)
        {
            _drawer.CurrentPixelUsage = PixelUsage.Leg;
        }
        else
            throw new System.Exception ("No body part selected");

    }
}