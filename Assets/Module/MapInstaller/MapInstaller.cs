using UnityEngine;
using Zenject;

public class MapInstaller : MonoInstaller
{
    [SerializeField]
    private ModeSwitcher _modeSwitcher;

    [SerializeField]
    private MoveIndicator _moveIndicatore;

    [SerializeField]
    private ActionDelayer _actionDelayer;

    [SerializeField]
    private ResizableBrush _resizableBrush;

    public override void InstallBindings ()
    {
        Container.Bind<ModeSwitcher> ().FromComponentsInNewPrefab (_modeSwitcher).AsSingle ();
        Container.Bind<MoveIndicator> ().FromComponentsInNewPrefab (_moveIndicatore).AsSingle ();
        Container.Bind<ActionDelayer> ().FromComponentsInNewPrefab (_actionDelayer).AsSingle ();
        Container.Bind<ResizableBrush> ().FromNewScriptableObject (_resizableBrush).AsSingle ();
    }
}