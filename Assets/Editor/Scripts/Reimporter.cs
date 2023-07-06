using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Reimporter : EditorWindow
{
    [SerializeField]
    private List<Sprite> _spriteToReimport;

    private SerializedObject serializedObject;

    private void OnEnable ()
    {
        serializedObject = new SerializedObject (this);
    }

    private void OnGUI ()
    {
        EditorGUILayout.PropertyField (serializedObject.FindProperty ("_spriteToReimport"));
        serializedObject.ApplyModifiedProperties ();

        EditorGUILayout.Space ();
        EditorGUILayout.Space ();
        EditorGUILayout.Space ();
        EditorGUILayout.Space ();

        Rect lastRect = GUILayoutUtility.GetLastRect ();
        lastRect.size = new Vector2 (lastRect.size.x, lastRect.size.y + 40);
        if (GUI.Button (lastRect, "Reimport all selected sprites"))
        {
            foreach (Sprite sprite in _spriteToReimport)
            {
                if (sprite == null)
                    continue;
                string assetPath = AssetDatabase.GetAssetPath (sprite);
                AssetDatabase.ImportAsset (assetPath, ImportAssetOptions.ImportRecursive);
            }
        }
    }

    [MenuItem ("Tools/Reimporter")]
    public static void ShowReimporter ()
    {
        // This method is called when the user selects the menu item in the Editor
        EditorWindow wnd = GetWindow<Reimporter> ();
        wnd.titleContent = new GUIContent ("Reimporter");
    }
}