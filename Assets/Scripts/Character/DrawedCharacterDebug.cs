using UnityEngine;

public class DrawedCharacterDebug : DrawedCharacter
{
    [SerializeField]
    private int _life;

    [SerializeField]
    private int _intelligence;

    [SerializeField]
    private int _strenght;

    [SerializeField]
    private int _mobility;

    [SerializeField]
    private float _kilogram = 30;

    protected override void Awake ()
    {
        Stats = new Stats (_life, _intelligence, _strenght, _mobility, _kilogram);
        base.Awake ();
    }
}