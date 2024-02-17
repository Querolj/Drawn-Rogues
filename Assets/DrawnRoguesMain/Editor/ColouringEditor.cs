using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (Colouring))]
[CanEditMultipleObjects]
public class ColouringEditor : Editor
{
    private SerializedProperty _colouringId;
    private SerializedProperty _colouringName;
    private SerializedProperty _colouringBaseColor;
    private SerializedProperty _colouringPixelUsages;
    private SerializedProperty _colouringHasBrushSize;
    private SerializedProperty _colouringUseTextureAsBrush;
    private SerializedProperty _colouringBrushSize;
    private SerializedProperty _colouringTexture;
    private SerializedProperty _colouringTextureUI;
    private SerializedProperty _colouringDescription;

    protected virtual void OnEnable ()
    {
        _colouringId = serializedObject.FindProperty ("_id");
        _colouringName = serializedObject.FindProperty ("Name");
        _colouringBaseColor = serializedObject.FindProperty ("BaseColorsUsedPerPixel");
        _colouringPixelUsages = serializedObject.FindProperty ("PixelUsages");
        _colouringHasBrushSize = serializedObject.FindProperty ("HasBrushSize");
        _colouringUseTextureAsBrush = serializedObject.FindProperty ("UseTextureAsBrush");
        _colouringBrushSize = serializedObject.FindProperty ("BrushSize");
        _colouringTexture = serializedObject.FindProperty ("Texture");
        _colouringTextureUI = serializedObject.FindProperty ("TextureUI");
        _colouringDescription = serializedObject.FindProperty ("Description");
    }

    public override void OnInspectorGUI ()
    {
        serializedObject.Update ();
        // int maxId = GetMaxId ();

        // if (_colouringId.intValue <= 0)
        // {
        //     int suggestedId = maxId + 1;
        //     EditorGUILayout.HelpBox ("You have to set the ID\n Suggested id is " + suggestedId + ", corresponding to max id + 1 ", MessageType.Warning);
        //     EditorGUILayout.LabelField ("Suggested Id : ", suggestedId.ToString ());

        //     if (GUILayout.Button ("Validate Id?"))
        //     {
        //         _colouringId.intValue = suggestedId;
        //     }
        //     return;
        // }

        // EditorGUILayout.LabelField ("Id", _colouringId.intValue.ToString ());
        EditorGUILayout.PropertyField (_colouringId);

        EditorGUILayout.PropertyField (_colouringName);
        EditorGUILayout.PropertyField (_colouringBaseColor);
        EditorGUILayout.PropertyField (_colouringPixelUsages, true);
        EditorGUILayout.PropertyField (_colouringUseTextureAsBrush);

        if (!_colouringUseTextureAsBrush.boolValue)
        {
            EditorGUILayout.PropertyField (_colouringHasBrushSize);

            if (_colouringHasBrushSize.boolValue)
            {
                EditorGUILayout.PropertyField (_colouringBrushSize);
            }
        }

        EditorGUILayout.PropertyField (_colouringTexture);
        EditorGUILayout.PropertyField (_colouringTextureUI);
        EditorGUILayout.LabelField ("Description : ");

        _colouringDescription.stringValue = EditorGUILayout.TextArea (_colouringDescription.stringValue, GUILayout.Height (200));

        serializedObject.ApplyModifiedProperties ();
    }

    private int GetMaxId ()
    {
        Colouring[] colourings = Resources.LoadAll<Colouring> ("Colouring");
        // get max id in colourings

        int maxId = 0;
        foreach (Colouring colouring in colourings)
        {
            if (colouring.Id > maxId)
            {
                maxId = colouring.Id;
            }
        }

        return maxId;
    }
}