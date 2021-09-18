using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerSettings : ScriptableObject {
    public CameraProperties defaultCameraProperties;
    public AnimationCurve panSmoothTimeOverDistanceFromTarget;
    public float panMaxSpeed = 10;
    public float rotationSmoothDamp = 2;
    public float rotationMaxSpeed = 260;
}
public class CameraController : MonoBehaviour
{
    public CameraControllerSettings settings;
    public CameraProperties cameraProperties;
    public new Camera camera => GetComponent<Camera>();
    [Space]
    public Vector3 cameraTarget;
    public Vector3 panVelocity;
    [Space]
    public Vector3 moveDirection;
    public float targetLookDegrees;
    public float lookDegrees;
    public float lookDegreesVelocity;

    void OnEnable () {
        cameraTarget = TwirlyCharacterController.Instance.pivotLeg.transform.position;
    }
    void Update() {
        cameraProperties = settings.defaultCameraProperties;
        var targetPosition = TwirlyCharacterController.Instance.pivotLeg.transform.position;
        var distanceToTarget = Vector3.Distance(cameraTarget, targetPosition);
        if(distanceToTarget > 0) {
            cameraTarget = Vector3.SmoothDamp(cameraTarget, targetPosition, ref panVelocity, settings.panSmoothTimeOverDistanceFromTarget.Evaluate(distanceToTarget), settings.panMaxSpeed, Time.deltaTime * 2);
            moveDirection = Vector3.Normalize(targetPosition-cameraTarget);
            if(distanceToTarget > 0.1f)
                targetLookDegrees = Vector3X.SignedDegreesAgainstDirection(Vector3.forward, moveDirection, Vector3.up);
            lookDegrees = Mathf.SmoothDampAngle(lookDegrees, targetLookDegrees, ref lookDegreesVelocity, settings.rotationSmoothDamp, settings.rotationMaxSpeed, Time.deltaTime * 2);
        }
        cameraProperties.axis = Quaternion.identity;
        cameraProperties.worldEulerAngles.y = lookDegrees;
        cameraProperties.targetPoint = cameraTarget;
        cameraProperties.ApplyTo(camera);
    }
}
