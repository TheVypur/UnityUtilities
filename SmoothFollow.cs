using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public bool lockon;
    public float followSpeed = 5;
    public float mouseSpeed = 1;
    public float controllerSpeed = 7;

    public Transform target;
    public Transform pivot;
    public Transform camTrans;

    public float turnSmoothing = .9f;
    public float minAngle = -75;
    public float maxAngle = 75;

    float smoothX;
    float smoothY;
    float smoothXVelocity;
    float smoothYVelocity;

    public float lookAngle;
    public float tiltAngle;

    public LayerMask layerMask;

    public float fPivotDampening = 0.2f;
    private Vector3 v3PivotOffset;
    public Vector3 v3PivotOffsetTarget;
    public Vector3 v3BasePivotOffset = Vector3.zero;
    public Vector3 v3FollowOffset = new Vector3(0, 1, 0);

    public float fScrollSensitivity = 0.1f;

    private Vector3 v3DesiredPivotOffset = new Vector3(0, 0, 0);

    public bool bFollowLeftShoulder = true;
    public float fShoulderFollowOffset = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        camTrans = Camera.main.transform;
        pivot = camTrans.parent;

        Application.targetFrameRate = 500;
        QualitySettings.vSyncCount = 0;
        v3DesiredPivotOffset = v3PivotOffset;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void FollowTarget(float fMouseScroll, float delta)
    {
        float speed = delta * followSpeed;
        Vector3 targetPosition = Vector3.Lerp(transform.position, target.position + v3FollowOffset, speed);
        transform.position = targetPosition;
        v3DesiredPivotOffset += (v3DesiredPivotOffset * -fMouseScroll);
    }

    void HandleRotation(float delta, float horizontal, float vertical, float targetSpeed)
    {
        if (turnSmoothing > 0)
        {
            smoothX = Mathf.SmoothDamp(smoothX, horizontal, ref smoothXVelocity, turnSmoothing);
            smoothY = Mathf.SmoothDamp(smoothY, vertical, ref smoothYVelocity, turnSmoothing);
        }
        else
        {
            smoothX = horizontal;
            smoothY = vertical;
        }

        lookAngle += smoothX * targetSpeed;
        transform.rotation = Quaternion.Euler(0, lookAngle, 0);

        tiltAngle -= smoothY * targetSpeed;
        tiltAngle = Mathf.Clamp(tiltAngle, minAngle, maxAngle);
        transform.localRotation = Quaternion.Euler(tiltAngle, lookAngle, 0);

        v3PivotOffset = pivot.localPosition;

        if (bFollowLeftShoulder)
        {
            v3PivotOffsetTarget.x = -fShoulderFollowOffset;
        }
        else
        {
            v3PivotOffsetTarget.x = fShoulderFollowOffset;
        }
        Vector3 v3TargetPos = HandleCollision(v3PivotOffsetTarget);
        pivot.localPosition += (v3TargetPos - v3PivotOffset) * fPivotDampening;
    }

    private void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float inputX = Input.GetAxis("Mouse X") * mouseSpeed;
        float inputY = Input.GetAxis("Mouse Y") * mouseSpeed;
        float inputScroll = Input.mouseScrollDelta.y * fScrollSensitivity;

        FollowTarget(inputScroll, Time.deltaTime);
        HandleRotation(Time.deltaTime, inputX, inputY, controllerSpeed);
    }

    Vector3 HandleCollision(Vector3 v3OriginalTarget)
    {
        RaycastHit hitInfo;
        Vector3 v3DestinationPoint = this.transform.position;

        v3OriginalTarget += v3BasePivotOffset;

        v3DestinationPoint += this.transform.forward * v3OriginalTarget.z;
        v3DestinationPoint += this.transform.right * v3OriginalTarget.x;
        v3DestinationPoint += this.transform.up * v3OriginalTarget.y;

        if (Physics.Linecast(this.transform.position, v3DestinationPoint, out hitInfo, layerMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 v3Ret = hitInfo.point;
            v3Ret = transform.InverseTransformPoint(v3Ret);
            return v3Ret * 0.85f;
        }

        return v3OriginalTarget;
    }
}
