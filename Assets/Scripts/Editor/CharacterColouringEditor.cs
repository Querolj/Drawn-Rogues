using UnityEditor;

[CustomEditor (typeof (CharacterColouring))]
[CanEditMultipleObjects]
public class CharacterColouringEditor : ColouringEditor
{
    private SerializedProperty _kgPerPixel;
    private SerializedProperty _baseBonus;
    private SerializedProperty _stats;

    protected override void OnEnable ()
    {
        base.OnEnable ();
        _baseBonus = serializedObject.FindProperty ("BaseBonusToMainStats");
        _kgPerPixel = serializedObject.FindProperty ("KilogramPerPixel");
        _stats = serializedObject.FindProperty ("Stats");

    }

    public override void OnInspectorGUI ()
    {
        base.OnInspectorGUI ();
        EditorGUILayout.PropertyField (_baseBonus);
        EditorGUILayout.PropertyField (_kgPerPixel);
        EditorGUILayout.PropertyField (_stats);

        serializedObject.ApplyModifiedProperties ();
    }
}