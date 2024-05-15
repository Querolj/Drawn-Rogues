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

    #region  Injected
    private AttackSelectionManager _attackSelectionManager;
    private AttackInstance.Factory _attackInstanceFactory;
    #endregion

    [Inject, UsedImplicitly]
    private void Init (AttackSelectionManager attackSelectionManager, AttackInstance.Factory attackInstanceFactory)
    {
        _attackSelectionManager = attackSelectionManager;
        _attackInstanceFactory = attackInstanceFactory;
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
                    AttackInstance attackInstance = _attackInstanceFactory.Create (attack, playerCharacter);
                    attackSelection.Activate (attackInstance, playerCharacter, combatZone, onAttackEnded);
                }
            );
        }
    }
}