using System;
using DG.Tweening;
using Objects.Doors;
using R3;
using Scripts.Tools;
using StarterAssets;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Zenject;

public class OpeningSceneController : MonoBehaviour
{
    //Opening Animation
    [Header("Exit Animation"), SerializeField] private DoorController firstDoor;
    [SerializeField] private Transform lightingObject;
    [SerializeField] private Renderer bloomRenderer;
    [SerializeField] private Light outsideLight;
    [SerializeField] private GameObject doorMovementBlocker;
    [SerializeField] private Volume volumeComponent;
    
    //LoweringAnimation
    [Header("Lowering Animation"), SerializeField] private RotateableObject loweringLever;
    [SerializeField] private Transform craneTransform;
    [SerializeField] private Transform tanksRotationTransform;
    [SerializeField] private Transform ropeTransform;
    [SerializeField] private Transform tanksPositionTransform;
    [SerializeField] private Transform outsideDoorTransform;
    
    private float lightDistance = 2.3f;
    private float lightStartIntensity = 20;
    private float lightOriginalIntensity;
    
    [Inject] private InteractableService _interactableService;
    private Sequence _exitAnimation;
    private Sequence _loweringAnimation;
    
    private FirstPersonController playerController;
    private CharacterController playerCharacterController;
    
    private static float ropeYScale = 1.5884f;
    private static float tanksYPos = -1.0542f;

    private void Awake()
    {
        doorMovementBlocker.SetActive(false);
        firstDoor.OnDoorOpen += PlayOpeningAnimation;
        
        loweringLever.OnRotate.Where(x => x > 0.95).Subscribe(f =>
        {
            PlayLoweringAnimation();
            loweringLever.SetIsInteractable(false);
        });
        
    }

    public void Init(FirstPersonController playerController, CharacterController playerCharacterController)
    {
        this.playerController = playerController;
        this.playerCharacterController = playerCharacterController;
        SetupAnimations();
    }
    
    private void PlayOpeningAnimation()
    {
        outsideLight.intensity = lightStartIntensity;
        _exitAnimation.Play();
    }

    private void PlayLoweringAnimation()
    {
        _loweringAnimation.Play();
    }

    private void SetupAnimations()
    {
        //Exit Animation
        lightOriginalIntensity = outsideLight.intensity;
        outsideLight.intensity = lightStartIntensity;
        
        _exitAnimation.Kill();
        _exitAnimation = DOTween.Sequence().Pause();
        
        float startingPosition = playerCharacterController.transform.position.z;
        float endingPosition = 4;
        float animationDuration = 4;
        float lightSetSwitchDuration = animationDuration * (lightDistance - startingPosition) / (endingPosition - startingPosition);

        float startLightObjectPos = lightingObject.position.z;
        float endLightObjectPos = startLightObjectPos + endingPosition - lightDistance;

        Material bloomMaterial = bloomRenderer.material;
        Color endColor = new Vector4(0f, 0f, 0f, 0f);
        
        _exitAnimation.Append(DOTween.To(() => playerCharacterController.transform.position.z,
            x => playerCharacterController.Move(Vector3.forward * (x - playerCharacterController.transform.position.z)),
            lightDistance, lightSetSwitchDuration).SetEase(Ease.InSine));
        
        _exitAnimation.Append(DOTween.To(() => playerCharacterController.transform.position.z, x => playerCharacterController.Move(Vector3.forward * (x - playerCharacterController.transform.position.z)), endingPosition, animationDuration - lightSetSwitchDuration).SetEase(Ease.OutSine).OnComplete(() =>
        {
            playerController.UnrestrictCamera();
            playerController.UnrestrictMovement();
            firstDoor.CloseDoor();
            doorMovementBlocker.SetActive(true);
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
        
        //Lowering Animation
        _loweringAnimation.Kill();
        _loweringAnimation = DOTween.Sequence().Pause();

        float rotationDegreeFirstHalf = 9;
        float rotationDurationFirstHalf = 2;
        float rotationDegreeSecondHalf = 22;
        float rotationDurationSecondHalf = 2;

        float loweringFinalPos = -13;
        float loweringDuration = 5.5f;
        
        _loweringAnimation.Append(craneTransform.DOLocalRotate(new Vector3(0, 0, rotationDegreeFirstHalf), rotationDurationFirstHalf).SetEase(Ease.InCubic));
        _loweringAnimation.Join(tanksRotationTransform.DOLocalRotate(new Vector3(0, 0, -rotationDegreeFirstHalf), rotationDurationFirstHalf).SetEase(Ease.InCubic));
        _loweringAnimation.Append(craneTransform.DOLocalRotate(new Vector3(0, 0, rotationDegreeSecondHalf), rotationDurationSecondHalf).SetEase(Ease.InSine));
        _loweringAnimation.Join(tanksRotationTransform.DOLocalRotate(new Vector3(0, 0, -((rotationDegreeSecondHalf - rotationDegreeFirstHalf) / 1.5f + rotationDegreeFirstHalf)), rotationDurationSecondHalf / 4).SetEase(Ease.OutSine));
        _loweringAnimation.Insert(rotationDurationFirstHalf + rotationDurationSecondHalf / 4, tanksRotationTransform.DOLocalRotate(new Vector3(0, 0, -rotationDegreeSecondHalf), rotationDurationSecondHalf).SetEase(Ease.InSine));
        _loweringAnimation.Append(tanksPositionTransform.DOLocalMoveY(loweringFinalPos, loweringDuration).SetEase(Ease.InQuad).OnComplete(() => tanksPositionTransform.gameObject.SetActive(false)));

        float ropeScale = 8.5f;
        _loweringAnimation.Join(ropeTransform.DOScaleY(ropeScale, loweringDuration).SetEase(Ease.InQuad));
        
        _loweringAnimation.Append(outsideDoorTransform.DOLocalRotate(new Vector3(0, -110, 0), 3).SetEase(Ease.InOutQuad));
    }

}
