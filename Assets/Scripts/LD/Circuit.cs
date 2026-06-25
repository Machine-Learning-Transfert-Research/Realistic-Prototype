using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehiclePhysics;

public class Circuit : MonoBehaviour
{
    [SerializeField] private List<Road> m_roads = new List<Road>();
    [SerializeField] private Transform m_spawnPoint;
    private Transform m_agent;


    public delegate void OnCircuitFinished(AgentML agent, Circuit circuit);
    public OnCircuitFinished onCircuitFinished;

    public float GetDistanceFromRoad(Transform car)
    {
        return m_roads.Min(e => e.GetDistanceToSpline(car));
    }

    public List<Road> GetRoads()
    {
        return m_roads;
    }

    public Transform GetCarSpawnPoint()
    {
        return m_spawnPoint;
    }

    public void SpawnAgent(Transform agent)
    {
        m_agent = agent;

        Rigidbody rb = agent.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        VPVehicleController vpVehicule = agent.GetComponent<VPVehicleController>();
        vpVehicule.Reposition(m_spawnPoint.position, m_spawnPoint.rotation);
        rb.position = m_spawnPoint.position;
       
    }

    public void EndCircuit()
    {
        AgentML agentML = m_agent.GetComponent<AgentML>();
        if (agentML)
        {
            onCircuitFinished?.Invoke(agentML,this);
        }
    }

    public void ResetCircuit()
    {
        foreach (var road in m_roads)
        {
            road.ResetRoads();
            RandomizeSplinePoint randomize = road.GetComponent<RandomizeSplinePoint>();
            if (randomize != null)
            {
                randomize.Randomize();
            }
            road.InitRoads(this);
        }

        m_roads[m_roads.Count - 1].SetEndCheckpoint();
    }

    public int GetCheckpointCount()
    {
        int value = 0;

        for (int i = 0; i < m_roads.Count; i++)
        {
            value += m_roads[i].GetCheckpoints().Count;
        }
        return value;
    }
}
