using Dreamteck.Splines;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Road : MonoBehaviour
{
    [SerializeField] private SplineComputer spline;
    [SerializeField] private Transform checkpointHolder;
    [SerializeField] private bool autoFillCheckpointsList = true;
    [SerializeField] private List<Checkpoint> checkpoints = new List<Checkpoint>();
     private Circuit m_circuitComp;


    private void OnValidate()
    {
        if (autoFillCheckpointsList)
        {
            checkpoints.Clear();
            checkpoints = checkpointHolder.GetComponentsInChildren<Checkpoint>().ToList();
        }
    }

    public float GetDistanceToSpline(Transform car)
    {
        SplineSample projection = spline.Project(car.transform.position);
        return Vector3.Distance(car.transform.position, projection.position);
    }

    public void InitRoads(Circuit circuit)
    {
        m_circuitComp = circuit;
    }
    
    public void ResetRoads()
    {
        for (int i = 0; i < checkpoints.Count; i++)
        {
            checkpoints[i].OnReset();
        }

        checkpoints[checkpoints.Count - 1].isEndTrack -= EndRoad;
    }

    public void SetEndCheckpoint()
    {
        checkpoints[checkpoints.Count - 1].SetEndCheckpoint();
        checkpoints[checkpoints.Count - 1].isEndTrack += EndRoad;
    }

    public void EndRoad()
    {
        checkpoints[checkpoints.Count - 1].isEndTrack -= EndRoad;
        m_circuitComp.EndCircuit();
    }

    public List<Checkpoint> GetCheckpoints()
    {
        return checkpoints;
    }

    public SplineComputer GetSpline()
    {
        return spline; 
    }
}
