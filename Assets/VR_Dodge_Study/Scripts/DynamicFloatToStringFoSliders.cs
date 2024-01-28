using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Slider))]
public class DynamicFloatToStringFoSliders : MonoBehaviour
{
    public enum Gains { None, MinRot, MaxRot, MinTrans, MaxTrans, CurveRadius, ResetBuffer }

    Slider slider;
    [SerializeField] TMP_Text text;
    [SerializeField] Gains gainToRead;
    [SerializeField] GlobalConfiguration config;

    private void Start()
    {
        slider = GetComponent<Slider>();
        text.text = slider.value.ToString();

        switch (gainToRead)
        {
            case Gains.MinRot:
                slider.value = config.ROT_AGAINST_USER_GAIN;
                break;
            case Gains.MaxRot:
                slider.value = config.ROT_WITH_USER_GAIN;
                break;
            case Gains.MinTrans:
                slider.value = config.MIN_TRANS_GAIN;
                break;
            case Gains.MaxTrans:
                slider.value = config.MAX_TRANS_GAIN;
                break;
            case Gains.CurveRadius:
                slider.value = config.CURVATURE_RADIUS;
                break;
            case Gains.ResetBuffer:
                slider.value = config.RESET_TRIGGER_BUFFER;
                break;
            default:
                break;
        }
    }

    private void Update()
    {
        text.text = slider.value.ToString("0.00");
    }
}
