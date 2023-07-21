using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class AttackButtonGenerator : MonoBehaviour
{
    [SerializeField]
    private Button _attackButtonTemplate;

    private AttackSelectionManager _attackSelectionManager;

    private Character _lastPlayerCharacter = null;
    private Action _onAttackEnded;

    [Inject, UsedImplicitly]
    private void Init (AttackSelectionManager attackSelectionManager)
    {
        _attackSelectionManager = attackSelectionManager;
    }

    public void GenerateButtons (Character playerCharacter, CombatZone combatZone, Action onAttackEnded)
    {
        List<GameObject> toDelete = new List<GameObject> ();
        for (int i = 0; i < transform.childCount; i++)
        {
            toDelete.Add (transform.GetChild (i).gameObject);
        }

        foreach (GameObject go in toDelete)
            GameObject.DestroyImmediate (go);

        _onAttackEnded = onAttackEnded ??
            throw new ArgumentNullException (nameof (onAttackEnded));

        _lastPlayerCharacter = playerCharacter ??
            throw new ArgumentNullException (nameof (playerCharacter));

        if (combatZone == null)
            throw new ArgumentNullException (nameof (combatZone));

        foreach (Attack attack in playerCharacter.Attacks)
        {
            Button attackButton = Instantiate (_attackButtonTemplate);
            TMP_Text text = attackButton.GetComponentInChildren<TMP_Text> ();
            text.text = attack.Name;
            attackButton.transform.SetParent (transform);
            attackButton.transform.localScale = Vector3.one;

            attackButton.onClick.AddListener (
                () =>
                {
                    AttackSelection attackSelection = _attackSelectionManager.SwitchAttackSelection (attack.AttackSelectionType, playerCharacter.transform.position);
                    attackSelection.gameObject.SetActive (true);
                    AttackInstance attackInstance = AttackInstFactory.Create (attack, playerCharacter);
                    attackInstance.OnAttackStarted += playerCharacter.GetComponentInParent<CharacterAnimation> ().PlayAttackAnimation;
                    attackSelection.Activate (attackInstance, playerCharacter, combatZone, onAttackEnded);
                }
            );
        }
    }
}