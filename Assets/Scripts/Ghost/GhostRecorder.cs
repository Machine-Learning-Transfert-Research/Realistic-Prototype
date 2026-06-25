using Evaluation;
using System.Collections.Generic;
using System.IO;
using Unity.InferenceEngine;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class GhostFrame
{
    public Vector3 position;
    public Quaternion rotation;
    public float timestamp;
}

[System.Serializable]
public class GhostRecording
{
    public List<GhostFrame> frames = new List<GhostFrame>();
    public int runNumber;
    public float totalTime;
    public Color colorRun;
}

[System.Serializable]
public class GhostDatabase
{
    public List<GhostRecording> recordings = new List<GhostRecording>();
    public int maxRecordings = 1500;
}

public class GhostRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    public bool isRecording = false;

    public string savePath = "Assets/Data/Ghost_Record_Data/";
    public string databaseFileName = "ghost_database_1.json";
    public int maxStoredRuns = 1500;
    public bool isMultipleNeuralNework = false;
    public int numberOfNetworkMax = 1;

    [Header("Color Settings")]
    public Color colorRunGhost = Color.white;
    [SerializeField] private bool m_hasMultipleColor;
    [SerializeField] private Color m_colorStartGhost;
    [SerializeField] private Color m_colorEndGhost;


    [HideInInspector] public bool isShowcase = false;
    private GhostRecording m_currentRecording;
    private GhostDatabase m_database;
    private float m_recordingStartTime;
    private int m_currentRunNumber = 0;
    private int m_currentNeuralGhostNetwork = 0;

    void Start()
    {
        LoadDatabase();
        EvaluationTests.GetEventOnTrialStart()?.AddListener(StartRecording);
        EvaluationTests.GetEventOnTrialEnd()?.AddListener(StopRecording);
    }

    void Update()
    {
        if (!isShowcase || isRecording)
        {
            RecordFrame();
        }
    }

    public void StartRecording(ModelAsset asset = null)
    {
        if (isShowcase) return;

        isRecording = true;
        m_currentRecording = new GhostRecording();
        m_currentRecording.runNumber = m_currentRunNumber;
        if (!m_hasMultipleColor)
        {
            m_currentRecording.colorRun = colorRunGhost;
        }
        else
        {
            m_currentRecording.colorRun = Color.Lerp(m_colorStartGhost, m_colorEndGhost, (float)m_currentNeuralGhostNetwork / (float)numberOfNetworkMax);
        }
        m_recordingStartTime = Time.time;
    }

    public void StopRecording()
    {

        if (!isRecording || isShowcase) return;

        isRecording = false;
        m_currentRecording.totalTime = Time.time - m_recordingStartTime;

        m_database.recordings.Add(m_currentRecording);

        m_currentRunNumber++;
        SaveDatabase();

        if (m_database.recordings.Count >= maxStoredRuns)
        {
            if (!isMultipleNeuralNework)
            {
#if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
#endif
            }
            else
            {
                if (m_currentNeuralGhostNetwork >= numberOfNetworkMax)
                {
#if UNITY_EDITOR
                    EditorApplication.ExitPlaymode();
#endif
                }
                else
                {
                    m_currentNeuralGhostNetwork++;
                }
            }
        }
    }

    private void RecordFrame()
    {
        GhostFrame frame = new GhostFrame
        {
            position = transform.position,
            rotation = transform.rotation,
            timestamp = Time.time - m_recordingStartTime
        };

        m_currentRecording.frames.Add(frame);
    }

    public void SaveDatabase()
    {
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        string json = JsonUtility.ToJson(m_database, true);
        string fullPath = Path.Combine(savePath, databaseFileName);
        File.WriteAllText(fullPath, json);
    }

    public void LoadDatabase()
    {
        string fullPath = Path.Combine(savePath, databaseFileName);

        if (File.Exists(fullPath))
        {
            string json = File.ReadAllText(fullPath);
            m_database = JsonUtility.FromJson<GhostDatabase>(json);

            if (m_database.recordings.Count > 0)
            {
                m_currentRunNumber = m_database.recordings[m_database.recordings.Count - 1].runNumber + 1;
            }
        }
        else
        {
            m_database = new GhostDatabase();
            m_database.maxRecordings = maxStoredRuns;
        }
    }

    public GhostDatabase GetDatabase()
    {
        LoadDatabase();
        return m_database;
    }

    public List<GhostRecording> GetLastNRuns(int n)
    {
        int startIndex = Mathf.Max(0, m_database.recordings.Count - n);
        int count = Mathf.Min(n, m_database.recordings.Count);
        return m_database.recordings.GetRange(startIndex, count);
    }

    public void ClearDatabase()
    {
        m_database.recordings.Clear();
        m_currentRunNumber = 0;
        SaveDatabase();
    }
}