using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwirlyCharacterController : MonoBehaviour {
    public TwirlyCharacterControllerSettings settings;
    public Leg leftLeg;
    public Leg rightLeg;
    
    [Space]
    public bool usingLeftLeg;
    public float angularVelocity;
    public float gait;
    
    [Space]
    public LineRenderer lineRenderer;
    
    void Update() {
        if(settings.angularVelocityMode == TwirlyCharacterControllerSettings.AngularVelocityMode.Fixed) {
            angularVelocity = settings.fixedAngularVelocity;
            if(settings.changeDirectionOnChangingLeg)
                angularVelocity *= (usingLeftLeg ? 1 : -1);
        } else if(settings.angularVelocityMode == TwirlyCharacterControllerSettings.AngularVelocityMode.Adjustable) {
            if(Input.GetKey(KeyCode.LeftArrow)) {
                angularVelocity -= settings.inputImpulse * Time.deltaTime;
            }
            if(Input.GetKey(KeyCode.RightArrow)) {
                angularVelocity += settings.inputImpulse * Time.deltaTime;
            }
            angularVelocity = DoLinearDrag(angularVelocity, settings.linearAngularDrag);
            angularVelocity = DoQuadraticDrag(angularVelocity, settings.quadraticAngularDrag);
        }
        
        if(settings.gaitMode == TwirlyCharacterControllerSettings.GaitMode.Fixed) {
            gait = settings.fixedGait;
        } else if(settings.gaitMode == TwirlyCharacterControllerSettings.GaitMode.Adjustable) {
            if(Input.GetKey(KeyCode.UpArrow)) {
                gait += settings.gaitAdjustSpeed * Time.deltaTime;
                gait = Mathf.Clamp(gait, settings.adjustableGaitRange.x, settings.adjustableGaitRange.y);
            }
            if(Input.GetKey(KeyCode.DownArrow)) {
                gait -= settings.gaitAdjustSpeed * Time.deltaTime;
                gait = Mathf.Clamp(gait, settings.adjustableGaitRange.x, settings.adjustableGaitRange.y);
            }
        } else if(settings.gaitMode == TwirlyCharacterControllerSettings.GaitMode.AngularVelocityVariable) {
            gait = settings.gaitOverAngularVelocity.Evaluate(Mathf.Abs(angularVelocity));
        }

        if(Input.GetKeyDown(KeyCode.Space)) {
            usingLeftLeg = !usingLeftLeg;
            if(settings.angularVelocityMode == TwirlyCharacterControllerSettings.AngularVelocityMode.Adjustable) {
                angularVelocity *= settings.velocityMultiplierOnChangingLeg;
            }
            if(settings.changeDirectionOnChangingLeg) {
                angularVelocity *= -1;
            }
        }
        if(usingLeftLeg) {
            UpdateForLeg(leftLeg, rightLeg);
        } else {
            UpdateForLeg(rightLeg, leftLeg);
        }

        lineRenderer.SetPosition(0, leftLeg.transform.position);
        lineRenderer.SetPosition(1, rightLeg.transform.position);
    }

    float DoLinearDrag (float angularVelocity, float linearDragConst) {
        if(linearDragConst <= 0) return angularVelocity;
        var normalizedVelocity = Mathf.Sign(angularVelocity);
        if(normalizedVelocity == 0) return angularVelocity;

        float speed = Mathf.Abs(angularVelocity);
        var dragForce = -normalizedVelocity * Mathf.Clamp01(speed) * linearDragConst * Time.deltaTime;
        angularVelocity += dragForce;
        return angularVelocity;
    }

    float DoQuadraticDrag (float angularVelocity, float quadraticDragConst) {
        if(quadraticDragConst <= 0) return angularVelocity;
        var normalizedVelocity = Mathf.Sign(angularVelocity);
        if(normalizedVelocity == 0) return angularVelocity;

        float speed = Mathf.Abs(angularVelocity);
        var dragForce = -normalizedVelocity * (speed * speed) * quadraticDragConst * Time.deltaTime;
        angularVelocity += dragForce;
        return angularVelocity;
    }

    void UpdateForLeg (Leg pivotLeg, Leg floatingLeg) {
        var direction = (floatingLeg.transform.position-pivotLeg.transform.position).normalized;
        if(direction == Vector3.zero) direction = Vector3.right;
        floatingLeg.transform.position = pivotLeg.transform.position + direction * gait;
        floatingLeg.transform.RotateAround(pivotLeg.transform.position, Vector3.up, angularVelocity * Time.deltaTime);

        pivotLeg.anchorDot.SetActive(true);
        floatingLeg.anchorDot.SetActive(false);
        pivotLeg.trailRenderer.SetActive(true);
        floatingLeg.trailRenderer.SetActive(false);
    }
}
