using System.Collections.Generic;
using UnityEngine;

public class DrawedCharacterInfos
{
    public string Name { get; }
    public FrameInfos FrameInfos { get; }
    public List<ModifierInfos> ModifierInfos { get; }
    public DrawedCharacterFormDescription DrawedCharacterFormDescription { get; }

    public DrawedCharacterInfos (FrameInfos frameInfos, string name, List<ModifierInfos> modifierInfos, DrawedCharacterFormDescription drawedCharacterFormDescription)
    {
        FrameInfos = frameInfos;
        Name = name;
        ModifierInfos = modifierInfos;
        DrawedCharacterFormDescription = drawedCharacterFormDescription;
    }
}