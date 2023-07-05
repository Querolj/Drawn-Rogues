using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (SpriteRenderer), typeof (SpriteAnimation))]
public class Projectile : MonoBehaviour
{
    private const float ACCELERATION_FROM_DIST = 100f;
    private float _speed;
    private float _lerpValue = 0f;
    private Action<bool> _onProjectileDestroyed; // bool : did the projectile made it to its destination
    private List<Vector3> _trajectoryPoints;
    private float _accelerationProportionInTime = 0.5f;
    private float _baseAcceleration;
    private SpriteAnimation _animation;
    private bool _projectileDestroyed = false;
    private bool _isProjectileDodge = false;
    private bool _isEnemyProjectile = false;
    private Vector3 _direction;
    private void Awake ()
    {
        _animation = GetComponent<SpriteAnimation> ();
        _animation.OnAnimationEnded += () => Destroy (gameObject);
    }

    public void Init (List<Vector3> trajectoryPoints, float projectileSpeed, bool isProjectileDodge, bool isEnemy, Action<bool> onProjectileHit)
    {
        _onProjectileDestroyed = onProjectileHit ??
            throw new ArgumentNullException (nameof (onProjectileHit));
        if (projectileSpeed <= 0)
            throw new ArgumentException ("Projectile speed must be greater than 0");
        _speed = projectileSpeed;
        _trajectoryPoints = trajectoryPoints ??
            throw new ArgumentNullException (nameof (trajectoryPoints));

        _baseAcceleration = _speed;
        _isProjectileDodge = isProjectileDodge;
        _isEnemyProjectile = isEnemy;
    }

    private void FixedUpdate ()
    {
        if (_projectileDestroyed)
            return;

        _lerpValue += Time.fixedDeltaTime * _speed;

        if (_accelerationProportionInTime > 0f)
        {
            _lerpValue += Time.fixedDeltaTime * _baseAcceleration * _accelerationProportionInTime;
            _accelerationProportionInTime -= Time.fixedDeltaTime;
        }

        if (_lerpValue >= 1f)
        {
            _projectileDestroyed = true;
            _onProjectileDestroyed.Invoke (true);
            if (!_isProjectileDodge)
                _animation.Play ();
            else
                Destroy (gameObject);
            return;
        }

        // add speed the more the last two points are far from each other
        int lerpedIndex = (int) Mathf.Lerp (0, _trajectoryPoints.Count - 1, _lerpValue);
        if (lerpedIndex > 0)
        {
            _lerpValue += ACCELERATION_FROM_DIST * Vector3.Distance (_trajectoryPoints[lerpedIndex], _trajectoryPoints[lerpedIndex - 1]) * Time.fixedDeltaTime;
            _direction = _trajectoryPoints[lerpedIndex] - _trajectoryPoints[lerpedIndex - 1];
            _direction.Normalize ();
        }

        transform.position = _trajectoryPoints[lerpedIndex];
    }

    private void OnTriggerEnter (Collider other)
    {
        if (_projectileDestroyed)
            return;

        if (_isEnemyProjectile && other.tag == "PlayerProjectileDefense")
        {
            _projectileDestroyed = true;
            _onProjectileDestroyed.Invoke (false);
            _animation.Play ();
            IPlayerProjectileDefense playerProjectileDefense = other.GetComponent<Attackable> () as IPlayerProjectileDefense;
            playerProjectileDefense.ReportImpactPoint (transform.position - _direction * 0.1f);
        }

    }
}