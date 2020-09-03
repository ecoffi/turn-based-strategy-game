using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundController : MonoBehaviour
{
    public enum HighlightType
    {
        None, //no highlight
        Moveable, //unit can move to this space
        Attackable //unit can attack this space
    }
    
    private SpriteRenderer _highlight; //spriterenderer of highlight outline
    
    // Start is called before the first frame update
    void Start()
    {
        _highlight = GetComponentInChildren<SpriteRenderer>();
    }

    public void SetHighlight(HighlightType highlight)
    {
        switch (highlight)
        {
            case HighlightType.None:
                _highlight.enabled = false;
                break;
            case HighlightType.Moveable:
                _highlight.enabled = true;
                _highlight.color = Color.white;
                break;
            case HighlightType.Attackable:
                _highlight.enabled = true;
                _highlight.color = new Color(1f, 0.32f, 0.27f);
                break;
        }
    }
}
