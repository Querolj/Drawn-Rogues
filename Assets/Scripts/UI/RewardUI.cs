using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardUI : MonoBehaviour
{
    [SerializeField]
    private ColorDropReward _colorDropRewardPrefab;

    [SerializeField]
    private ColouringSlot _colouringSlotRewardPrefab;

    [SerializeField]
    private Transform _rewardContainer;

    [SerializeField]
    private Button _validateButton;

    private Action _onValidate;

    private void Awake ()
    {
        _validateButton.onClick.AddListener (() =>
        {
            gameObject.SetActive (false);

            foreach (Transform child in _rewardContainer)
            {
                Destroy (child.gameObject);
            }

            _onValidate?.Invoke ();
        });
    }

    public void Display (Dictionary<BaseColor, int> colorDropsLooted, List<Colouring> colouringsDiscovered, Action onValidate)
    {
        _onValidate = onValidate;
        gameObject.SetActive (true);
        foreach (KeyValuePair<BaseColor, int> colorDrop in colorDropsLooted)
        {
            ColorDropReward colorDropReward = Instantiate (_colorDropRewardPrefab, _rewardContainer);
            colorDropReward.Display (colorDrop.Key, colorDrop.Value);
        }

        if (colouringsDiscovered == null || colouringsDiscovered.Count == 0)
            return;

        foreach (Colouring colouring in colouringsDiscovered)
        {
            ColouringSlot colouringSlot = Instantiate (_colouringSlotRewardPrefab, _rewardContainer);
            colouringSlot.Display (colouring);
        }
    }
}