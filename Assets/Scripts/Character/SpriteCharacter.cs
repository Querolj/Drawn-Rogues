using UnityEngine;

public class SpriteCharacter : Character
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
        Stats = new AttackableStats (_life, _intelligence, _strenght, _mobility, _kilogram);
        base.Awake ();
    }

    public override Bounds GetSpriteBounds ()
    {
        return _renderer.bounds;
    }
}