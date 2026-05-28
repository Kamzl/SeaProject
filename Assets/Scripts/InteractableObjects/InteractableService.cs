using System.Collections.Generic;
using Scripts.Tools;
using UnityEngine;

public class InteractableService : MonoBehaviour
{
    public InteractableSettings InteractableSettings;
    
    public List<RotateableInfo> rotateableInfo;
    
    [System.Serializable]
    public struct RotateableInfo
    {
        public RotateableObject RotateableObject;
        public float Threshold;
    }
}
