using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent (typeof (SpriteRenderer), typeof (SpriteAnimation))]
public class Projectile : MonoBehaviour
{
    [SerializeField, BoxGroup ("References")]
    private SpriteRenderer _spriteRenderer;

    [SerializeField, BoxGroup ("References")]
    private ParticleSystem _trailParticleSystem;

    private const float ACCELERATION_FROM_DIST = 100f;
    private float _speed;
    private float _wholeTrajectorylerpValue = 0f;
    private Action<bool> _onProjectileTrajectoryEnd; // bool : did the projectile made it to its destination
    private List<Vector3> _trajectoryPoints;
    private float _baseAcceleration;
    private bool _projectileWillBeDestroyed = false;
    private bool _isEnemyProjectile = false;
    private float _timerUntilDestroy = 0f;

    public float Radius
    {
        get
        {
            return _spriteRenderer.bounds.extents.x;
        }
    }

    public void Init (List<Vector3> trajectoryPoints, float projectileSpeed, bool isEnemy, Action<bool> onProjectileTrajectoryEnd)
    {
        _onProjectileTrajectoryEnd = onProjectileTrajectoryEnd ??
            throw new ArgumentNullException (nameof (onProjectileTrajectoryEnd));
        if (projectileSpeed <= 0)
            throw new ArgumentException ("Projectile speed must be greater than 0");
        _trajectoryPoints = trajectoryPoints ??
            throw new ArgumentNullException (nameof (trajectoryPoints));

        _speed = projectileSpeed / TrajectoryPointsLenght ();
        float dist = Vector3.Distance (_trajectoryPoints[0], _trajectoryPoints[_trajectoryPoints.Count - 1]);
        _baseAcceleration = _speed;
        _isEnemyProjectile = isEnemy;
        _timerUntilDestroy = _trailParticleSystem.main.startLifetime.constantMax;
    }

    private float TrajectoryPointsLenght ()
    {
        float lenght = 0f;
        for (int i = 0; i < _trajectoryPoints.Count - 1; i++)
        {
            lenght += Vector3.Distance (_trajectoryPoints[i], _trajectoryPoints[i + 1]);
        }
        return lenght;
    }

    private void FixedUpdate ()
    {
        if (_projectileWillBeDestroyed)
        {
            _timerUntilDestroy -= Time.fixedDeltaTime;
            if (_timerUntilDestroy <= 0f)
            {
                Destroy (gameObject);
            }
            return;
        }

        _wholeTrajectorylerpValue += Mathf.SmoothStep (0f, 1f, Time.fixedDeltaTime * _speed);

        if (_wholeTrajectorylerpValue >= 1f)
        {
            _projectileWillBeDestroyed = true;
            ParticleSystem.EmissionModule emission = _trailParticleSystem.emission;
            emission.enabled = false;
            _spriteRenderer.enabled = false;
            _onProjectileTrajectoryEnd.Invoke (true);
            return;
        }

        float indexFloat = (_trajectoryPoints.Count - 1) * _wholeTrajectorylerpValue;
        int index = (int) (indexFloat);
        float lerp = indexFloat - index;

        transform.position =  Vector3.Lerp (_trajectoryPoints[index], _trajectoryPoints[index + 1], lerp);
    }

    private void OnTriggerEnter (Collider other)
    {
        if (_projectileWillBeDestroyed)
            return;

        if (_isEnemyProjectile && other.tag == "PlayerProjectileDefense")
        {
            _projectileWillBeDestroyed = true;
            _onProjectileTrajectoryEnd.Invoke (false);
            IPlayerProjectileDefense playerProjectileDefense = other.GetComponent<Attackable> () as IPlayerProjectileDefense;
            Vector3 impactDirection = _trajectoryPoints[_trajectoryPoints.Count - 1] - _trajectoryPoints[_trajectoryPoints.Count - 2];
            impactDirection.Normalize ();
            playerProjectileDefense.ReportImpactPoint (transform.position - impactDirection * 0.1f);
        }

    }
}