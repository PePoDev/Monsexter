using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDialog : MonoBehaviour
{
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    public void GameExit()
    {
        Application.Quit();
    }
}
