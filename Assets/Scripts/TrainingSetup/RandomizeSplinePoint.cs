using Dreamteck.Splines;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PointModification
{
    public int pointIndex;
    public bool useRandomSign;
    public Vector3 minPositionLocalOffset;
    public Vector3 maxPositionLocalOffset;
    [HideInInspector] public Vector3 baseLocalPosition;
}

[Serializable]
public struct CheckpointSnapData
{
    public double percent;
    public Vector3 positionOffset;
}

public class RandomizeSplinePoint : MonoBehaviour
{
    [SerializeField] private SplineComputer m_splineComputer;
    [SerializeField] private List<PointModification> m_pointsModification = new List<PointModification>();
    [SerializeField] private List<SplineComputer> splinesToCopyTo;

    private bool m_isInit = false;
    private List<CheckpointSnapData> m_checkpointsSnapData = new List<CheckpointSnapData>();
    private List<Checkpoint> m_checkpointsToSnap;
    void Init()
    {
        Road roadComp = GetComponent<Road>();
        m_checkpointsToSnap = roadComp.GetCheckpoints();

        for (int i = 0; i < m_pointsModification.Count; i++)
        {
            PointModification pointModification = m_pointsModification[i];
            pointModification.baseLocalPosition = m_splineComputer.GetPointPosition(pointModification.pointIndex, SplineComputer.Space.Local);
            m_pointsModification[i] = pointModification;
        }

        for (int i = 0; i < m_checkpointsToSnap.Count; i++)
        {
            SplineSample projection = m_splineComputer.Project(m_checkpointsToSnap[i].transform.position);
            CheckpointSnapData snapData = new CheckpointSnapData();
            snapData.percent = projection.percent;
            snapData.positionOffset = m_checkpointsToSnap[i].transform.position - projection.position;
            m_checkpointsSnapData.Add(snapData);
        }

        m_isInit = true;
    }

    public void Randomize()
    {
        if (!m_isInit)
        {
            Init();
        }

        StartCoroutine(Cor_Randomize());
    }

    IEnumerator Cor_Randomize()
    {
        RandomizeSpline();
        yield return null;
        SnapCheckpoints();
    }

    private void Update()
    {
        foreach (SplinePoint point in m_splineComputer.GetPoints())
        {
            Debug.DrawLine(point.position, point.position + Vector3.up * 10);
        }
    }

    private void RandomizeSpline()
    {
        for (int i = 0; i < m_pointsModification.Count; i++)
        {
            float randomX = UnityEngine.Random.Range(m_pointsModification[i].minPositionLocalOffset.x, m_pointsModification[i].maxPositionLocalOffset.x) * (m_pointsModification[i].useRandomSign ? RandomSign() : 1.0f);
            float randomY = UnityEngine.Random.Range(m_pointsModification[i].minPositionLocalOffset.y, m_pointsModification[i].maxPositionLocalOffset.y) * (m_pointsModification[i].useRandomSign ? RandomSign() : 1.0f);
            float randomZ = UnityEngine.Random.Range(m_pointsModification[i].minPositionLocalOffset.z, m_pointsModification[i].maxPositionLocalOffset.z) * (m_pointsModification[i].useRandomSign ? RandomSign() : 1.0f);

            Vector3 offset = new Vector3(randomX, randomY, randomZ);
            Vector3 newPointPosition = m_pointsModification[i].baseLocalPosition + offset;
            m_splineComputer.SetPointPosition(m_pointsModification[i].pointIndex, newPointPosition, SplineComputer.Space.Local);

            foreach (SplineComputer copy in splinesToCopyTo)
            {
                copy.SetPointPosition(m_pointsModification[i].pointIndex, newPointPosition, SplineComputer.Space.Local);
            }
        }
    }

    private float RandomSign()
    {
        return UnityEngine.Random.value > 0.5f ? 1.0f : -1.0f;
    }

    private void SnapCheckpoints()
    {
        for (int i = 0; i < m_checkpointsToSnap.Count; i++)
        {
            CheckpointSnapData snapData = m_checkpointsSnapData[i];
            SplineSample sample = m_splineComputer.Evaluate(snapData.percent);
            m_checkpointsToSnap[i].transform.position = sample.position + snapData.positionOffset;
            m_checkpointsToSnap[i].transform.rotation = Quaternion.LookRotation(sample.forward, sample.up);
        }
    }
}
