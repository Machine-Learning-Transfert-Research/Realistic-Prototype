using System.Data;
using UnityEngine;

public class RollCrashComponent : MonoBehaviour
{
    public AgentML agent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Road" && other.tag != "OffRoad" && other.tag != "Obstacle")
            return;

        if (agent)
            agent.ResetAgent();
        else
            Debug.Log("Agent not found in RollCrashComponent");
    }
    
}
