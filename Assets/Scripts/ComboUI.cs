using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboUI : MonoBehaviour
{
    public Image image;
    public UnityEngine.Gradient gradient;
    public void Update () {
        image.color = gradient.Evaluate(Mathf.InverseLerp(0, TwirlyCharacterController.Instance.settings.comboCap, TwirlyCharacterController.Instance.stepCombo));

    }
}
