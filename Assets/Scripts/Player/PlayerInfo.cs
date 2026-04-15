using System;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public static PlayerInfo instance;

    private void OnEnable()
    {
        instance = this;
    }
}
