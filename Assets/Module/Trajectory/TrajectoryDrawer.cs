using System.Collections.Generic;
using UnityEngine;

public class TrajectoryDrawer : MonoBehaviour
{
    [SerializeField]
    private TrajectoryPoint _trajectoryPointTemplate;

    private GameObject _trajectoryPointsContainer = null;
    private List<TrajectoryPoint> _trajectoryPointsInstantiated = null;

    public void DrawTrajectory (List<Vector3> trajectoryPoints, float radius)
    {
        if (_trajectoryPointsContainer == null)
        {
            _trajectoryPointsContainer = new GameObject ("TrajectoryPointsContainer");
        }

        if (_trajectoryPointsInstantiated == null)
            _trajectoryPointsInstantiated = new List<TrajectoryPoint> ();

        int i = 0;

        foreach (Vector3 point in trajectoryPoints)
        {
            if (i < _trajectoryPointsInstantiated.Count)
            {
                _trajectoryPointsInstantiated[i].transform.position = point;
                _trajectoryPointsInstantiated[i].Init (radius);
                _trajectoryPointsInstantiated[i].gameObject.SetActive (true);
            }
            else
            {
                TrajectoryPoint tp = GameObject.Instantiate<TrajectoryPoint> (_trajectoryPointTemplate, point, Quaternion.identity);
                tp.transform.SetParent (_trajectoryPointsContainer.transform);
                tp.Init (radius);
                _trajectoryPointsInstantiated.Add (tp);
                tp.transform.position = point;
            }

            i++;
        }

        // deactivate unused trajectory points instances
        for (int j = i; j < _trajectoryPointsInstantiated.Count; j++)
        {
            _trajectoryPointsInstantiated[j].gameObject.SetActive (false);
        }
    }

    public void ClearTrajectory ()
    {
        if (_trajectoryPointsInstantiated != null)
        {
            for (int i = 0; i < _trajectoryPointsInstantiated.Count; i++)
            {
                _trajectoryPointsInstantiated[i].gameObject.SetActive (false);
            }
        }
    }
}