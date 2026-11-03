using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuUIHandler : MonoBehaviour
{
    public ColorPicker ColorPicker;
    
    private void Start()
    {
        ColorPicker.Init();
        ColorPicker.onColorChanged += NewColorSelected;
        
        //when the menu load we set the color picker to the right color
        ColorPicker.SelectColor(MainManager.Instance.TeamColor);
    }

    public void StartNew()
    {
        SceneManager.LoadScene(1);
    }
    
    public void Exit()
    {
        //save the team color before exiting
        MainManager.Instance.SaveColor();
        
#if UNITY_EDITOR
        //Don't forget since this use editor code, need to add "using UnityEditor" at the top and wrap it between #if
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    public void NewColorSelected(Color color)
    {
        MainManager.Instance.TeamColor = color;
    }

    public void SaveColorClicked()
    {
        MainManager.Instance.SaveColor();
    }

    public void LoadColorCliked()
    {
        MainManager.Instance.LoadColor();
        ColorPicker.SelectColor(MainManager.Instance.TeamColor);
    }
}
