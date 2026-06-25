using UnityEngine;

public class RespawnSystem : MonoBehaviour
{
    private Vector3 m_lastPosition;
    private Quaternion m_lastRotation;
    private Rigidbody m_rb;

    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_lastPosition = transform.position;
        m_lastRotation = transform.rotation;
    }

    public void SetCheckpoint(Vector3 pos, Quaternion rot)
    {
        m_lastPosition = pos;
        m_lastRotation = rot;
    }

    public void Respawn()
    {
        m_rb.linearVelocity = Vector3.zero;
        m_rb.angularVelocity = Vector3.zero;

        transform.position = m_lastPosition;
        transform.rotation = m_lastRotation;
    }
}
