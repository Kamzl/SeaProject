using System;
using System.Collections.Generic;
using DG.Tweening;
using Objects.Doors;
using R3;
using Scripts.Tools;
using StarterAssets;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Zenject;

public class ProgressController : MonoBehaviour
{
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private CharacterController playerCharacterController;
    
    //Opening Animation
    [SerializeField] private DoorController firstDoor;
    [SerializeField] private Transform lightingObject;
    [SerializeField] private Renderer bloomRenderer;
    [SerializeField] private Light outsideLight;
    [SerializeField] private Volume volumeComponent;

    private float lightDistance = 2.3f;
    private float lightStartIntensity = 20;
    private float lightOriginalIntensity;
    
    [Inject] private InteractableService _interactableService;
    private Sequence _exitAnimation;

    private void OnEnable()
    {
        playerController.RestrictCamera(new Vector2(0, 0), 50, 80);
        playerController.RestrictMovement();
        firstDoor.OnDoorOpen += PlayAnimation;
        
        lightOriginalIntensity = outsideLight.intensity;
        outsideLight.intensity = lightStartIntensity;
    }

    private void PlayAnimation()
    {
        _exitAnimation.Kill();
        _exitAnimation = DOTween.Sequence();

        float startingPosition = playerCharacterController.transform.position.z;
        float endingPosition = 4;
        float animationDuration = 4;
        float lightSetSwitchDuration = animationDuration * (lightDistance - startingPosition) / (endingPosition - startingPosition);

        float startLightObjectPos = lightingObject.position.z;
        float endLightObjectPos = startLightObjectPos + endingPosition - lightDistance;

        Material bloomMaterial = bloomRenderer.material;
        Color endColor = new Vector4(0f, 0f, 0f, 0f);
        outsideLight.intensity = lightStartIntensity;
        
        _exitAnimation.Append(DOTween.To(() => playerCharacterController.transform.position.z,
            x => playerCharacterController.Move(Vector3.forward * (x - playerCharacterController.transform.position.z)),
            lightDistance, lightSetSwitchDuration).SetEase(Ease.Linear));
        
        _exitAnimation.Append(DOTween.To(() => playerCharacterController.transform.position.z, x => playerCharacterController.Move(Vector3.forward * (x - playerCharacterController.transform.position.z)), endingPosition, animationDuration - lightSetSwitchDuration).SetEase(Ease.OutSine).OnComplete(() =>
        {
            playerController.UnrestrictCamera();
            playerController.UnrestrictMovement();
            firstDoor.CloseDoor();
        }));
        
        _exitAnimation.Join(lightingObject.DOMoveZ(endLightObjectPos, animationDuration - lightSetSwitchDuration).SetEase(Ease.OutSine).OnComplete(() => lightingObject.gameObject.SetActive(false)));

        _exitAnimation.Insert(0, DOTween.To(() => bloomMaterial.GetColor("_EmissionColor"),
            x => bloomMaterial.SetColor("_EmissionColor", x), endColor, animationDuration));
        _exitAnimation.Insert(0, DOTween.To(() => outsideLight.intensity,
            x => outsideLight.intensity = x, lightOriginalIntensity, animationDuration));

        Bloom bloom;
        volumeComponent.profile.TryGet(out bloom);
        float bloomDisableDuration = 1;
        float bloomDisabledIntensity = 0.1f;
        _exitAnimation.Insert(animationDuration - bloomDisableDuration, DOTween.To(() => bloom.intensity.value,
            x => bloom.intensity.value = x, bloomDisabledIntensity, bloomDisableDuration));
        
        _exitAnimation.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
