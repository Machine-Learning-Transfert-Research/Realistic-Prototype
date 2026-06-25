using Evaluation;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Vector3 m_spawnPosition = Vector3.zero;    
    [SerializeField] private bool m_isValid;
    [SerializeField] public bool m_isEnd;

    public System.Action isEndTrack;

    #region Unity Functions
    public void OnTriggerEnter(Collider other)
    {
        AgentML agent = other.GetComponentInParent<AgentML>();
        if (agent == null || other.gameObject.tag != "Player") return;

        if (!m_isValid)
        {
            m_isValid = true;
            if (!m_isEnd)
            {
                RespawnSystem respawn = other.GetComponentInParent<RespawnSystem>();
                respawn.SetCheckpoint(transform.position, transform.rotation);
            }
            else
            {
                EvaluationTests.AddAgentCheckpoint();
                isEndTrack?.Invoke();
            }
         
        }
    }

    #endregion
    
    public void SetEndCheckpoint()
    {
        m_isEnd = true;
    }

    public void OnReset()
    {
        m_isValid = false;
        m_isEnd = false;
    }

    #region  Debug Functions

    public void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.red;

        Gizmos.DrawSphere(transform.position + m_spawnPosition, 0.4f);
#endif
    }

    #endregion


}
