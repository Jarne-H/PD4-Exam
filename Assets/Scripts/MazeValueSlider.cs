using TMPro;
using UnityEngine;

public class MazeValueSlider : MonoBehaviour
{
    [SerializeField]
    private string _text = "Value: ";
    [SerializeField]
    private TextMeshProUGUI _valueText = null;
    [SerializeField]
    private UnityEngine.UI.Slider _slider = null;
    private int _value = 0;
    public int Value
    {
        get { return _value; }
        set
        {
            _value = value;
            if (_slider != null)
            {
                _slider.value = _value;
            }
            UpdateValue();
        }
    }

    void Start()
    {
        //get text value from _valueText if it is not null
        if (_valueText != null && !string.IsNullOrEmpty(_valueText.text))
        {
            _text = _valueText.text;
        }
        else
        {
            Debug.LogError("Value Text is not assigned in the MazeValueSlider script.");
        }

        if (_slider != null)
        {
            _slider.onValueChanged.AddListener(delegate { UpdateValue(); });
            UpdateValue(); // Initialize the value text on start
        }
        else
        {
            Debug.LogError("Slider is not assigned in the MazeValueSlider script.");
        }
    }

    public void UpdateValue()
    {
        if (_valueText != null)
        {
            _valueText.text = _text + _slider.value.ToString();
        }

        _value = (int)_slider.value;
    }
}
