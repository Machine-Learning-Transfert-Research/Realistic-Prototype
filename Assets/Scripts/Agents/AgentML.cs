using Dreamteck.Splines;
using Evaluation;
using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Unity.Vehicles;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using VehiclePhysics;
using static VehiclePhysics.VehicleBase;

public class AgentML : Agent
{
    private CarInterface m_carInterface;
    private InputCarPlayer m_carPlayer;
    private Rigidbody m_rb;
    private Vector3 m_startPosition;
    private Quaternion m_startRotation;

    [SerializeField] private EnvTraining m_envTraining;

    [Header("Checkpoints")]
    public List<Checkpoint> checkpoints;
    private int m_maxCheckpointCount;
    private Checkpoint m_currentCheckpoint;

    [Header("Active Roads")]
    private List<Road> m_roads;

    [Header("Track")]
    [SerializeField] private List<SplineComputer> m_splines;
    [SerializeField] private float m_maxDistanceFromSpline = 15f;
    private float m_previousProgress = 0f;
    private float m_baseProgressDistance = 0f;
    [SerializeField] private int m_groundType;

    [Header("Rewards")]
    [SerializeField] private float m_checkpointReward = 1.0f;
    [SerializeField] private float m_lastCheckpointReward = 1.0f;
    [SerializeField] private float m_maxProgressReward = 1.0f;
    [SerializeField] private float m_splineDistancePenaltyMultiplier = 0.01f;
    [SerializeField] private bool m_enableIdlePenalty = false;
    [SerializeField] private float m_idlePenality = 2.5f;
    [SerializeField] private float m_idlePenalityMinThreshold = 0.5f;
    [SerializeField] private float m_crashPenalty = -1.0f;
    [SerializeField] private bool m_enableSpeedReward = false;
    [SerializeField] private bool m_enableFrictionOffRoad = true;
    [SerializeField] private bool m_enableTimePenalty = true;
    [SerializeField] private float m_maxTimePenalty = 1.0f;


    private const float m_checkpointRewardValue = 1.0f;

    public float LastCheckpointReward => m_lastCheckpointReward;

    public System.Action<bool> OnEndEpisode;
    [Header("Normale parameter")]
    [SerializeField] private LayerMask m_layerMask;
    private Vector3 normalGround;
    private int m_groundId;
    [System.Serializable]
    public class CollisionReward
    {
        public string objectTag = "Obstacle";
        public bool giveReward = true;
        public float rewardAmount = -1f;
        public bool endEpisode = true;
    }

    private List<GameObject> m_uniqueHitObstacle = new List<GameObject>();
    private List<GameObject> m_hitObjects = new List<GameObject>();
    [Header("Collision Rules")]
    [SerializeField] private List<CollisionReward> m_collisionRules = new List<CollisionReward>();
    private const float m_maxSpeed = 35.0f;

    [Header("Wheel Control")]
    public VPVehicleController vehicle;
    private WheelState[] m_tireFrictions;
    [SerializeField] private float m_roadGrip = 1f;
    [SerializeField] private float m_offroadGrip = 0.2f;
    [SerializeField] private float m_roadDrag = 0f;
    [SerializeField] private float m_offroadDrag = 1f;


    [Header("Observation")]
    private const float m_maxDistanceOberservationMesure = 500.0f;
    [SerializeField] private float m_distanceFirstSplinePoint = 30.0f;
    [SerializeField] private float m_distanceSecondSplinePoint = 60.0f;

    private const float m_constSizeRoad = 15.0f;


    // --- Input Var ---
    private const int m_boostInputValue = 0;



    #region Unity Function
    public void Start()
    {
        var wheelData = vehicle.wheelState;
        m_carInterface = GetComponent<CarInterface>();
        m_carPlayer = GetComponent<InputCarPlayer>();
        m_rb = GetComponent<Rigidbody>();
        m_startPosition = transform.position;
        m_startRotation = transform.rotation;
        foreach (WheelState tire in vehicle.wheelState)
        {
            tire.groundMaterial = new GroundMaterial();
        }

        EvaluationTests.SetAgentTrainingStepValue(CompletedEpisodes);
        EvaluationTests.GetEventTimeTrialEnd()?.AddListener(ResetAgent);
    }

    public void FixedUpdate()
    {
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f, m_layerMask))
        {
            normalGround = hit.normal;
            if (hit.collider.tag == "Road")
            {
                EvaluationTests.SetAgentOffRoad(false);
                m_groundId = 1;
                if (vehicle && m_enableFrictionOffRoad)
                    SetRoadFriction();
            }
            else
            {
                EvaluationTests.SetAgentOffRoad(true);
                m_groundId = 2;
                if (vehicle && m_enableFrictionOffRoad)
                    SetOffroadFriction();
            }
        }
        else
        {
            normalGround = transform.up;
            m_groundId = 0;
        }
    }
    #endregion

    #region MLAgent Functions
    public override void OnEpisodeBegin()
    {
        // Reset input car
        m_carInterface.SetThrottleValue(0.0f);
        m_carInterface.SetBrakeValue(0.0f);
        m_carInterface.SetSteeringValue(0.0f);

        // Set max speed evaluation data
        EvaluationTests.SetAgentMaxSpeed(m_maxSpeed);

        m_uniqueHitObstacle.Clear();

        m_envTraining.ResetEnvTraining();

        checkpoints = new List<Checkpoint>(m_envTraining.GetCheckpointList());
        m_maxCheckpointCount = checkpoints.Count;

        m_splines = m_envTraining.GetSplineList();

        m_currentCheckpoint = checkpoints[0];

        m_hitObjects.Clear();

        if (m_currentCheckpoint != null)
        {
            m_previousProgress = Vector3.Distance(transform.position, m_currentCheckpoint.transform.position);
            m_baseProgressDistance = m_previousProgress;
        }
    }



    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.InverseTransformDirection(m_rb.linearVelocity / m_maxSpeed));

        Vector3 targetDirection = m_currentCheckpoint != null ? m_currentCheckpoint.transform.position - transform.position : Vector3.zero;
        sensor.AddObservation(transform.InverseTransformDirection(targetDirection) / m_maxDistanceOberservationMesure);

        sensor.AddObservation(m_groundId);
        sensor.AddObservation(m_carInterface.GetBoostAmount());

        Vector3 toSpline0 = transform.InverseTransformDirection((GetClosetSplinePoint() - transform.position) / m_maxDistanceOberservationMesure);
        sensor.AddObservation(toSpline0);

        Vector3 toSpline1 = transform.InverseTransformDirection((GetSplinePointInFront(m_distanceFirstSplinePoint) - transform.position) / m_maxDistanceOberservationMesure);
        sensor.AddObservation(toSpline1);

        Vector3 toSpline2 = transform.InverseTransformDirection((GetSplinePointInFront(m_distanceSecondSplinePoint) - transform.position) / m_maxDistanceOberservationMesure);
        sensor.AddObservation(toSpline2);

        sensor.AddObservation(normalGround.normalized);
    }

    // Action Buffer
    // - Continous Action 0: Steering
    // - Continuous Action 1 : Throttle / Brake
    // - Discrete Action 0 : Handbrake
    // - Discrete Action 1 : Boost
    public override void OnActionReceived(ActionBuffers actions)
    {
        float throttleValue = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        if (throttleValue >= 0)
        {
            m_carInterface.SetThrottleValue(throttleValue);
            m_carInterface.SetBrakeValue(0.0f);

        }
        else
        {
            m_carInterface.SetBrakeValue(throttleValue);
            m_carInterface.SetThrottleValue(0.0f);
        }

        float steeringValue = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        m_carInterface.SetSteeringValue(steeringValue);

        bool handBrake = actions.DiscreteActions[0] == 1;

        HandleProgressReward();
        HandleSplinePenalty();

        if (m_enableIdlePenalty)
            HandleIdlePenalty();
        if (m_enableSpeedReward)
            HandleSpeedReward();
        if (m_enableTimePenalty)
            HandleTimePenality();

        foreach (var hit in m_hitObjects)
        {
            HandleCollision(hit);
        }

        EvaluationTests.SetAgentSpeed(m_rb.linearVelocity.magnitude);

    }



    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continousAction = actionsOut.ContinuousActions;
        continousAction[1] = m_carPlayer.GetThrottleValue();
        continousAction[0] = m_carPlayer.GetSteeringValue();

        var discreteAction = actionsOut.DiscreteActions;
        discreteAction[0] = m_carPlayer.GetHandbrakeValue() ? 1 : 0;
        discreteAction[1] = m_boostInputValue;
    }
    #endregion

    #region Collisions Functions
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle" && !m_hitObjects.Contains(collision.gameObject))
        {
            m_hitObjects.Add(collision.gameObject);
            if (!m_uniqueHitObstacle.Contains(collision.gameObject))
            {
                m_uniqueHitObstacle.Add(collision.gameObject);
                EvaluationTests.AddHitObjstacle();
                EvaluationTests.SetAgentIsColliding(true);
            }

        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (m_hitObjects.Contains(collision.gameObject))
        {
            EvaluationTests.SetAgentIsColliding(false);
            m_hitObjects.Remove(collision.gameObject);
        }
    }

    //Ground ID
    // 0 is on road
    // 1 is off road
    // 2 is not touching ground

    private void OnTriggerEnter(Collider other)
    {
        if (checkpoints == null)
            return;

        if (m_currentCheckpoint != null && other.transform == m_currentCheckpoint.transform)
        {
            if (m_groundId < 2)
            {
                AddReward((m_previousProgress / m_baseProgressDistance) / m_maxCheckpointCount * m_maxProgressReward);
                AddReward(m_checkpointRewardValue / m_maxCheckpointCount);
            }
            else
            {
                AddReward((m_checkpointRewardValue / 2.0f) / m_maxCheckpointCount);
            }


            if (!m_currentCheckpoint.m_isEnd)
            {
                EvaluationTests.AddAgentCheckpoint();
            }

            checkpoints.Remove(m_currentCheckpoint);
            m_currentCheckpoint = checkpoints[0];

            if (m_currentCheckpoint != null)
            {
                m_previousProgress = Vector3.Distance(transform.position, m_currentCheckpoint.transform.position);
                m_baseProgressDistance = m_previousProgress;
            }
        }
    }

    public void CrossLastCheckpoint()
    {
        if (m_currentCheckpoint != null)
        {
            if (m_groundId < 2)
            {
                AddReward((m_previousProgress / m_baseProgressDistance) / m_maxCheckpointCount * m_maxProgressReward);
            }

            AddReward(1.0f / m_maxCheckpointCount);
            checkpoints.Remove(m_currentCheckpoint);
            m_currentCheckpoint = null;
        }
    }
    #endregion

    #region GetInfos Functions

    private SplineComputer GetClosestSpline()
    {
        if (m_splines.Count == 0) return null;

        SplineComputer closestSpline = m_splines[0];
        float closestDistance = Vector3.Distance(closestSpline.Project(transform.position).position, this.transform.position);
        foreach (SplineComputer spline in m_splines)
        {
            float distanceTest = Vector3.Distance(spline.Project(transform.position).position, this.transform.position);

            if (closestDistance > distanceTest)
            {
                closestSpline = spline;
                closestDistance = distanceTest;
            }
        }
        return closestSpline;

    }

    private float GetDistanceToSpline()
    {
        if (GetClosestSpline())
        {
            SplineSample splineSample = GetClosestSpline().Project(transform.position);
            float distance = Vector3.Distance(
                transform.position,
                splineSample.position
            );
            return distance;
        }
        return 0f;
    }
    private Vector3 GetClosetSplinePoint()
    {
        SplineComputer ClosestSpline = GetClosestSpline();
        if (ClosestSpline)
        {
            return ClosestSpline.Project(transform.position).position;
        }
        return Vector3.zero;
    }

    private Vector3 GetSplinePointInFront(float distance)
    {
        SplineComputer Spline = GetClosestSpline();
        if (Spline)
        {
            int splineIndex = m_splines.IndexOf(Spline);
            SplineSample sampleClosest = Spline.Project(transform.position);
            double percentClosest = sampleClosest.percent;
            double currentDistance = Spline.CalculateLength() * percentClosest;
            double targetDistance = currentDistance + distance;

            while (splineIndex < m_splines.Count - 1 && targetDistance > Spline.CalculateLength())
            {
                targetDistance -= Spline.CalculateLength();
                splineIndex++;
                Spline = m_splines[splineIndex];
            }

            float newPercent = Mathf.Clamp01((float)targetDistance / Spline.CalculateLength());
            SplineSample inFront = Spline.Evaluate(newPercent);

            return inFront.position;
        }

        return Vector3.zero;
    }

    #endregion

    #region Handling Functions

    private void HandleProgressReward()
    {
        if (m_currentCheckpoint == null) return;

        float progress = Vector3.Distance(transform.position, m_currentCheckpoint.transform.position);
        if (progress < m_previousProgress)
        {
            float progressReward = (m_previousProgress - progress) / m_baseProgressDistance;
            AddReward(progressReward / m_maxCheckpointCount * m_maxProgressReward);
            m_previousProgress = progress;
        }
    }

    //Penalty when you go far from the spline - to test might be too strict
    private void HandleSplinePenalty()
    {
        float distance = GetDistanceToSpline();
  

        float ratioValue = 0;
        if (distance < m_constSizeRoad)
        {
            return;
        }
        else
        {
            ratioValue = -1.0f * (1.0f + ((distance - m_constSizeRoad) / (m_maxDistanceFromSpline - m_constSizeRoad)));
        }

        if (MaxStep > 0)
            AddReward((m_splineDistancePenaltyMultiplier * ratioValue) / (float)MaxStep);
        if (distance > m_maxDistanceFromSpline)
        {
            AddReward(-2f);
            EvaluationTests.FinishTrial(true);
            OnEndEpisode?.Invoke(false);
            EndEpisode();
        }
    }

    //To prevent car from not moving
    private void HandleIdlePenalty()
    {
        if (m_rb.linearVelocity.magnitude < m_idlePenalityMinThreshold)
            AddReward(-m_idlePenality / (float)MaxStep);
    }

    private void HandleCollision(GameObject hitObject)
    {
        if (hitObject && hitObject.activeInHierarchy)
        {
            foreach (var rule in m_collisionRules)
            {
                if (hitObject.CompareTag(rule.objectTag))
                {
                    if (rule.giveReward)
                    {
                        AddReward(rule.rewardAmount / (float)MaxStep);
                    }

                    if (rule.endEpisode)
                    {
                        OnEndEpisode?.Invoke(false);
                        EvaluationTests.FinishTrial(true);
                        EndEpisode();
                    }

                    return;
                }
            }
        }
    }
    private void HandleSpeedReward()
    {
        AddReward(Mathf.Clamp01(((m_rb.linearVelocity.magnitude / m_maxSpeed)) / (float)MaxStep));
    }

    private void HandleTimePenality()
    {
        AddReward(-m_maxTimePenalty / (float)MaxStep);
    }
    #endregion

    public void ResetAgent()
    {
        AddReward(m_crashPenalty);
        EvaluationTests.FinishTrial(true);
        OnEndEpisode?.Invoke(false);
        EndEpisode();
    }
    void SetRoadFriction()
    {
        foreach (WheelState tire in vehicle.wheelState)
        {
            tire.groundMaterial.grip = m_roadGrip;
            tire.groundMaterial.drag = m_roadDrag;
        }
    }
    void SetOffroadFriction()
    {
        foreach (WheelState tire in vehicle.wheelState)
        {
            tire.groundMaterial.grip = m_offroadGrip;
            tire.groundMaterial.drag = m_offroadDrag;
        }
    }
}
