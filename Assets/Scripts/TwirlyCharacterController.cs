using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class TwirlyCharacterController : MonoSingleton<TwirlyCharacterController> {
    public TwirlyCharacterControllerSettings settings;
    public Leg leftLeg;
    public Leg rightLeg;
    public Leg pivotLeg => usingLeftLeg ? leftLeg : rightLeg;
    public Leg swingingLeg => !usingLeftLeg ? leftLeg : rightLeg;
    
    [Space]
    public bool usingLeftLeg;
    public float angularVelocity;
    public float gait;
    
    [Space]
    public float stepCombo;
    public float stepComboResetTime = 0.75f;
    public float stepComboTimer = Mathf.Infinity;
    
    [Space]
    public LineRenderer lineRenderer;
    
    void OnEnable () {
        stepComboTimer = Mathf.Infinity;
        angularVelocity = settings.fixedAngularVelocity;
        // moveDirection = Vector3.Slerp(moveDirection, Vector3.Normalize(cameraTarget - pivotLeg.transform.position), Time.deltaTime);
    }

    void Update() {
        if(stepComboTimer != Mathf.Infinity) {
            stepComboTimer -= Time.deltaTime;
            if(stepComboTimer < 0) {
                stepComboTimer = Mathf.Infinity;
                ResetCombo();
            }
        }
        stepCombo = Mathf.Max(0, stepCombo-settings.comboFadeSpeed*Time.deltaTime);

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

        // if(Input.GetKeyDown(KeyCode.Space)) {
        //     usingLeftLeg = !usingLeftLeg;
        //     if(settings.angularVelocityMode == TwirlyCharacterControllerSettings.AngularVelocityMode.Adjustable) {
        //         angularVelocity *= settings.velocityMultiplierOnChangingLeg;
        //     }
        //     if(settings.changeDirectionOnChangingLeg) {
        //         angularVelocity *= -1;
        //     }
        // }

        var peg = IsLegInTrigger(swingingLeg.transform);
        if(InputManager.ActiveDevice.Action1.WasPressed || Input.GetKeyDown(KeyCode.Q)) {
            if(peg == null) {
                angularVelocity = -angularVelocity;
                MissStep();
            } else {
                SetPivotPosition(peg.transform.position);
                usingLeftLeg = !usingLeftLeg;
                angularVelocity = ApplyCombo(settings.fixedAngularVelocity) * (usingLeftLeg ? 1 : -1);
            }
        }
        if(Input.GetKeyDown(KeyCode.W)) {
            if(peg == null) {
                angularVelocity = -angularVelocity;
                MissStep();
            } else {
                angularVelocity = ApplyCombo(settings.fixedAngularVelocity) * -Mathf.Sign(angularVelocity);
            }
        }
        if(InputManager.ActiveDevice.Action2.WasPressed || InputManager.ActiveDevice.DPadDown.WasPressed || Input.GetKeyDown(KeyCode.Space)) {
            if(peg == null) {
                angularVelocity = -angularVelocity;
                MissStep();
            } else {
                SetPivotPosition(peg.transform.position);
                usingLeftLeg = !usingLeftLeg;
                angularVelocity = ApplyCombo(settings.fixedAngularVelocity) * Mathf.Sign(angularVelocity);
                if(peg.reverseDirection) angularVelocity *= -1;
            }
        }
        if(Input.GetKeyDown(KeyCode.Z)) {
            if(peg == null) {
                angularVelocity = -angularVelocity;
                MissStep();
            } else {
                SetPivotPosition(peg.transform.position);
                usingLeftLeg = !usingLeftLeg;
                angularVelocity = ApplyCombo(settings.fixedAngularVelocity) * -1;
            }
        }
        if(Input.GetKeyDown(KeyCode.X)) {
            if(peg == null) {
                angularVelocity = -angularVelocity;
                MissStep();
            } else {
                SetPivotPosition(peg.transform.position);
                usingLeftLeg = !usingLeftLeg;
                angularVelocity = ApplyCombo(settings.fixedAngularVelocity) * 1;
            }
        }
        angularVelocity = ApplyCombo(settings.fixedAngularVelocity) * Mathf.Sign(angularVelocity);
        
        // if(settings.angularVelocityMode == TwirlyCharacterControllerSettings.AngularVelocityMode.FlipFlop) {
        // }

        UpdateForLeg(pivotLeg, swingingLeg);

        lineRenderer.SetPosition(0, leftLeg.transform.position);
        lineRenderer.SetPosition(1, rightLeg.transform.position);
    }

    void ResetCombo () {
        stepCombo = Mathf.Max(stepCombo-1,0);
        angularVelocity = ApplyCombo(settings.fixedAngularVelocity) * Mathf.Sign(angularVelocity);
    }
    void AddCombo () {
        // stepComboTimer = stepComboResetTime;
        stepCombo = Mathf.Min(settings.comboCap, stepCombo+settings.comboGainOnStep);
    }
    float ApplyCombo (float speed) {
        return settings.angularVelocityOverCombo.Evaluate(stepCombo);
        return speed + settings.comboVelocityAdditive*stepCombo;
    }

    Peg IsLegInTrigger (Transform leg) {
        var overlap = Physics.OverlapSphere(leg.position, settings.triggerCheckRadius, settings.nodeLayerMask, QueryTriggerInteraction.Collide);
        if(overlap.Length > 0) {
            return overlap[0].GetComponent<Peg>();
        }
        return null;
    }

    void SetPivotPosition (Vector3 position) {
        var pivotLeg = !usingLeftLeg ? leftLeg : rightLeg;
        pivotLeg.transform.position = position;
        AddCombo();
        StepAudio.Instance.Play();
    }

    void MissStep () {
        ResetCombo();
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
