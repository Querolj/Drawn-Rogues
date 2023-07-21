using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (SpriteRenderer))]
public class SpriteAnimation : MonoBehaviour
{
    public enum AnimeDirection
    {
        Right,
        Left
    }
    private AnimeDirection _direction = AnimeDirection.Right;
    public AnimeDirection Direction
    {
        get
        {
            return _direction;
        }
        set
        {
            _direction = value;
            _renderer.flipX = _direction == AnimeDirection.Left;
        }
    }
    private SpriteRenderer _renderer;

    [SerializeField]
    private List<Sprite> _sprites;

    [SerializeField]
    private float _frameTime;

    [SerializeField]
    private int _maxLoopCount = 1;

    [SerializeField]
    private bool _autoStart = false;

    private float _currentFrameTime;
    private int _index = 0;
    public event Action OnAnimationEnded;
    private bool _start = false;
    private int _currentLoopCount = 0;

    private void Awake ()
    {
        _renderer = GetComponent<SpriteRenderer> ();
        _start = _autoStart;
        _currentFrameTime = _frameTime;
    }

    private void Update ()
    {
        if (!_start)
            return;

        _currentFrameTime -= Time.deltaTime;
        if (_currentFrameTime <= 0)
        {
            if (_maxLoopCount > 0 && _index + 1 >= _sprites.Count)
            {
                _currentLoopCount++;
                if (_currentLoopCount >= _maxLoopCount)
                {
                    OnAnimationEnded?.Invoke ();
                    DestroyImmediate (gameObject);
                    return;
                }
            }

            _index = (_index + 1) % _sprites.Count;
            _currentFrameTime = _frameTime;
        }

        _renderer.sprite = _sprites[_index];
    }

    public void Play ()
    {
        _start = true;
    }
}