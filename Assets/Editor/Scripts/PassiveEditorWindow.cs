using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class PassiveEditorWindow : OdinMenuEditorWindow
{
    [MenuItem ("Tools/Passive Editor")]
    private static void OpenWindow ()
    {
        GetWindow<PassiveEditorWindow> ().Show ();
    }

    private CreateNewPassive _createNewPassive;

    private const string RED_CROSS_ICON_PATH = "Assets/Editor/Icons/RedCross.png";
    private Texture2D _redCrossIcon = null;

    protected override OdinMenuTree BuildMenuTree ()
    {
        OdinMenuTree tree = new OdinMenuTree ();
        tree.DefaultMenuStyle = OdinMenuStyle.TreeViewStyle;
        tree.Config.DrawSearchToolbar = true;
        _createNewPassive = new CreateNewPassive ();
        tree.Add ("Create New Passive", _createNewPassive, SdfIconType.Plus);
        tree.AddAllAssetsAtPath ("AttackDefPassive", "Assets/Resources/Passive/AttackPassive/AttackDefPassive", typeof (AttackDefPassive));
        tree.AddAllAssetsAtPath ("AttackOffPassive", "Assets/Resources/Passive/AttackPassive/AttackOffPassive", typeof (AttackOffPassive));
        tree.AddAllAssetsAtPath ("EffectDefPassive", "Assets/Resources/Passive/EffectPassive/EffectDefPassive", typeof (EffectDefPassive));
        tree.AddAllAssetsAtPath ("EffectOffPassive", "Assets/Resources/Passive/EffectPassive/EffectOffPassive", typeof (EffectOffPassive));
        return tree;
    }

    protected override void OnBeginDrawEditors()
    {
        base.OnBeginDrawEditors();
        OdinMenuTreeSelection selected = MenuTree.Selection;

        SirenixEditorGUI.BeginHorizontalToolbar();
        {
            GUILayout.FlexibleSpace();
            if( _redCrossIcon == null)
            {
                _redCrossIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(RED_CROSS_ICON_PATH);
            }

            if(SirenixEditorGUI.ToolbarButton(new GUIContent("  Delete Selected", _redCrossIcon)) && selected.SelectedValue != null)
            {
                Passive selectedPassive = selected.SelectedValue as Passive;
                if(selectedPassive != null)
                {
                    string path = AssetDatabase.GetAssetPath(selectedPassive);
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.SaveAssets();
                }
            }

        }
        SirenixEditorGUI.EndHorizontalToolbar();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if(_createNewPassive != null)
        {
            DestroyImmediate(_createNewPassive.AttackDefPassive);
            DestroyImmediate(_createNewPassive.AttackOffPassive);
            DestroyImmediate(_createNewPassive.EffectDefPassive);
            DestroyImmediate(_createNewPassive.EffectOffPassive);
        }
    }

    public class CreateNewPassive
    {
        public enum PassiveType
        {
            AttackDefPassive,
            AttackOffPassive,
            EffectDefPassive,
            EffectOffPassive
        }

        [PropertySpace (SpaceAfter = 30), OnValueChanged (nameof (UpdateAssetNameExists))]
        [HideLabel, EnumToggleButtons, Title ("Type")]
        public PassiveType Type;

        [PropertySpace (SpaceAfter = 15), OnValueChanged (nameof (UpdateAssetNameExists))]
        public string AssetName;
        private bool assetNameExists = false;

        [InlineEditor (InlineEditorObjectFieldModes.Hidden), ShowIf (nameof (Type), PassiveType.AttackDefPassive)]
        public AttackDefPassive AttackDefPassive;

        [InlineEditor (InlineEditorObjectFieldModes.CompletelyHidden), ShowIf (nameof (Type), PassiveType.AttackOffPassive)]
        public AttackOffPassive AttackOffPassive;

        [InlineEditor (InlineEditorObjectFieldModes.Hidden), ShowIf (nameof (Type), PassiveType.EffectDefPassive)]
        public EffectDefPassive EffectDefPassive;

        [InlineEditor (InlineEditorObjectFieldModes.Hidden), ShowIf (nameof (Type), PassiveType.EffectOffPassive)]
        public EffectOffPassive EffectOffPassive;

        [InlineEditor (InlineEditorObjectFieldModes.CompletelyHidden)]
        public CreateNewPassive ()
        {
            AttackDefPassive = ScriptableObject.CreateInstance<AttackDefPassive> ();
            AttackOffPassive = ScriptableObject.CreateInstance<AttackOffPassive> ();
            EffectDefPassive = ScriptableObject.CreateInstance<EffectDefPassive> ();
            EffectOffPassive = ScriptableObject.CreateInstance<EffectOffPassive> ();
        }

        private bool IsAssetNameEmpty ()
        {
            return string.IsNullOrEmpty (AssetName);
        }

        private void UpdateAssetNameExists ()
        {
            if (Type == PassiveType.AttackDefPassive)
            {
                assetNameExists = AssetDatabase.LoadAssetAtPath<AttackDefPassive> ("Assets/Resources/Passive/AttackPassive/AttackDefPassive/" + AssetName + ".asset") != null;
            }
            else if (Type == PassiveType.AttackOffPassive)
            {
                assetNameExists = AssetDatabase.LoadAssetAtPath<AttackOffPassive> ("Assets/Resources/Passive/AttackPassive/AttackOffPassive/" + AssetName + ".asset") != null;
            }
            else if (Type == PassiveType.EffectDefPassive)
            {
                assetNameExists = AssetDatabase.LoadAssetAtPath<EffectDefPassive> ("Assets/Resources/Passive/EffectPassive/EffectDefPassive/" + AssetName + ".asset") != null;
            }
            else if (Type == PassiveType.EffectOffPassive)
            {
                assetNameExists = AssetDatabase.LoadAssetAtPath<EffectOffPassive> ("Assets/Resources/Passive/EffectPassive/EffectOffPassive/" + AssetName + ".asset") != null;
            }
        }

        private bool IsAssetNameAlreadyCreated ()
        {
            return assetNameExists;
        }

        [InfoBox ("Make sure to set the AssetName before creating the passive", InfoMessageType.Error, VisibleIf = nameof (IsAssetNameEmpty))]
        [InfoBox ("A passive with this name already exists", InfoMessageType.Error, VisibleIf = nameof (IsAssetNameAlreadyCreated))]
        [Button ("Create New Passive", ButtonSizes.Large)]
        private void CreateNewData ()
        {
            if(IsAssetNameEmpty() || IsAssetNameAlreadyCreated())
            {
                Debug.LogError("Can't create asset, asset name is empty or already exists");
                return;
            }

            if (Type == PassiveType.AttackDefPassive)
            {
                AssetDatabase.CreateAsset (AttackDefPassive, "Assets/Resources/Passive/AttackPassive/AttackDefPassive/" + AssetName + ".asset");
                AttackDefPassive = ScriptableObject.CreateInstance<AttackDefPassive> ();
            }
            else if (Type == PassiveType.AttackOffPassive)
            {
                AssetDatabase.CreateAsset (AttackOffPassive, "Assets/Resources/Passive/AttackPassive/AttackOffPassive/" + AssetName + ".asset");
                AttackOffPassive = ScriptableObject.CreateInstance<AttackOffPassive> ();
            }
            else if (Type == PassiveType.EffectDefPassive)
            {
                AssetDatabase.CreateAsset (EffectDefPassive, "Assets/Resources/Passive/EffectPassive/EffectDefPassive/" + AssetName + ".asset");
                EffectDefPassive = ScriptableObject.CreateInstance<EffectDefPassive> ();
            }
            else if (Type == PassiveType.EffectOffPassive)
            {
                AssetDatabase.CreateAsset (EffectOffPassive, "Assets/Resources/Passive/EffectPassive/EffectOffPassive/" + AssetName + ".asset");
                EffectOffPassive = ScriptableObject.CreateInstance<EffectOffPassive> ();
            }

            AssetDatabase.SaveAssets ();
        }
    }
}