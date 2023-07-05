using System.Collections.Generic;
using UnityEngine;

public class TrajectoryCalculator
{
    private const float SMASH_SPEED_MULT = 4f;
    private const float SMASH_HEIGHT = 1f;

    public bool TryGetSmashTrajectory (Vector3 startPosition, Vector3 targettedPos, float projectileSpeed,
        float radius, int instanceIdToAvoid, out Attackable attackableHit, out List<Vector3> trajectoryPoints)
    {
        attackableHit = null;
        targettedPos.y += SMASH_HEIGHT; // start higher then smash down

        Vector3 center = (startPosition + targettedPos) / 2f;
        if (targettedPos.x < startPosition.x)
            center -= Vector3.Cross (targettedPos - startPosition, Vector3.forward).normalized * SMASH_HEIGHT;
        else
            center += Vector3.Cross (targettedPos - startPosition, Vector3.forward).normalized * SMASH_HEIGHT;

        Vector3 projRelCenter = startPosition - center;
        Vector3 targetRelCenter = targettedPos - center;

        float lerpValue = 0f;
        Vector3 lastLerpedPos = Vector3.Slerp (projRelCenter, targetRelCenter, 0f);
        trajectoryPoints = new List<Vector3> ();
        while (lerpValue < 1f)
        {
            lerpValue += Time.deltaTime * projectileSpeed;
            Vector3 lerpedPos = Vector3.Slerp (projRelCenter, targetRelCenter, lerpValue);
            lerpedPos += center;

            if (TrySphereCast (lerpedPos, lastLerpedPos, radius, 1 << LayerMask.NameToLayer ("Attackable"), instanceIdToAvoid, out Vector3 hitPos, out attackableHit))
            {
                hitPos.z = center.z;
                trajectoryPoints.Add (hitPos);

                return false;
            };

            trajectoryPoints.Add (lerpedPos);

            lastLerpedPos = lerpedPos;
        }

        // smash down 
        if (TrySphereCast (lastLerpedPos, lastLerpedPos + Vector3.down * 10f, radius, 1 << LayerMask.NameToLayer ("Attackable"), instanceIdToAvoid, out Vector3 hitPos2, out attackableHit))
        {
            lerpValue = 0f;

            while (lerpValue < 1f)
            {
                Vector3 smashLerpedPos = Vector3.Lerp (lastLerpedPos, hitPos2, lerpValue);
                smashLerpedPos.z = center.z;
                trajectoryPoints.Add (smashLerpedPos);

                lerpValue += Time.deltaTime * projectileSpeed * SMASH_SPEED_MULT;
            }
        }
        else if (TryDoubleRayCastOnMap (lastLerpedPos, lastLerpedPos + Vector3.down * 10f, radius, out Vector3 smashHitPos))
        {
            lerpValue = 0f;
            while (lerpValue < 1f)
            {
                Vector3 smashLerpedPos = Vector3.Lerp (lastLerpedPos, smashHitPos, lerpValue);
                smashLerpedPos.z = center.z;
                trajectoryPoints.Add (smashLerpedPos);

                lerpValue += Time.deltaTime * projectileSpeed * SMASH_SPEED_MULT;
            }
        }
        else
            throw new System.Exception ("Smash trajectory did not hit anything, but it should have.");

        return true;
    }

    public List<Vector3> GetCurvedTrajectory (Vector3 startPosition, Vector3 targettedPos, float projectileSpeed,
        float radius, int goIdToAvoid, out Attackable attackableHit)
    {
        attackableHit = null;

        Vector3 center = (startPosition + targettedPos) / 2f;
        if (targettedPos.x < startPosition.x)
            center -= Vector3.Cross (targettedPos - startPosition, Vector3.forward).normalized * 0.01f;
        else
            center += Vector3.Cross (targettedPos - startPosition, Vector3.forward).normalized * 0.01f;

        // Debug.Log ("Center: " + center + " start pos: " + startPosition + " target pos: " + targettedPos + " cross: " + Vector3.Cross (targettedPos - startPosition, Vector3.forward).normalized * 0.01f);
        Vector3 projRelCenter = startPosition - center;
        Vector3 targetRelCenter = targettedPos - center;

        float lerpValue = 0f;
        Vector3 lastLerpedPos = Vector3.Slerp (projRelCenter, targetRelCenter, 0f);
        List<Vector3> trajectoryPoints = new List<Vector3> ();
        while (lerpValue < 1f)
        {
            lerpValue += Time.deltaTime * projectileSpeed;
            Vector3 lerpedPos = Vector3.Slerp (projRelCenter, targetRelCenter, lerpValue);
            lerpedPos += center;
            Debug.DrawLine (lerpedPos, lastLerpedPos, Color.red, 10f);
            if (TrySphereCast (lerpedPos, lastLerpedPos, radius, 1 << LayerMask.NameToLayer ("Attackable"), goIdToAvoid, out Vector3 hitPos, out attackableHit))
            {
                hitPos.z = center.z;
                trajectoryPoints.Add (hitPos);

                return trajectoryPoints;
            };

            trajectoryPoints.Add (lerpedPos);

            lastLerpedPos = lerpedPos;
        }

        return trajectoryPoints;
    }

    public List<Vector3> GetCurvedTrajectory (Vector3 startPosition, Vector3 targettedPos, float projectileSpeed,
        float radius, int goIdToAvoid)
    {
        Vector3 center = (startPosition + targettedPos) / 2f;
        if (targettedPos.x < startPosition.x)
            center -= Vector3.Cross (targettedPos - startPosition, Vector3.forward).normalized * 0.01f;
        else
            center += Vector3.Cross (targettedPos - startPosition, Vector3.forward).normalized * 0.01f;

        // Debug.Log ("Center: " + center + " start pos: " + startPosition + " target pos: " + targettedPos + " cross: " + Vector3.Cross (targettedPos - startPosition, Vector3.forward).normalized * 0.01f);
        Vector3 projRelCenter = startPosition - center;
        Vector3 targetRelCenter = targettedPos - center;

        float lerpValue = 0f;
        Vector3 lastLerpedPos = Vector3.Slerp (projRelCenter, targetRelCenter, 0f);
        List<Vector3> trajectoryPoints = new List<Vector3> ();
        while (lerpValue < 1f)
        {
            lerpValue += Time.deltaTime * projectileSpeed;
            Vector3 lerpedPos = Vector3.Slerp (projRelCenter, targetRelCenter, lerpValue);
            lerpedPos += center;
            trajectoryPoints.Add (lerpedPos);
            lastLerpedPos = lerpedPos;
        }

        return trajectoryPoints;
    }

    private Dictionary<int, Attackable> _attackableCached = new Dictionary<int, Attackable> ();
    private bool TrySphereCast (Vector3 origin, Vector3 target, float projectileRadius, int layerMask, int goIdToAvoid, out Vector3 hitPos, out Attackable attackableHit)
    {
        attackableHit = null;
        hitPos = Vector3.zero;

        Vector3 direction = target - origin;
        float distance = direction.magnitude;
        direction.Normalize ();

        RaycastHit[] hits = Physics.SphereCastAll (origin, projectileRadius, direction, distance, layerMask : layerMask);
        if (hits == null || hits.Length == 0)
            return false;
        foreach (RaycastHit hit in hits)
        {
            int id = hit.transform.gameObject.GetInstanceID ();

            if (id != goIdToAvoid && hit.transform.gameObject.tag != "Player")
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer ("Attackable"))
                {
                    if (_attackableCached.ContainsKey (id))
                    {
                        attackableHit = _attackableCached[id];
                    }
                    else
                    {
                        attackableHit = hit.transform.GetComponentInParent<Attackable> ();
                        _attackableCached.Add (id, attackableHit);
                    }
                }
                hitPos = hit.point;
                return true;
            }
        }

        return false;
    }

    private bool TryDoubleRayCastOnMap (Vector3 origin, Vector3 target, float radius, out Vector3 hitPos)
    {
        hitPos = Vector3.zero;

        Vector3 direction = target - origin;
        float distance = direction.magnitude;
        direction.Normalize ();

        Vector3 extends = new Vector3 (radius, radius, 0.001f);
        Physics.Raycast (origin + Vector3.right * radius, direction, out RaycastHit rightHit, Mathf.Infinity, layerMask : 1 << LayerMask.NameToLayer ("Map"));
        Physics.Raycast (origin + Vector3.left * radius, direction, out RaycastHit leftHit, Mathf.Infinity, layerMask : 1 << LayerMask.NameToLayer ("Map"));

        if (rightHit.transform != null && leftHit.transform != null)
        {
            hitPos = (rightHit.point + leftHit.point) / 2f;
            return true;
        }

        return false;
    }
}