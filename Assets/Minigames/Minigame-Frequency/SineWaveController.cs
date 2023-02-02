using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SineWaveController : MonoBehaviour
{
    public Slider frequencySlider;
    public Slider amplitudeSlider;
    public Sinewave sineWave;

    void Start()
    {
        frequencySlider.onValueChanged.AddListener((newFrequency) => {
            sineWave.frequency = newFrequency;
        });
        amplitudeSlider.onValueChanged.AddListener((newAmplitude) => {
            sineWave.amplitude = newAmplitude;
        });
    }

    public void EnableSliders(bool input)
    {
        if (!input)
        {
            frequencySlider.enabled = false;
            amplitudeSlider.enabled = false;
        }
    }
}
