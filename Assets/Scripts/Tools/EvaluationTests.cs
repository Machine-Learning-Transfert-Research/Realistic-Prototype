using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using Unity.MLAgents;
using Unity.InferenceEngine;

namespace Evaluation
{
    public enum GameType
    {
        Realist = 0,
        Arcade = 1,
    }

    public class EvaluationTests : MonoBehaviour
    {
        private static EvaluationTests instance;

        [Header("Setup")]
        [SerializeField] private bool m_isEvaluationActive = false;
        [SerializeField] private bool m_isRecordActive = false;
        [SerializeField] private int m_numberOfTrial = 50;
        [SerializeField] private float m_speedTest = 2;
        private int m_currentTrialNumber = 0;
        [SerializeField] private ModelAsset[] m_modelTested;
        private int m_modelTestCount;


        [Tooltip("Time in second")]
        [SerializeField] private float m_timeTrialMax = 300;
        private float m_currentTrialDuration;

        [Header("Evaluation Info")]
        public GameType type = GameType.Realist;
        private int m_traininStep = 0;

        private int m_currentCheckpointValidate;
        private int m_maxCheckpoint;

        private float m_averageSpeed;
        private int m_entrySpeed = 0;
        private float m_cumulativeSpeed = 0;
        private float m_maxSpeed;

        private float m_offRoadTime;
        private bool m_isOffRoad;

        private int m_hitNumber;
        private int m_maxObstacle;
        private bool m_isCollidingWithObstacle;
        private float m_ObstacleTime;


        public UnityEvent<ModelAsset> OnStartTrial;
        public UnityEvent OnEndTrial;
        public UnityEvent OnTimeTrialEnd;

        private bool m_isInTrial = false;


        //1. Get Training Step

        #region Unity Function

        public void Awake()
        {
            if ((m_isEvaluationActive || m_isRecordActive) && instance == null)
                instance = this;
        }
        public void Start()
        {

  
            InitComponent();
        }

        public void Update()
        {
            UpdateTrial();
        }

        private void OnDestroy()
        {
            OnEndTrial.RemoveListener(EndTrial);
        }

        #endregion

        private void InitComponent()
        {
            ToolsCsv.CreateCategory(
                "Trial ID",
                "Game Type",
                "Name Model",
                "Training Step",
                "Race Completion",
                "Checkpoint Max",
                "Time",
                "Max Time",
                "Speed Average",
                "Max Speed",
                "Off Road Time",
                "Hit Number",
                "Max Obstacle",
                "Obstacle Time"
                );

            Time.timeScale = m_speedTest;
        }

        public void StartTrial()
        {
            if (m_modelTestCount >= m_modelTested.Length) return;

            m_isInTrial = true;
            m_currentTrialDuration = 0.0f;
            m_entrySpeed = 0;
            m_offRoadTime = 0;
            m_cumulativeSpeed = 0;
            m_currentCheckpointValidate = 0;
            m_ObstacleTime = 0;
            m_hitNumber = 0;
            m_isCollidingWithObstacle = false;
            OnStartTrial?.Invoke(m_modelTested[m_modelTestCount]);
        }

        private void UpdateTrial()
        {
            if (!m_isInTrial) return;

            if (m_isOffRoad)
            {
                m_offRoadTime += Time.deltaTime;
            }
            if (m_isCollidingWithObstacle)
            {
                m_ObstacleTime += Time.deltaTime;
            }
            m_currentTrialDuration += Time.deltaTime;
            if (m_currentTrialDuration >= m_timeTrialMax)
            {
                OnTimeTrialEnd?.Invoke();
            }
        }

        public void EndTrial()
        {

            WriteMetric();
            m_isInTrial = false;

            if (m_currentTrialNumber >= m_numberOfTrial)
            {

                m_modelTestCount++;
                if (m_modelTestCount >= m_modelTested.Length)
                {
                    FinishEvaluation();
                }
                else
                {
                    StartTrial();
                    m_currentTrialNumber = 0;
                }
            }
            else
            {
                m_currentTrialNumber++;
            }

            OnEndTrial?.Invoke();
        }

        private void WriteMetric()
        {

            m_averageSpeed = m_cumulativeSpeed / m_entrySpeed;


            ToolsCsv.WriteLine(
                m_currentTrialNumber.ToString(),
                type.ToString(),
                m_modelTested[m_modelTestCount].name,
                m_traininStep.ToString(),
                m_currentCheckpointValidate.ToString(),
                m_maxCheckpoint.ToString(),
                m_currentTrialDuration.ToString(),
                m_timeTrialMax.ToString(),
                m_averageSpeed.ToString(),
                m_maxSpeed.ToString(),
                m_offRoadTime.ToString(),
                m_hitNumber.ToString(),
                m_maxObstacle.ToString(),
                m_ObstacleTime.ToString()
                );
        }

        public void FinishEvaluation()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#endif
        }


        #region Static Functions


        public static void SetAgentTrainingStepValue(int trainingStep)
        {
            if (instance == null) return;

            instance.m_traininStep = trainingStep;
        }

        public static UnityEvent<ModelAsset> GetEventOnTrialStart()
        {
            if (instance == null) return null;

            return instance.OnStartTrial;
        }

        public static UnityEvent GetEventOnTrialEnd()
        {
            if (instance == null) return null;

            return instance.OnEndTrial;
        }

        public static UnityEvent GetEventTimeTrialEnd()
        {
            if (instance == null) return null;

            return instance.OnTimeTrialEnd;
        }

        public static void LaunchTrial()
        {
            if (instance == null) return;

            instance.StartTrial();
        }

        public static void FinishTrial(bool isCrash = false)
        {
            if (instance == null) return;

            if (isCrash)
            {
                instance.m_currentTrialDuration = instance.m_timeTrialMax;
            }
            instance.EndTrial();
        }

        public static void SetAgentMaxSpeed(float maxSpeed)
        {
            if (instance == null) return;

            instance.m_maxSpeed = maxSpeed;
        }

        public static void SetMaxObstacle(int maxObstacle)
        {
            if (instance == null) return;

            instance.m_maxObstacle = maxObstacle;
        }

        public static void SetMaxCheckpoint(int maxCheckpoint)
        {
            if (instance == null) return;

            instance.m_maxCheckpoint = maxCheckpoint;
        }

        public static void SetAgentSpeed(float speed)
        {
            if (instance == null) return;

            instance.m_cumulativeSpeed += speed;
            instance.m_entrySpeed++;
        }

        public static void AddHitObjstacle()
        {
            if (instance == null) return;
            instance.m_hitNumber++;
        }

        public static void SetAgentOffRoad(bool isOffRoad)
        {
            if (instance == null) return;

            instance.m_isOffRoad = isOffRoad;
        }

        public static void SetAgentIsColliding(bool isColliding)
        {
            if (instance == null) return;

            instance.m_isCollidingWithObstacle = isColliding;
        }

        public static void AddAgentCheckpoint()
        {
            if (instance == null) return;

            instance.m_currentCheckpointValidate++;
        }
        
        public static void IncreaseModel()
        {
            if (instance == null) return;

            instance.m_modelTestCount++;
            if (instance.m_modelTestCount >= instance.m_modelTested.Length)
            {
                instance.FinishEvaluation();
            }
        }

        #endregion
    }
}