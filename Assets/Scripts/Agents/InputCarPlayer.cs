using UnityEngine;
using UnityEngine.InputSystem;
public class InputCarPlayer : MonoBehaviour
{

    private float m_throttleValue;
    private float m_steeringValue;
    private bool m_handbrakeInput;
    [SerializeField] private bool m_isDebugActive;

    public void ThrottleInput(InputAction.CallbackContext ctx)
    {
        float value = ctx.ReadValue<float>();
        m_throttleValue = value;
       if(m_isDebugActive)
            Debug.Log("Throttle : " + value);
    }

    public void SteeringInput(InputAction.CallbackContext ctx)
    {
        float value = ctx.ReadValue<float>();
        m_steeringValue = value;
       if(m_isDebugActive)
            Debug.Log("Steering : " + value);
    }

    public void HandbrakeInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            m_handbrakeInput = true;
        }
        else
        {
            m_handbrakeInput = false;
        }
    }


    public float GetThrottleValue() { return m_throttleValue; }
    public float GetSteeringValue() { return m_steeringValue; }
    public bool GetHandbrakeValue() { return m_handbrakeInput; }
}
