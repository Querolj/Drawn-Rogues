using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class MapInstaller : MonoInstaller
{
    [SerializeField, BoxGroup ("Prefabs")]
    private CursorModeSwitcher _modeSwitcher;

    [SerializeField, BoxGroup ("Prefabs")]
    private MoveIndicator _moveIndicatore;

    [SerializeField, BoxGroup ("Prefabs")]
    private ActionDelayer _actionDelayer;

    [SerializeField, BoxGroup ("Prefabs")]
    private ResizableBrush _resizableBrush;

    [SerializeField, BoxGroup ("Prefabs")]
    private TrajectoryDrawer _trajectoryDrawer;

    [SerializeField, BoxGroup ("Prefabs")]
    private AttackSelectionManager _attackSelectionManager;

    [SerializeField, BoxGroup ("Prefabs")]
    private WorldUIContainer _worldUIContainer;

    [SerializeField, BoxGroup ("Prefabs")]
    private FightRegistry _fightRegistry;

    [SerializeField, BoxGroup ("Prefabs")]
    private InputMapSwitcher _inputMapSwitcher;

    [SerializeField, BoxGroup ("Prefabs")]
    private TextureTransition _textureTransition;

    [SerializeField, BoxGroup ("Prefabs")]
    private Drawer _drawer;

    [SerializeField, BoxGroup ("Objects in scene")]
    private CameraService _camera;

    [SerializeField, BoxGroup ("Objects in scene")]
    private GameObject _combatUIContainer;

    public override void InstallBindings ()
    {
        Container.Bind<CursorModeSwitcher> ().FromComponentsInNewPrefab (_modeSwitcher).AsSingle ();
        Container.Bind<MoveIndicator> ().FromComponentsInNewPrefab (_moveIndicatore).AsTransient ();
        Container.Bind<ActionDelayer> ().FromComponentsInNewPrefab (_actionDelayer).AsSingle ();
        Container.Bind<WorldUIContainer> ().FromComponentsInNewPrefab (_worldUIContainer).AsSingle ();
        Container.Bind<ResizableBrush> ().FromNewScriptableObject (_resizableBrush).AsSingle ();
        Container.Bind<TrajectoryDrawer> ().FromComponentsInNewPrefab (_trajectoryDrawer).AsSingle ();
        Container.Bind<AttackSelectionManager> ().FromComponentsInNewPrefab (_attackSelectionManager).AsSingle ();
        Container.Bind<InputMapSwitcher> ().FromComponentsInNewPrefab (_inputMapSwitcher).AsSingle ();
        Container.Bind<TextureTransition> ().FromComponentsInNewPrefab (_textureTransition).AsTransient ();
        Container.Bind<Drawer> ().FromComponentsInNewPrefab (_drawer).AsSingle ();
        Container.Bind<CameraService> ().FromComponentOn (_camera.gameObject).AsSingle ();

        GameObjectCreationParameters combatUIContainerParams = new GameObjectCreationParameters ();
        combatUIContainerParams.ParentTransform = _combatUIContainer.transform;
        Container.Bind<FightRegistry> ().FromComponentsInNewPrefab (_fightRegistry, combatUIContainerParams).AsSingle ().NonLazy ();
        Container.Bind<BaseColorInventory> ().AsSingle ();
        Container.Bind<TrajectoryCalculator> ().AsSingle ();

        // Factories
        Container.BindFactory<AttackSelection, AttackSelection, AttackSelection.Factory> ().FromFactory<PrefabFactory<AttackSelection>> ();
        Container.BindFactory<GameObject, CombatEntity, CombatEntity.Factory> ().FromFactory<PrefabFactory<CombatEntity>> ();
        Container.BindFactory<Attack, Character, AttackInstance, AttackInstance.Factory> ().FromFactory<AttackInstFactory> ();
    }
}