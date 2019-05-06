using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Loading : MonoBehaviour
{

    public GameObject main;
    public VideoPlayer video;

    private void Start()
    {
        video.Prepare();
    }

    public void StartLoading()
    {
        main.SetActive(true);
        video.Play();
    }

    public void StopLoading()
    {
        main.SetActive(false);
        video.Stop();
    }
}
