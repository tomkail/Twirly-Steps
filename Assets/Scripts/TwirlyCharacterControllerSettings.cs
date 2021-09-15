using UnityEngine;

public class TwirlyCharacterControllerSettings : ScriptableObject {
    public GaitMode gaitMode;
    public enum GaitMode {
        Fixed,
        Adjustable,
        AngularVelocityVariable,
    }
    public float fixedGait = 1;
    public Vector2 adjustableGaitRange = new Vector2(0.2f, 2f);
    public float gaitAdjustSpeed = 1;
    public AnimationCurve gaitOverAngularVelocity;
    
    [Space]
    public AngularVelocityMode angularVelocityMode;
    public enum AngularVelocityMode {
        Fixed,
        Adjustable,
    }
    public bool changeDirectionOnChangingLeg;
    public float fixedAngularVelocity = 1000;
    public float inputImpulse = 1000;
    public float linearAngularDrag = 3;
    public float quadraticAngularDrag = 3;
    public float velocityMultiplierOnChangingLeg = 3;
}