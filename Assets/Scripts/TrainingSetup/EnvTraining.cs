using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Evaluation;
using Unity.InferenceEngine;
using Unity.MLAgents.Policies;

public class EnvTraining : MonoBehaviour
{
    [SerializeField] private List<Circuit> m_circuits;
    [SerializeField] private AgentML m_agent;
    [SerializeField] private RoadObstacle m_roadObstacle;
    [SerializeField] private int m_startIndex;


    [Header("Succes Rating ")]
    [SerializeField] private int m_maxSuccesCount =10;
    private int m_succesCount;
    [SerializeField] private float m_successRate;

    [Header("Odds Circuit")]
    [Range(0.0f, 100.0f)]
    [SerializeField] private float m_oddMainCircuit = 70;

    private int m_maxCircuitID = 0;
    private bool m_allCircuitsFinished = false;
    private Circuit m_currentCircuit = null;


    public void Awake()
    {
        foreach (Circuit circuit in m_circuits)
        {
            circuit.gameObject.SetActive(false);
        }

        m_maxCircuitID = m_startIndex;
    }

    private void OnDisable()
    {
        ChangeCircuit(null);
        m_roadObstacle.DeleteObstacle();
    }

    public void ResetEnvTraining()
    {
        m_roadObstacle.DeleteObstacle();

        float rate = UnityEngine.Random.Range(0, 100);

        int circuitID = -1;

        if(!m_allCircuitsFinished)
        {
            if (rate < m_oddMainCircuit)
            {
                circuitID = m_maxCircuitID;
            }
            else
            {
                circuitID = UnityEngine.Random.Range(0, m_maxCircuitID);
            }
        }
        else
        {
            circuitID = UnityEngine.Random.Range(0, m_maxCircuitID + 1);
        }

        ChangeCircuit(m_circuits[circuitID]);

        m_currentCircuit.ResetCircuit();
        StartCoroutine(AddObstacle());
        m_currentCircuit.SpawnAgent(m_agent.transform);
        EvaluationTests.SetMaxCheckpoint(m_currentCircuit.GetCheckpointCount());
        EvaluationTests.GetEventOnTrialStart()?.AddListener(SetupAgentModel);
        EvaluationTests.LaunchTrial();
    }

    private void ChangeCircuit(Circuit newCircuit)
    {
        if (m_currentCircuit == newCircuit) return;

        if (m_currentCircuit != null)
        {
            m_currentCircuit.onCircuitFinished -= OnCircuitFinished;
            m_currentCircuit.gameObject.SetActive(false);
        }

        m_currentCircuit = newCircuit;

        if (m_currentCircuit != null)
        {
            m_currentCircuit.gameObject.SetActive(true);
            m_currentCircuit.onCircuitFinished += OnCircuitFinished;
        }
    }

    public void OnCircuitFinished(AgentML agent, Circuit circuit)
    {
        agent.CrossLastCheckpoint();

        OnEpisodeFinish(true);

        agent.AddReward(agent.LastCheckpointReward);
        agent.OnEndEpisode?.Invoke(true);
        EvaluationTests.FinishTrial();
        agent.EndEpisode();
    }

    public void OnEpisodeFinish(bool isSuccess)
    {
        if (m_circuits.IndexOf(m_currentCircuit) != m_maxCircuitID)
            return;

        m_succesCount += isSuccess ? 1 : -1;

        m_succesCount = Mathf.Clamp(m_succesCount, 0, m_maxSuccesCount);

        float rate = (float)m_succesCount / (float)m_maxSuccesCount;
        if(rate >= m_successRate/100.0f)
        {

            m_succesCount = 0;
            m_maxCircuitID = m_circuits.IndexOf(m_currentCircuit) + 1;
            if(m_maxCircuitID >= m_circuits.Count)
            {
                m_allCircuitsFinished = true;
                m_maxCircuitID = m_circuits.Count - 1;
            }
        }
    }

    public Checkpoint[] GetCheckpointList()
    {
        if (m_currentCircuit == null) return new Checkpoint[0];

        List<Road> roadsList = m_currentCircuit.GetRoads();
        List<Checkpoint> checkpointList = new List<Checkpoint>();

        for (int i = 0; i < roadsList.Count; i++)
        {
            checkpointList.AddRange(roadsList[i].GetCheckpoints());
        }

        return checkpointList.ToArray();

    }

    public List<SplineComputer> GetSplineList()
    {
        if(m_currentCircuit == null) return new List<SplineComputer>();

        List<Road> roadsList = m_currentCircuit.GetRoads();
        List<SplineComputer> splinesList = new List<SplineComputer>();
        for (int i = 0; i < roadsList.Count; i++)
        {
            SplineComputer splineComputer = roadsList[i].GetComponentInChildren<SplineComputer>();
            splinesList.Add(splineComputer);
        }

        return splinesList;

    }

    IEnumerator AddObstacle()
    {
        yield return null;
        m_roadObstacle.circuit = m_currentCircuit;
        m_roadObstacle.SetObstacle();
    }

    public List<Circuit> GetCircuits()
    {
        return m_circuits;
    }

    public Circuit GetActiveCircuit()
    {
        return m_currentCircuit;
    }

    private void SetupAgentModel(ModelAsset model)
    {
        m_agent.GetComponent<BehaviorParameters>().Model = model;
    }
}
