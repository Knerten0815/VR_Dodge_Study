using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Slider))]
public class DynamicFloatToStringFoSliders : MonoBehaviour
{
    Slider slider;
    [SerializeField] TMP_Text text;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        text.text = slider.value.ToString();        
    }

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(setText);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(setText);
    }

    private void setText(float value)
    {
        text.text = value.ToString();
    }
}
