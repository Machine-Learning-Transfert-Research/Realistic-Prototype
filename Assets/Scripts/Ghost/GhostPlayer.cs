using System.Collections.Generic;
using UnityEngine;

public class GhostPlayer : MonoBehaviour
{
    [Header("Ghost Settings")]
    public GhostRecorder recorder;
    public GameObject ghostCarPrefab;
    public Material ghostMaterial;

    [Header("Playback Settings")]
    public int numberOfGhostsToShow = 10; 
    public bool showAllGhosts = false; 

    private List<GhostInstance> m_activeGhosts = new List<GhostInstance>();
    private float m_playbackStartTime;
    private bool m_isPlaying = false;

    private class GhostInstance
    {
        public GameObject gameObject;
        public GhostRecording recording;
        public int currentFrameIndex;
        public Material material;
    }

    public void StartGhostPlayback()
    {
        StopGhostPlayback();

        if (recorder == null)
        {
            return;
        }

        GhostDatabase database = recorder.GetDatabase();

        if (database == null || database.recordings.Count == 0)
        {
            return;
        }

        int ghostCount = showAllGhosts ? database.recordings.Count : Mathf.Min(numberOfGhostsToShow, database.recordings.Count);
        List<GhostRecording> recordingsToPlay = recorder.GetLastNRuns(ghostCount);
        for (int i = 0; i < recordingsToPlay.Count; i++)
        {
            CreateGhostInstance(recordingsToPlay[i], i, recordingsToPlay.Count);
        }

        m_playbackStartTime = Time.time;
        m_isPlaying = true;
    }

    private void CreateGhostInstance(GhostRecording recording, int index, int totalGhosts)
    {
        if (recording.frames.Count == 0)
        {
            return;
        }

        GameObject ghostObj = Instantiate(ghostCarPrefab);
        Material instanceMaterial = new Material(ghostMaterial);
        instanceMaterial.color = recording.colorRun;

        foreach (Renderer rend in ghostObj.GetComponentsInChildren<Renderer>())
        {
            for (int i = 0; i < rend.materials.Length; i++)
            {
                rend.sharedMaterials[i].color = recording.colorRun;
            }
        }

        GhostInstance instance = new GhostInstance
        {
            gameObject = ghostObj,
            recording = recording,
            currentFrameIndex = 0,
            material = instanceMaterial
        };

        m_activeGhosts.Add(instance);
    }

    public void StopGhostPlayback()
    {
        m_isPlaying = false;

        foreach (GhostInstance ghost in m_activeGhosts)
        {
            if (ghost.gameObject != null)
            {
                Destroy(ghost.gameObject);
            }
        }

        m_activeGhosts.Clear();
    }

    void Update()
    {
        if (!m_isPlaying || m_activeGhosts.Count == 0)
            return;

        float currentTime = Time.time - m_playbackStartTime;
        bool anyGhostActive = false;

        foreach (GhostInstance ghost in m_activeGhosts)
        {
            if (UpdateGhost(ghost, currentTime))
            {
                anyGhostActive = true;
            }
        }

        if (!anyGhostActive)
        {
            Debug.Log("All ghosts finished playback");
            StopGhostPlayback();
        }
    }

    private bool UpdateGhost(GhostInstance ghost, float currentTime)
    {
        while (ghost.currentFrameIndex < ghost.recording.frames.Count - 1 &&
               ghost.recording.frames[ghost.currentFrameIndex + 1].timestamp <= currentTime)
        {
            ghost.currentFrameIndex++;
        }

        if (ghost.currentFrameIndex >= ghost.recording.frames.Count - 1)
        {
            ghost.gameObject.SetActive(false);
            return false;
        }

        GhostFrame currentFrame = ghost.recording.frames[ghost.currentFrameIndex];
        GhostFrame nextFrame = ghost.recording.frames[ghost.currentFrameIndex + 1];

        float frameDuration = nextFrame.timestamp - currentFrame.timestamp;
        float frameProgress = frameDuration > 0 ? (currentTime - currentFrame.timestamp) / frameDuration : 0;

        ghost.gameObject.transform.position = Vector3.Lerp(currentFrame.position, nextFrame.position, frameProgress);
        ghost.gameObject.transform.rotation = Quaternion.Slerp(currentFrame.rotation, nextFrame.rotation, frameProgress);

        return true;
    }

    public void SetNumberOfGhosts(int count)
    {
        numberOfGhostsToShow = count;
    }

    public void ToggleShowAllGhosts(bool showAll)
    {
        showAllGhosts = showAll;
    }
}