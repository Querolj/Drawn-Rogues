using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackChoser : MonoBehaviour
{
    [SerializeField]
    private AttackDisplayer _attackDisplayerTemplate;

    [SerializeField]
    private ToggleGroup _toggleGroup;

    [SerializeField]
    private Button _validate;

    private Attack _chosenAttack;
    private List<AttackDisplayer> _attackDisplayersInstantiated = new List<AttackDisplayer> ();
    private Action<Attack> _onAttackChosen;

    private void Awake ()
    {
        _validate.onClick.AddListener (() =>
        {
            if (_chosenAttack != null)
            {
                gameObject.SetActive (false);
                _onAttackChosen?.Invoke (_chosenAttack);
            }
        });
    }

    public void DisplayAttackToChose (List<Attack> attacks, Action<Attack> onAttackChosen)
    {
        _onAttackChosen = onAttackChosen ??
            throw new ArgumentNullException (nameof (onAttackChosen));
        _chosenAttack = null;

        int i = 0;
        foreach (Attack attack in attacks)
        {
            AttackDisplayer attackDisplayer;
            if (i < _attackDisplayersInstantiated.Count)
            {
                attackDisplayer = _attackDisplayersInstantiated[i];
                attackDisplayer.gameObject.SetActive (true);
            }
            else
            {
                attackDisplayer = Instantiate (_attackDisplayerTemplate, _toggleGroup.gameObject.transform);
                attackDisplayer.Toggle.group = _toggleGroup;
                attackDisplayer.Toggle.onValueChanged.AddListener ((bool isOn) =>
                {
                    if (isOn)
                    {
                        _chosenAttack = attack;
                    }
                });
                _attackDisplayersInstantiated.Add (attackDisplayer);
            }

            attackDisplayer.Display (attack);

            i++;
        }

        gameObject.SetActive (true);
    }
}