using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class TempUIManager : MonoBehaviour
{
    [Header("Subtitles")]
    [SerializeField] private TMP_Text subtitles;
    [SerializeField] private float lingerTime;
    private Coroutine subtitleCoroutine;
    
    [Header("Input Devices")]
    [SerializeField] private TMP_Dropdown deviceDropdown;

    private void Awake()
    {
        deviceDropdown.ClearOptions();
        deviceDropdown.AddOptions(Microphone.devices.ToList());

        if (Managers.Instance && Managers.Instance.Recorder)
        {
            deviceDropdown.value =
                deviceDropdown.options.FindIndex(
                    opt => opt.text == Managers.Instance.Recorder.SelectedMicDevice);
        }
    }

    private void Start()
    {
        subtitles.text = "";
    }

    public void SetSubtitleText(string text)
    {
        // print("Set subtitle text: " + text);
        if (subtitleCoroutine != null) StopCoroutine(subtitleCoroutine);
        subtitleCoroutine = StartCoroutine(SetSubtitleTextCor(text));
    }

    private IEnumerator SetSubtitleTextCor(string text)
    {
        subtitles.text = text;
        yield return new WaitForSeconds(lingerTime);
        subtitles.text = "";
    }
}
