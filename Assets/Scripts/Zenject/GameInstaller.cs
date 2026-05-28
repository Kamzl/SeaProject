using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private InteractableService interactableService;
    public override void InstallBindings()
    {
        Container.Bind<InteractableService>().FromInstance(interactableService);
    }
}
