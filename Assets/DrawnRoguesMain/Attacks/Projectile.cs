using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (SpriteRenderer), typeof (SpriteAnimation))]
public class Projectile : MonoBehaviour
{
    private const float ACCELERATION_FROM_DIST = 100f;
    private float _speed;
    private float _lerpValue = 0f;
    private Action<bool> _onProjectileTrajectoryEnd; // bool : did the projectile made it to its destination
    private List<Vector3> _trajectoryPoints;
    private float _baseAcceleration;
    private bool _projectileDestroyed = false;
    private bool _isEnemyProjectile = false;
    private Vector3 _direction;

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
        if (_projectileDestroyed)
            return;

        _lerpValue += Mathf.SmoothStep (0f, 1f, Time.fixedDeltaTime * _speed);

        if (_lerpValue >= 1f)
        {
            _projectileDestroyed = true;
            _onProjectileTrajectoryEnd.Invoke (true);
            Destroy (gameObject);
            return;
        }

        int lerpedIndex = (int) Mathf.Lerp (0, _trajectoryPoints.Count - 1, _lerpValue);
        if (lerpedIndex > 0)
        {
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
            _onProjectileTrajectoryEnd.Invoke (false);
            IPlayerProjectileDefense playerProjectileDefense = other.GetComponent<Attackable> () as IPlayerProjectileDefense;
            playerProjectileDefense.ReportImpactPoint (transform.position - _direction * 0.1f);
        }

    }
}