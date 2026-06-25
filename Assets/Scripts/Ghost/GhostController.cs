using UnityEngine;

public class GhostController : MonoBehaviour
{
    public GhostRecorder recorder;
    public GhostPlayer player;
    public bool showGhost;
    public bool recordGhost;

    [Header("Display Settings")]
    public int ghostsToShow = 10;

    private void Start()
    {
        if (recorder != null && showGhost)
        {
            recorder.isShowcase = true;
            player.SetNumberOfGhosts(ghostsToShow);
            player.StartGhostPlayback();
        }

    }

    private void OnDestroy()
    {
        recorder.StopRecording();
    }

}