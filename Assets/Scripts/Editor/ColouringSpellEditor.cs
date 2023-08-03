using UnityEditor;

[CustomEditor (typeof (ColouringSpell))]
[CanEditMultipleObjects]
public class ColouringSpellEditor : ColouringEditor
{
    private SerializedProperty _behaviourPrefab;
    private SerializedProperty _clearMetadataOnFrame;

    protected override void OnEnable ()
    {
        base.OnEnable ();
        _behaviourPrefab = serializedObject.FindProperty ("BehaviourPrefab");
        _clearMetadataOnFrame = serializedObject.FindProperty ("ClearMetadataOnFrame");
    }

    public override void OnInspectorGUI ()
    {
        base.OnInspectorGUI ();
        EditorGUILayout.PropertyField (_behaviourPrefab);
        EditorGUILayout.PropertyField (_clearMetadataOnFrame);
        serializedObject.ApplyModifiedProperties ();
    }
}