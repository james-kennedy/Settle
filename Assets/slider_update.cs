using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class slider_update : MonoBehaviour
{
    private Text txtText;

    public Slider sldSlider;


    // Start is called before the first frame update
    void Start()
    {
        txtText = GetComponent<Text>();
    }

    public void ShowValue()
    {
        string sliderMessage = sldSlider.value.ToString();
        txtText.text = sliderMessage;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
