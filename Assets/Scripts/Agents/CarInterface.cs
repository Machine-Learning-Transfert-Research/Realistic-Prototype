using Unity.Vehicles;
using UnityEngine;
using VehiclePhysics;

public class CarInterface : MonoBehaviour
{
    [Header("Components ")]
    public VehicleBase vehicle;

    [Header("Debug Value")]
    [SerializeField] private bool m_isDebugActive;
    [SerializeField] private bool m_deactiveInput;

    private float m_throttleValue;
    private float m_brakeValue;
    private float m_steeringValue;
    private bool m_isHandBrakeActive;
    private float m_handbrakeValue;

    private const float m_maxInputValue = 10000;

    #region Input Value Function
    /// <summary>
    /// Set the throttle value 
    /// </summary>
    /// <param name="value">Value is limit between 0.0 and 1.0</param>
    public void SetThrottleValue(float value)
    {
        value = Mathf.Clamp(value, 0.0f, 1.0f);
        
        m_throttleValue = m_maxInputValue * value;
    }

    /// <summary>
    /// Set the brake value 
    /// </summary>
    /// <param name="value">Value is limit between 0.0 and 1.0</param>
    public void SetBrakeValue(float value)
    {
        value *= -1;
        value = Mathf.Clamp(value, 0.0f, 1.0f);

        m_brakeValue = m_maxInputValue * value;
    }

    /// <summary>
    /// Set the steering input value
    /// </summary>
    /// <param name="value">The value is between -1.0 and 1.0</param>
    public void SetSteeringValue(float value)
    {
        value = Mathf.Clamp(value, -1.0f, 1.0f);

        m_steeringValue = m_maxInputValue * value;

    }

    /// <summary>
    /// Active the handbrake input
    /// </summary>
    /// <param name="state"></param>
    public void ActiveHandbrake(bool state)
    {
        float value = state ? 1.0f : 0.0f;
        m_isHandBrakeActive = state;
        m_handbrakeValue = m_maxInputValue * value;
    }

    public float GetBoostAmount()
    {
        return 0;
    }

    #endregion

    #region Unity Functions
    public void Start()
    {
        vehicle = GetComponent<VehicleBase>();
    }

    public void FixedUpdate()
    {
        if (m_deactiveInput)
            return;
        vehicle.data.Set(Channel.Input, InputData.Throttle, (int)m_throttleValue);
        vehicle.data.Set(Channel.Input, InputData.Brake, (int)m_brakeValue);
        vehicle.data.Set(Channel.Input, InputData.Steer, (int)m_steeringValue);
        vehicle.data.Set(Channel.Input, InputData.Handbrake, (int)m_handbrakeValue);
    }

    #endregion

    #region Debug Functions

    public void OnGUI()
    {
        if (!m_isDebugActive)
            return;

        GUI.Label(new Rect(25, 25, 150, 25), "Throttle input :" + m_throttleValue);
        GUI.Label(new Rect(25, 50, 150, 25), "Brake input :" + m_brakeValue);
        GUI.Label(new Rect(25, 75, 150, 25), "Steering input :" + m_steeringValue);
        GUI.Label(new Rect(25, 100, 150, 25), "HandBrake input :" + m_handbrakeValue);
        GUI.Label(new Rect(25, 125, 150, 25), "Speed :" + vehicle.speed);
    }

    #endregion
}
