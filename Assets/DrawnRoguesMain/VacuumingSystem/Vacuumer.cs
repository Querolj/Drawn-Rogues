using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class Vacuumer : MonoBehaviour
{
    [SerializeField]
    private GameObject _vacuumerRenderer;

    [SerializeField]
    private ColouringInventory _colouringInventory;

    [SerializeField]
    private RewardUI _rewardUI;

    [SerializeField]
    private FailMessage _failMessage;

    private bool _active = false;
    private Camera _mainCamera;
    private Attackable _focusedAttackable = null;
    private Vacuumable _focusedVacuumable = null;
    private CombatZone _combatZone;

    private Dictionary<int, Vacuumable> _vacuumableByAttackableId = new Dictionary<int, Vacuumable> (); // cache
    public event Action OnVacuumDone;
    private BaseColorInventory _baseColorInventory;

    [Inject, UsedImplicitly]
    private void Init (BaseColorInventory baseColorInventory)
    {
        _baseColorInventory = baseColorInventory;
    }

    private void Awake ()
    {
        _vacuumerRenderer.SetActive (false);
        _mainCamera = Camera.main;
    }

    public void ActivateVacuumer (CombatZone combatZone)
    {
        _active = true;
        _vacuumerRenderer.SetActive (true);
        _combatZone = combatZone;
        foreach (Attackable attackable in _combatZone.AttackablesInZone)
        {
            attackable.OnMouseEntered += UpdateTarget;
            attackable.OnMouseExited += RemoveTarget;
        }
    }

    public void DeactivateVacuumer ()
    {
        _active = false;
        _vacuumerRenderer.SetActive (false);
        _combatZone = null;
    }

    private void UpdateTarget (Attackable attackable)
    {
        if (Utils.IsPointerOverUIElement ())
        {
            return;
        }

        attackable.DisplayOutline (Color.green);
        if (_focusedAttackable != null)
            _focusedAttackable.StopDisplayOutline ();

        _focusedAttackable = attackable;
        int id = attackable.GetInstanceID ();
        if (!_vacuumableByAttackableId.TryGetValue (id, out Vacuumable vacuumable))
        {
            vacuumable = attackable.GetComponent<Vacuumable> ();
            _vacuumableByAttackableId.Add (id, vacuumable);
        }

        _focusedVacuumable = vacuumable;
    }

    private void RemoveTarget (Attackable attackable)
    {
        attackable.StopDisplayOutline ();
        _focusedAttackable = null;
        _focusedVacuumable = null;
    }

    private void Update ()
    {
        if (!_active)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame && _focusedAttackable != null && _focusedVacuumable != null)
        {
            _focusedAttackable.StopDisplayOutline ();
            TryVacuumeFocusedAttackable ();
        }
    }

    private void TryVacuumeFocusedAttackable ()
    {
        _active = false;

        bool isVacuumed = UnityEngine.Random.Range (0f, 1f) < _focusedVacuumable.VacuumingBaseChance;

        if (!isVacuumed)
        {
            Debug.Log ("Vacuuming failed");
            _failMessage.Display ("Vacuuming failed", OnVacuumDone);
            return;
        }

        Debug.Log ("Vacuuming successfull");

        // Loot colourings
        List<Colouring> colouringsLooted = new List<Colouring> ();
        foreach (ColouringReward colouringReward in _focusedVacuumable.ColouringsReward)
        {
            if (_colouringInventory.HasColouring (colouringReward.Colouring))
                continue;

            if (UnityEngine.Random.Range (0f, 1f) < colouringReward.ChanceToDrop)
            {
                Debug.Log ("Dropped colouring " + colouringReward.Colouring);
                _colouringInventory.AddColouring (colouringReward.Colouring);
                colouringsLooted.Add (colouringReward.Colouring);
            }
        }

        // Loot color drops
        Dictionary<BaseColor, int> colorDropsLooted = new Dictionary<BaseColor, int> ();
        foreach (PixelDropQuantity colorDropQuantity in _focusedVacuumable.ColorDropsReward)
            colorDropsLooted.Add (colorDropQuantity.BaseColor, colorDropQuantity.Quantity);

        _baseColorInventory.AddColorDrops (colorDropsLooted);

        // display rewards
        _rewardUI.Display (colorDropsLooted, colouringsLooted, OnVacuumDone);
    }
}