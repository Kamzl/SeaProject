using System;
using DG.Tweening;
using Objects.Doors;
using StarterAssets;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Zenject;

public class ProgressController : MonoBehaviour
{
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private CharacterController playerCharacterController;
    [SerializeField] private OpeningSceneController openingSceneController;


    private void Awake()
    {
        playerController.RestrictCamera(new Vector2(0, 0), 50, 80);
        playerController.RestrictMovement();
        openingSceneController.Init(playerController, playerCharacterController);
    }
}
