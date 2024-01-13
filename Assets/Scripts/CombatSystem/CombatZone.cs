using System.Collections.Generic;
using Cinemachine;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

public class CombatZone : MonoBehaviour
{
    [SerializeField]
    private GameObject _playerSlot;

    [SerializeField]
    private CinemachineVirtualCamera _virtualCamera;
    private const int VIRTUAL_CAMERA_PRIORITY = 20;

    [SerializeField]
    private AttackableDetector _attackableDetector;
    public List<Attackable> AttackablesInZone { get { return _attackableDetector.AttackablesInZone; } }
    public List<Character> EnemiesInZone { get { return _attackableDetector.EnemiesCharactersInZone; } }

    [SerializeField]
    private CombatLine _combatLine;

    [SerializeField]
    private CombatLine _moveLine;

    [SerializeField]
    private Trigger _trigger;

    [SerializeField]
    private EscapeZone _escapeTemplate;

    private TurnManager _turnBasedCombat;
    private RewardUI _rewardUI;

    private Drawer _drawer;
    private PlayerController _playerController;
    private bool _fightStarted = false;
    public bool FightStarted { get { return _fightStarted; } }

    private Vector3 _centerPoint;
    private float _lineXMax;
    public float LineXMax { get { return _lineXMax; } }
    private float _lineXMin;
    public float LineXMin { get { return _lineXMin; } }
    public float SizeX { get { return Mathf.Abs (_lineXMax - _lineXMin); } }

    public Bounds Bounds { get { return _trigger.Bounds; } }
    private const float ESCAPE_ZONE_SIZE = 0.1f;
    private EscapeZone _escapeZoneLeft;
    private EscapeZone _escapeZoneRight;
    private MoveIndicator _moveIndicator;
    private BaseColorInventory _baseColorPalette;
    private WorldUIContainer _worldUIContainer;

    [Inject, UsedImplicitly]
    private void Init (MoveIndicator moveIndicator, BaseColorInventory baseColorPalette, WorldUIContainer worldUIContainer)
    {
        _baseColorPalette = baseColorPalette;
        _moveIndicator = moveIndicator;
        _worldUIContainer = worldUIContainer;
    }

    private void Awake ()
    {
        _turnBasedCombat = GameObject.FindObjectOfType<TurnManager> (); // TODO : Inject
        _drawer = GameObject.FindObjectOfType<Drawer> (); // TODO : Inject
        _playerController = GameObject.FindObjectOfType<PlayerController> (); // TODO : Inject
        _rewardUI = GameObject.FindObjectOfType<RewardUI> (true); // TODO : Inject

        _trigger.OnDetect += OnTriggerDetect;

        _combatLine.transform.position = new Vector3 (_trigger.transform.position.x, _combatLine.transform.position.y, _combatLine.transform.position.z);
        SetCombatZoneLimit ();
        Vector3 leftLimit = new Vector3 (_lineXMin + ESCAPE_ZONE_SIZE, 0f, _centerPoint.z);
        Vector3 mapLeftLimit = new Vector3 (_lineXMin, Utils.GetMapHeight (leftLimit), _centerPoint.z);
        mapLeftLimit.y += 0.2f;
        _escapeZoneLeft = Instantiate<EscapeZone> (_escapeTemplate, mapLeftLimit, Quaternion.identity, transform);
        _escapeZoneLeft.FlipIcon ();
        _escapeZoneLeft.gameObject.SetActive (false);
        _worldUIContainer.AddUI (_escapeZoneLeft.transform);

        Vector3 rightLimit = new Vector3 (_lineXMax - ESCAPE_ZONE_SIZE, 0f, _centerPoint.z);
        Vector3 mapRightLimit = new Vector3 (_lineXMax, Utils.GetMapHeight (rightLimit), _centerPoint.z);
        mapRightLimit.y += 0.2f;
        _escapeZoneRight = Instantiate<EscapeZone> (_escapeTemplate, mapRightLimit, Quaternion.identity, transform);
        _escapeZoneRight.gameObject.SetActive (false);
        _worldUIContainer.AddUI (_escapeZoneRight.transform);
    }

    private void SetCombatZoneLimit ()
    {
        _centerPoint = _trigger.Bounds.center;
        Vector3 centerExtentX = new Vector3 (_trigger.Bounds.extents.x, 0f, 0f);

        if (Physics.Raycast (_centerPoint + Vector3.up * 100f, Vector3.down, out RaycastHit hit, Mathf.Infinity, 1 << LayerMask.NameToLayer ("Map")))
        {
            _centerPoint.y = hit.point.y;
        }
        else
        {
            Debug.LogError ("SetCenterPointAndLineUv : No map found under combat zone");
            return;
        }

        if (Physics.Raycast (_centerPoint + Vector3.up * 100f + centerExtentX, Vector3.down, out RaycastHit hitRight, Mathf.Infinity, 1 << LayerMask.NameToLayer ("Map")))
        {
            _lineXMax = hitRight.point.x;
        }
        else
        {
            Debug.LogError ("SetCenterPointAndLineUv : No map found under combat zone");
            return;
        }

        if (Physics.Raycast (_centerPoint + Vector3.up * 100f - centerExtentX, Vector3.down, out RaycastHit hitLeft, Mathf.Infinity, 1 << LayerMask.NameToLayer ("Map")))
        {
            _lineXMin = hitLeft.point.x;
        }
        else
        {
            Debug.LogError ("SetCenterPointAndLineUv : No map found under combat zone");
            return;
        }
    }

    private void OnTriggerDetect (Collider other)
    {
        if (_fightStarted) return;

        if (other.tag == "Player")
        {
            // stop player controller movement input
            _playerController.SetMoveMode (PlayerController.ControlMode.None);

            // hide trigger renderer
            _trigger.HideRenderer ();

            // move player to combat zone
            CharacterMovement charMove = other.gameObject.GetComponentInParent<CharacterMovement> ();
            charMove.MoveToTarget (_playerSlot.transform.position, InitTurnBasedCombat);

            // move camera to combat zone
            _virtualCamera.Priority = VIRTUAL_CAMERA_PRIORITY;

            // set the context to draw on the map
            _drawer.StopBrushDrawingPrediction ();
            _drawer.StopCurrentDrawing ();

            // show the combat zone
            DrawCombatZoneLineOnMap ();

            // Set escape route
            _moveIndicator.OnPositionSet += ActivateEscapeRoute;
        }
    }

    public void EndFight (bool escaped = false)
    {
        _virtualCamera.Priority = 0;
        _combatLine.gameObject.SetActive (false);
        _moveLine.gameObject.SetActive (false);
        _escapeZoneLeft.gameObject.SetActive (false);
        _escapeZoneRight.gameObject.SetActive (false);
        _moveIndicator.OnPositionSet -= ActivateEscapeRoute;
        _playerController.RemoveAllTempEffectOnChar ();
        if (!escaped)
            CollectReward ();
    }

    private void CollectReward ()
    {
        Reward[] rewards = GetComponentsInChildren<Reward> ();
        Dictionary<BaseColor, int> colorDrops = new Dictionary<BaseColor, int> ();
        int totalXpGained = 0;

        foreach (Reward reward in rewards)
        {
            foreach (ColorDropQuantity colorDrop in reward.ColorDropsReward)
            {
                if (colorDrops.ContainsKey (colorDrop.BaseColor))
                    colorDrops[colorDrop.BaseColor] += colorDrop.Quantity;
                else
                    colorDrops.Add (colorDrop.BaseColor, colorDrop.Quantity);
            }
            _baseColorPalette.AddColorDrops (colorDrops);
            totalXpGained += reward.XpToGain;
        }

        _rewardUI.Display (colorDrops, null, () =>
        {
            _playerController.AddXp (totalXpGained);
        });
    }

    public void EscapeFight ()
    {
        EndFight (true);
        _trigger.ShowRenderer ();
        _fightStarted = false;
    }

    private void InitTurnBasedCombat ()
    {
        _turnBasedCombat.InitCombat (this);
        _fightStarted = true;
    }

    private void DrawCombatZoneLineOnMap ()
    {
        if (!_combatLine.gameObject.activeSelf)
            _combatLine.gameObject.SetActive (true);

        _combatLine.SetWidth (Mathf.Abs (_lineXMax - _lineXMin) - 2 * ESCAPE_ZONE_SIZE);
    }

    public (float, float) GetMoveZoneLimitOnMap (float distMoveMax, Character character)
    {
        if (distMoveMax <= 0)
            return (character.Pivot.transform.position.x, character.Pivot.transform.position.x);

        Bounds bounds = character.GetSpriteBounds ();
        Vector3 startingPos = bounds.center;
        startingPos.z = character.transform.position.z;
        startingPos.y += bounds.extents.y;

        float xPosRight = CastCharacterInZone (distMoveMax, character, character.CharMovement.DirectionRight, startingPos);
        float xPosLeft = CastCharacterInZone (distMoveMax, character, !character.CharMovement.DirectionRight, startingPos);

        if (!character.CharMovement.DirectionRight)
        {
            float tmp = xPosRight;
            xPosRight = xPosLeft;
            xPosLeft = tmp;
        }

        if (xPosLeft < _lineXMin)
        {
            xPosLeft = _lineXMin;
        }
        if (xPosRight > _lineXMax)
        {
            xPosRight = _lineXMax;
        }

        return (xPosLeft, xPosRight);
    }

    public float CastCharacterInZone (float distMoveMax, Character character, bool senseRight, Vector3 startingPos, HashSet<int> idToIgnore = null)
    {
        Bounds bounds = (Bounds) character.GetSpriteBounds ();
        HashSet<int> allIdsToIgnore = new HashSet<int> ();
        if (idToIgnore != null)
            allIdsToIgnore.UnionWith (idToIgnore);
        allIdsToIgnore.Add (character.gameObject.GetInstanceID ());
        allIdsToIgnore.Add (character.ColliderId);

        Vector3 currentMapPos = Vector3.zero;
        Quaternion currentRotation;
        Physics.SyncTransforms ();

        Debug.DrawLine (startingPos, startingPos + Vector3.down * 100f, Color.red, 5f);
        if (Physics.Raycast (startingPos, Vector3.down, out RaycastHit firstRayHit, Mathf.Infinity, 1 << LayerMask.NameToLayer ("Map")))
        {
            currentMapPos = firstRayHit.point;
            currentRotation = Quaternion.LookRotation (firstRayHit.normal);
        }
        else
        {
            return currentMapPos.x;
        }

        float accumulatedDist = 0f;
        int id = character.gameObject.GetInstanceID ();
        Vector3 oldMapPos = currentMapPos;

        while (accumulatedDist <= distMoveMax)
        {
            if (oldMapPos.x < _lineXMin || oldMapPos.x > _lineXMax)
                return oldMapPos.x;

            Vector3 boxCastStart = currentMapPos + Vector3.up * bounds.extents.y;
            boxCastStart.z = character.transform.position.z;
            Debug.DrawLine (boxCastStart, boxCastStart + (senseRight ? character.transform.right : -character.transform.right) * bounds.extents.x, (senseRight ? Color.green : Color.red), 5f);
            Collider[] hitboxes = Physics.OverlapBox (boxCastStart, bounds.extents, currentRotation, 1 << LayerMask.NameToLayer ("Attackable"));

            foreach (Collider hitbox in hitboxes)
            {
                int hitId = hitbox.gameObject.GetInstanceID ();
                if (!allIdsToIgnore.Contains (hitId))
                {
                    return oldMapPos.x;
                }
            }

            oldMapPos = currentMapPos;

            // Vector3 raycastStart = currentMapPos + (senseRight ? Vector3.right : Vector3.left) * bounds.extents.x;
            Vector3 raycastStart = currentMapPos + (senseRight ? Vector3.right : Vector3.left) * 0.02f;

            raycastStart.y += 20f;
            Debug.DrawLine (raycastStart, raycastStart + Vector3.down * 100f, Color.blue, 5f);
            if (Physics.Raycast (raycastStart, Vector3.down, out RaycastHit rayHit, Mathf.Infinity, 1 << LayerMask.NameToLayer ("Map")))
            {
                currentMapPos = rayHit.point;
                currentRotation = Quaternion.LookRotation (rayHit.normal);
            }
            else
            {
                return oldMapPos.x;
            }

            accumulatedDist += Vector3.Distance (currentMapPos, oldMapPos);
            // Debug.Log ("accumulatedDist : " + accumulatedDist + ", distMoveMax : " + distMoveMax);
        }

        return oldMapPos.x;
    }

    public bool TryCastCharacterToAttackable (float distMoveMax, Character character, Character attackable, Vector3 startingPos, out Attackable attackableHit)
    {
        if (attackable == null)
            throw new System.ArgumentNullException (nameof (attackable));

        if (attackable == character)
            throw new System.ArgumentException ("Player and character are the same", nameof (attackable));

        attackableHit = null;
        Bounds bounds = (Bounds) character.GetSpriteBounds ();
        Vector3 currentMapPos = Vector3.zero;
        bool senseRight = attackable.transform.position.x > character.transform.position.x;
        startingPos.x += (senseRight ? 1 : -1) * bounds.extents.x;

        Debug.DrawLine (startingPos, startingPos + Vector3.down * 100f, Color.red, 5f);
        if (Physics.Raycast (startingPos, Vector3.down, out RaycastHit firstRayHit, Mathf.Infinity, 1 << LayerMask.NameToLayer ("Map")))
        {
            currentMapPos = firstRayHit.point;
        }
        else
        {
            return false;
        }

        float accumulatedDist = 0f;
        int charId = character.gameObject.GetInstanceID ();
        Vector3 oldMapPos = currentMapPos;

        while (accumulatedDist <= distMoveMax)
        {
            if (oldMapPos.x < _lineXMin || oldMapPos.x > _lineXMax)
            {
                return false;
            }

            Vector3 boxCastStart = currentMapPos + Vector3.up * bounds.extents.y;
            boxCastStart.z = character.transform.position.z;

            Collider[] hitboxes = Physics.OverlapBox (boxCastStart, bounds.extents, Quaternion.identity, 1 << LayerMask.NameToLayer ("Attackable"));
            Debug.DrawLine (boxCastStart, boxCastStart + (senseRight ? character.transform.right : -character.transform.right) * bounds.extents.x, (senseRight ? Color.green : Color.red), 5f);
            foreach (Collider hitbox in hitboxes)
            {
                Attackable attackableTouched = hitbox.GetComponentInParent<Attackable> ();

                int hitId = attackableTouched.gameObject.GetInstanceID ();
                if (hitId == charId) continue;

                attackableHit = attackableTouched;

                if (hitbox != null && hitId == attackable.gameObject.GetInstanceID ())
                {
                    return true;
                }

                return false;
            }

            oldMapPos = currentMapPos;

            Vector3 raycastStart = currentMapPos + (senseRight ? Vector3.right : Vector3.left) * bounds.extents.x;
            raycastStart.y += 20f;
            Debug.DrawLine (raycastStart, raycastStart + Vector3.down * 100f, Color.blue, 100f);
            if (Physics.Raycast (raycastStart, Vector3.down, out RaycastHit rayHit, Mathf.Infinity, 1 << LayerMask.NameToLayer ("Map")))
            {
                currentMapPos = rayHit.point;
            }
            else
            {
                return false;
            }

            accumulatedDist += Vector3.Distance (currentMapPos, oldMapPos);
            // Debug.Log ("accumulatedDist : " + accumulatedDist + ", distMoveMax : " + distMoveMax);
        }

        return false;
    }

    public void DrawMoveLineOnMap (float xPosLeft, float xPosRight, Bounds bounds)
    {
        if (!_moveLine.gameObject.activeSelf)
            _moveLine.gameObject.SetActive (true);

        xPosLeft = Mathf.Clamp (xPosLeft, _lineXMin + ESCAPE_ZONE_SIZE, _lineXMax - ESCAPE_ZONE_SIZE);
        xPosRight = Mathf.Clamp (xPosRight, _lineXMin + ESCAPE_ZONE_SIZE, _lineXMax - ESCAPE_ZONE_SIZE);

        Vector3 linePos = _moveLine.transform.position;
        linePos.x = Mathf.Lerp (xPosLeft, xPosRight, 0.5f);
        _moveLine.transform.position = linePos;
        _moveLine.SetWidth (Mathf.Abs (xPosRight - xPosLeft) + bounds.size.x);
    }

    public void StopDrawMoveZoneLineOnMap ()
    {
        _moveLine.gameObject.SetActive (false);
    }

    private void ActivateEscapeRoute (Vector3 position)
    {
        if (position.x < _lineXMin + ESCAPE_ZONE_SIZE)
        {
            _escapeZoneLeft.gameObject.SetActive (true);
            _escapeZoneLeft.SetChanceOfEscape (GetChanceOfEscape ());
        }
        else if (_escapeZoneLeft.gameObject.activeSelf)
            _escapeZoneLeft.gameObject.SetActive (false);

        if (position.x > _lineXMax - ESCAPE_ZONE_SIZE)
        {
            _escapeZoneRight.gameObject.SetActive (true);
            _escapeZoneRight.SetChanceOfEscape (GetChanceOfEscape ());
        }
        else if (_escapeZoneRight.gameObject.activeSelf)
            _escapeZoneRight.gameObject.SetActive (false);
    }

    public bool IsInEscapeZone (Vector3 position)
    {
        return position.x < _lineXMin + ESCAPE_ZONE_SIZE || position.x > _lineXMax - ESCAPE_ZONE_SIZE;
    }

    public float GetChanceOfEscape ()
    {
        float chance = 1f;
        foreach (Character character in EnemiesInZone)
        {
            chance -= character.ReduceEscapeChanceValue;
        }

        return Mathf.Clamp (chance, 0f, 1f);
    }
}