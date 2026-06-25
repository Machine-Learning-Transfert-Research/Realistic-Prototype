using UnityEngine;
using UnityEngine.InputSystem;


public class CamController : MonoBehaviour
{
    [Header("Cam Movement Parameters")]
    public float baseSpeedMovemement =  0 ;
    public float maxSpeedMovement = 0;
    public AnimationCurve animationCurveSpeedMvt;
    public float durationMaxMovement = 0;
    private Vector2 m_movementInputValue = new Vector2(0, 0);
    private float m_timeMovementStarted;

    [Header("Cam Rotation Parameters")]
    public float minSpeedRotation = 0 ;
    public float maxSpeedRotation = 0;
    public AnimationCurve animationCurveRotationSpeed;
    public float durationMaxRotation = 0;
    private bool m_isRotationButtonPress = false;
    private Vector2 m_rotationCameraInputValue = new Vector2(0, 0);
    private float m_timeRotationStarted = 0;

    private Vector3 m_currentAngularRotation;

    #region Unity Function

    public void Awake()
    {
        m_currentAngularRotation = transform.rotation.eulerAngles;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Update()
    {
        UpdateCameraMovement();
        UpdateCameraRotation();
    }

    #endregion

    #region Input Functions

    public void OnMoveCam(InputAction.CallbackContext ctx)
    {
        m_movementInputValue = ctx.ReadValue<Vector2>();
        
    }

    public void OnRotationCam(InputAction.CallbackContext ctx)
    {
        m_rotationCameraInputValue = ctx.ReadValue<Vector2>();
    }

    public void OnPressRotation(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            m_isRotationButtonPress = true;
        }
        else
        {
            m_isRotationButtonPress = false;
        }
    }

    #endregion

    #region Update functions
    public void UpdateCameraMovement()
    {
        if (m_movementInputValue == Vector2.zero)
        {
            m_timeMovementStarted = 0;
            return;
        }
        float speed = Mathf.Lerp(baseSpeedMovemement, maxSpeedMovement,  animationCurveSpeedMvt.Evaluate(m_timeMovementStarted/ durationMaxMovement));
        Vector3 mvtX = transform.forward * m_movementInputValue.y * speed * Time.deltaTime;
        Vector3 mvtY = transform.right * m_movementInputValue.x * speed * Time.deltaTime;
        transform.position += mvtX + mvtY;
        m_timeMovementStarted += Time.deltaTime;
    }

    public void UpdateCameraRotation()
    {

        if (m_rotationCameraInputValue== Vector2.zero || !m_isRotationButtonPress)
        {
            m_timeRotationStarted = 0;
            return;
        }
        float speed = Mathf.Lerp(minSpeedRotation, maxSpeedRotation, animationCurveRotationSpeed.Evaluate(m_timeRotationStarted / durationMaxRotation));
        float xRot =  m_rotationCameraInputValue.x * speed * Time.deltaTime;
        float yRot = -m_rotationCameraInputValue.y * speed * Time.deltaTime;

        m_currentAngularRotation += new Vector3(yRot, xRot, 0);
        transform.rotation = Quaternion.Euler(m_currentAngularRotation) ;
        m_timeRotationStarted += Time.deltaTime;

    }
    #endregion
}
