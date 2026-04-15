using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    public static HudController instance;

    [SerializeField] private Image crosshair;
    [SerializeField] private List<Color> crosshairColors;

    private CrosshairStatus _crosshairStatus;
    private bool _isCrosshairOnInteractable;
    private bool _isInteractableDragged;

    public enum CrosshairStatus
    {
        Default,
        OnInteractable,
        DraggingInteractable
    }

    private void Start()
    {
        ChangeCrosshairStatus(CrosshairStatus.Default);
        instance = this;
    }

    public CrosshairStatus GetCrosshairStatus()
    {
        return _crosshairStatus;
    }

    public void SetIsCrosshairOnInteractable(bool value)
    {
        _isCrosshairOnInteractable = value;
        RecalculateCrosshair();
    }
    
    public void SetIsInteractableDragged(bool value)
    {
        _isInteractableDragged = value;
        RecalculateCrosshair();
    }

    private void RecalculateCrosshair()
    {
        if (_isInteractableDragged)
        {
            ChangeCrosshairStatus(CrosshairStatus.DraggingInteractable);
        }
        else if(_isCrosshairOnInteractable)
        {
            ChangeCrosshairStatus(CrosshairStatus.OnInteractable);
        }
        else
        {
            ChangeCrosshairStatus(CrosshairStatus.Default);
        }
    }
    
    private void ChangeCrosshairStatus(CrosshairStatus status)
    {
        _crosshairStatus = status;
        crosshair.color = crosshairColors[(int)_crosshairStatus];
    }
}
