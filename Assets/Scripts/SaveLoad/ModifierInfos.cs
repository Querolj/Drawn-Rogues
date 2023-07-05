using Newtonsoft.Json;
using UnityEngine;
public class ModifierInfos
{
    public Vector2 PercentagePosition { get; set; }
    public string SOFileName { get; private set; }
    public bool IsFlipped { get; private set; }

    [JsonIgnore]
    public Modifier Modifier { get; set; }

    [JsonConstructor]
    public ModifierInfos (Vector3 percentagePos, string soFileName, bool isFlipped)
    {
        percentagePos.z = 0f;
        PercentagePosition = percentagePos;
        SOFileName = soFileName;
        IsFlipped = isFlipped;
    }

    public ModifierInfos (Vector3 percentagePos, Modifier modifier, bool isFlipped)
    {
        percentagePos.z = 0f;
        PercentagePosition = percentagePos;
        SOFileName = modifier.Type + "/" + modifier.Name;
        Modifier = modifier;
        IsFlipped = isFlipped;
    }

    public Vector3 GetLocalPosition (Bounds bounds, Vector3 pivot)
    {
        Vector3 pos = new Vector3 (PercentagePosition.x * bounds.size.x, PercentagePosition.y * bounds.size.y, 0f);
        Vector3 pivotLocation = new Vector3 (pivot.x * bounds.size.x, pivot.y * bounds.size.y, 0f);
        pos -= pivotLocation;

        return pos;
    }
}