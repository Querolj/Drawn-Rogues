using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

public class AttackSelectionManager : MonoBehaviour
{

    [SerializeField]
    private AttackSelection[] _attackSelectionTemplates;

    private TrajectoryDrawer _trajectoryDrawer;

    // to sort the templates by type
    private Dictionary<AttackSelectionType, AttackSelection> _attackSelectionTemplatesByType = new Dictionary<AttackSelectionType, AttackSelection> ();
    private Dictionary<AttackSelectionType, AttackSelection> _attackSelectionInstancesByType = new Dictionary<AttackSelectionType, AttackSelection> ();

    private AttackSelection _activatedAttackSelection = null;
    private TrajectoryCalculator _trajectoryCalculator = new TrajectoryCalculator ();
    private AttackSelection.Factory _attackSelectionFactory;

    [Inject, UsedImplicitly]
    private void Init (AttackSelection.Factory attackSelectionFactory, TrajectoryDrawer trajectoryDrawer)
    {
        _attackSelectionFactory = attackSelectionFactory;
        _trajectoryDrawer = trajectoryDrawer;
    }

    private void Awake ()
    {
        if (_attackSelectionTemplates == null)
            throw new System.ArgumentNullException (nameof (_attackSelectionTemplates));

        foreach (var attackSelection in _attackSelectionTemplates)
        {
            _attackSelectionTemplatesByType.Add (attackSelection.AttackSelectionType, attackSelection);
        }
    }

    // Change current activated attack selection to a new one, deactivate the old one
    public AttackSelection SwitchAttackSelection (AttackSelectionType attackSelectionType, Vector3 position)
    {
        if (_activatedAttackSelection != null)
        {
            _activatedAttackSelection.Deactivate ();
        }

        if (!_attackSelectionInstancesByType.ContainsKey (attackSelectionType))
        {
            AttackSelection attackSelectionTemplate = _attackSelectionTemplatesByType[attackSelectionType];
            AttackSelection attackSelectionInstance = _attackSelectionFactory.Create (attackSelectionTemplate);

            attackSelectionInstance.Init (_trajectoryCalculator, _trajectoryDrawer);
            attackSelectionInstance.gameObject.SetActive (false);
            _attackSelectionInstancesByType.Add (attackSelectionType, attackSelectionInstance);
        }

        _activatedAttackSelection = _attackSelectionInstancesByType[attackSelectionType];
        _attackSelectionInstancesByType[attackSelectionType].transform.position = position;
        return _attackSelectionInstancesByType[attackSelectionType];
    }

    public void DeactivateCurrentAttackSelection ()
    {
        _activatedAttackSelection?.Deactivate ();
    }
}