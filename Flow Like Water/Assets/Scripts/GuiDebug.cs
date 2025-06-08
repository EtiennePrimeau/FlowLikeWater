using System;
using System.Collections.Generic;
using UnityEngine;

public class GuiDebug : MonoBehaviour
{
    public static GuiDebug Instance;

    private float _offset = 40;
    private GUIStyle _guiStyle;
    private Dictionary<string,string> _finalStrings = new Dictionary<string, string>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        _guiStyle = new GUIStyle();
        _guiStyle.fontSize = 40;
        _guiStyle.normal.textColor = Color.red;
        _guiStyle.fontStyle = FontStyle.Bold;
        _guiStyle.alignment = TextAnchor.MiddleLeft;
    }

    private void OnGUI()
    {
        int i = 0;
        foreach (var value in _finalStrings)
        {
            i++;
            string txt = value.Key + " :  " + value.Value;
            float posY = _offset * i;
            
            GUI.Box(new Rect(30, posY, 100, 30), txt, _guiStyle);
        }
    }

    public void PrintFloat(string name, float value)
    {
        if (_finalStrings.ContainsKey(name))
            _finalStrings[name] = value.ToString();
        else
            _finalStrings.Add(name, value.ToString());
    }
    
    public void PrintString(string name, string value)
    {
        if (_finalStrings.ContainsKey(name))
            _finalStrings[name] = value.ToString();
        else
            _finalStrings.Add(name, value.ToString());
    }
    
    public void PrintVector2(string name, Vector2 value)
    {
        if (_finalStrings.ContainsKey(name))
            _finalStrings[name] = value.ToString();
        else
            _finalStrings.Add(name, value.ToString());
    }
    
    public void PrintVector3(string name, Vector3 value)
    {
        if (_finalStrings.ContainsKey(name))
            _finalStrings[name] = value.ToString();
        else
            _finalStrings.Add(name, value.ToString());
    }
    
    
}