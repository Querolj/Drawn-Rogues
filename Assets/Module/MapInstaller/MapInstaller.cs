using UnityEngine;
using Zenject;

public class MapInstaller : MonoInstaller
{
    [SerializeField]
    private CursorModeSwitcher _modeSwitcher;

    [SerializeField]
    private MoveIndicator _moveIndicatore;

    [SerializeField]
    private ActionDelayer _actionDelayer;

    [SerializeField]
    private ResizableBrush _resizableBrush;

    [SerializeField]
    private TrajectoryDrawer _trajectoryDrawer;

    [SerializeField]
    private AttackSelectionManager _attackSelectionManager;

    [SerializeField]
    private WorldUIContainer _worldUIContainer;

    [SerializeField]
    private GameObject _combatUIContainer;

    [SerializeField]
    private FightRegistry _fightRegistry;

    [SerializeField]
    private InputMapSwitcher _inputMapSwitcher;

    [SerializeField]
    private TextureTransition _textureTransition;

    [SerializeField]
    private Drawer _drawer;

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