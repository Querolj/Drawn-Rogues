using UnityEditor;

[CustomEditor (typeof (ColouringSpell))]
[CanEditMultipleObjects]
public class ColouringSpellEditor : ColouringEditor
{
    private SerializedProperty _behaviourPrefab;

    protected override void OnEnable ()
    {
        base.OnEnable ();
        _behaviourPrefab = serializedObject.FindProperty ("BehaviourPrefab");
    }

    public override void OnInspectorGUI ()
    {
        base.OnInspectorGUI ();
        EditorGUILayout.PropertyField (_behaviourPrefab);
        serializedObject.ApplyModifiedProperties ();
    }
}