using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FightRegistry : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{
    #region Serialized Fields
    [SerializeField]
    private GameObject _content;

    [SerializeField]
    private TextLine _lineTemplate;

    [SerializeField]
    private RectTransform _scrollViewRect;

    [SerializeField]
    private string _playerTag = "Player";
    #endregion

    #region Private Fields
    private float spacingBetweenLines = 0;
    private float _contentHeight = 0;
    private bool _isZoomed = false;

    private const float TOP_SCROLL_VIEW_ZOOMED = -75;
    private const float TOP_SCROLL_VIEW_UNZOOMED = -275;

    private VerticalLayoutGroup _layout;
    #endregion

    #region Private Methods
    private void Awake ()
    {
        _layout = _content.GetComponent<VerticalLayoutGroup> ();
        if (_layout == null)
            throw new System.Exception ("FightDescription content must have a VerticalLayoutGroup component");

        _contentHeight = _layout.padding.top;
        spacingBetweenLines = _layout.spacing + _lineTemplate.GetComponent<RectTransform> ().rect.height;
    }

    private void AddLine (string text)
    {
        TextLine line = Instantiate (_lineTemplate);
        line.SetText (text);
        line.transform.SetParent (_content.transform);
        line.transform.localScale = Vector3.one;

        _contentHeight += spacingBetweenLines;
        StartCoroutine (RepositionContent ());
    }

    public void OnPointerExit (PointerEventData eventData)
    {
        _isZoomed = false;
        ZoomOut ();
    }

    public void OnPointerClick (PointerEventData eventData)
    {
        if (_isZoomed)
        {
            _isZoomed = false;
            ZoomOut ();
        }
        else
        {
            _isZoomed = true;
            ZoomIn ();
        }

    }

    private void ZoomIn ()
    {
        _scrollViewRect.offsetMax = new Vector2 (_scrollViewRect.offsetMax.x, TOP_SCROLL_VIEW_ZOOMED);
        StartCoroutine (RepositionContent ());
    }

    private void ZoomOut ()
    {
        _scrollViewRect.offsetMax = new Vector2 (_scrollViewRect.offsetMax.x, TOP_SCROLL_VIEW_UNZOOMED);
        StartCoroutine (RepositionContent ());
    }
    #endregion 

    #region Public methods

    public void ReportRoundStart (int roundNumber)
    {
        string text = "------ Starting round <b>" + roundNumber + "</b> ------";
        AddLine (text);
    }

    public void Clean ()
    {
        foreach (Transform child in _content.transform)
            Destroy (child.gameObject);
        _contentHeight = _layout.padding.top;
    }

    public void ReportAttackDamage (string attackerName, string targetName,
        AttackElement damageType, string attackName, int damage, string attackerTag, bool isCritical)
    {
        bool isAttackerPlayer = _playerTag == attackerTag;
        string coloredAttackerName = isAttackerPlayer ? GetColoredPlayerName (attackerName) : GetColoredEnemyName (attackerName);
        string coloredTargetName = isAttackerPlayer ? GetColoredEnemyName (targetName) : GetColoredPlayerName (targetName);
        string text;
        if (damageType == AttackElement.Heal)
            text = coloredAttackerName + " use <b>" + attackName + "</b> and heal " + coloredTargetName + " for <color=\"green\"><b>" + (damage * -1) + "</b></color> life.";
        else
            text = coloredAttackerName + " use <b>" + attackName + "</b> and damages " + coloredTargetName + " for <b>" + damage + "</b> damages.";
        
        if (isCritical)
            text += " <color=\"yellow\"><b>Critical!</b></color>";
        AddLine (text);
    }

    public void ReportAttackDamage (string targetName,
        AttackElement damageType, string attackName, int damage, string attackerTag)
    {
        bool isAttackerPlayer = _playerTag == attackerTag;
        string coloredTargetName = isAttackerPlayer ? GetColoredEnemyName (targetName) : GetColoredPlayerName (targetName);
        string text;
        if (damageType == AttackElement.Heal)
            text = attackName + "</b> is healing " + coloredTargetName + " for <color=\"green\"><b>" + (damage * -1) + "</b></color> life.";
        else
            text = attackName + "</b> is damaging " + coloredTargetName + " for <b>" + damage + "</b> damages.";
        AddLine (text);
    }

    public void ReportAttackUse (string attackerName, AttackableDescription targetDescription, string attackName, string attackerTag)
    {
        bool isAttackerPlayer = _playerTag == attackerTag;
        string coloredAttackerName = isAttackerPlayer ? GetColoredPlayerName (attackerName) : GetColoredEnemyName (attackerName);
        string targetName = isAttackerPlayer ? GetColoredEnemyName (attackerName) : GetColoredPlayerName (attackerName);
        string text = coloredAttackerName + " use <b>" + attackName + "</b> on " + targetName + ".";
        AddLine (text);
    }

    public void ReportAttackDodge (string attackerName, AttackableDescription targetDescription, string attackName, string attackerTag)
    {
        bool isAttackerPlayer = _playerTag == attackerTag;
        string coloredAttackerName = isAttackerPlayer ? GetColoredPlayerName (attackerName) : GetColoredEnemyName (attackerName);
        string targetName = isAttackerPlayer ? GetColoredEnemyName (attackerName) : GetColoredPlayerName (attackerName);
        string text = coloredAttackerName + " use <b>" + attackName + "</b> on " + targetName + ".";
        AddLine (text);
        text = targetName + " <b>dodged</b> the attack.";
        AddLine (text);
    }

    public string GetColoredAttackableName (string attackerName, string attackerTag)
    {
        bool isAttackerPlayer = _playerTag == attackerTag;
        string name = isAttackerPlayer ? GetColoredEnemyName (attackerName) : GetColoredPlayerName (attackerName);
        return name;
    }

    public string GetColoredPlayerName (string attackerName)
    {
        return "<color=\"green\">" + attackerName + "</color>";
    }

    public string GetColoredEnemyName (string attackerName)
    {
        return "<color=\"red\">" + attackerName + "</color>";
    }

    public void Report (string text)
    {
        AddLine (text);
    }

    private IEnumerator RepositionContent ()
    {
        yield return new WaitForEndOfFrame ();
        Vector3 newContentPos = _content.transform.localPosition;
        newContentPos.y = _contentHeight;
        _content.transform.localPosition = newContentPos;
    }
    #endregion
}