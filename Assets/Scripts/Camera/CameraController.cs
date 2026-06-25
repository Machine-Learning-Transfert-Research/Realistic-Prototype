using UnityEngine;
using UnityEngine.InputSystem;


public class CameraController : MonoBehaviour
{
    public Transform[] indexCamPosition;
    public bool hasTransition;

    private float m_camTime = 0;
    private int m_currentIndex;
    private int m_prevIndex;
    private Vector3 m_startPosition;
    private Vector3 m_targetPosition;
    private Quaternion m_startRotation;
    private Quaternion m_targetRotation;

    public void OnNormalView(InputAction.CallbackContext ctx)
    {

        if (ctx.performed)
            SetupCamera(0);
    }

    public void OnTopView(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            SetupCamera(1);
    }

    public void OnFrontView(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            SetupCamera(2);
    }


    public void OnSideView(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            SetupCamera(3);
    }


    public void SetupCamera(int index)
    {
        m_prevIndex = m_currentIndex;
        m_currentIndex = index;

        if (!hasTransition)
        {
            transform.position = indexCamPosition[m_currentIndex].position;
            transform.rotation = indexCamPosition[m_currentIndex].rotation;
        }
        else
        {
            m_startPosition = indexCamPosition[m_prevIndex].position;
            m_startRotation = indexCamPosition[m_prevIndex].rotation;

            m_targetPosition = indexCamPosition[m_currentIndex].position;
            m_targetRotation = indexCamPosition[m_currentIndex].rotation;
            m_camTime = 0;

        }

    }

    public void Update()
    {
        if (!hasTransition || m_camTime == 1.0f) return;

        m_startPosition = indexCamPosition[m_prevIndex].position;
        m_startRotation = indexCamPosition[m_prevIndex].rotation;

        m_targetPosition = indexCamPosition[m_currentIndex].position;
        m_targetRotation = indexCamPosition[m_currentIndex].rotation;

        m_camTime += Time.deltaTime;
        m_camTime = Mathf.Clamp(m_camTime, 0.0f, 1.0f);

        transform.position = Vector3.Lerp(m_startPosition, m_targetPosition, m_camTime);
        transform.rotation = Quaternion.Slerp(m_startRotation, m_targetRotation, m_camTime);
    }


}
