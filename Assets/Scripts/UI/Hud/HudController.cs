using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    public static HudController instance;

    [SerializeField] private Image crosshair;
    [SerializeField] private Camera UICamera;
    [SerializeField] private List<Color> crosshairColors;
    
    [SerializeField] private RectTransform dragOriginImage;

    private CrosshairStatus _crosshairStatus;
    private bool _isCrosshairOnInteractable;
    private bool _isInteractableDragged;
    private Transform _dragOriginPoint;

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

    private void Update()
    {
        if (_isInteractableDragged)
        {
            if (UICamera != null)
                dragOriginImage.position = UICamera.WorldToScreenPoint(_dragOriginPoint.position);
        }
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
        Debug.Log($"InteractableDragged {value}");
        _isInteractableDragged = value;
        if (!_isInteractableDragged)
        {
            dragOriginImage.gameObject.SetActive(false);
        }
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

    public void PutDragOriginPoint(Transform worldPoint)
    {
        _dragOriginPoint = worldPoint;
        dragOriginImage.gameObject.SetActive(true);
    }
}
