using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnAnnouncer : MonoBehaviour
{
    [SerializeField]
    private Image _turnAnnouncerBackground;

    [SerializeField]
    private TMP_Text _turnAnnouncerText;

    private Color _playerTurnColor = new Color (18f / 255f, 34f / 255f, 1f, 1f);

    private Color _enemyTurnColor = new Color (219f / 255f, 57f / 255f, 61f / 255f, 1f);
    public void SetText (Character character, bool isPlayerTurn)
    {
        _turnAnnouncerText.text = character.Name + "'s turn";
        _turnAnnouncerBackground.color = isPlayerTurn ? _playerTurnColor : _enemyTurnColor;
    }

}