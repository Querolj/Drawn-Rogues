using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class Loader
{
    public static FrameInfos LoadFrame (string saveName)
    {
        if (!System.IO.Directory.Exists (SavesPaths.FrameSavePath))
        {
            Debug.LogWarning ("No save availables at " + SavesPaths.FrameSavePath);
            return null;
        }

        string fullSavePath = SavesPaths.FrameSavePath + saveName;

        if (!System.IO.File.Exists (fullSavePath))
        {
            Debug.LogWarning ("No save available at " + fullSavePath);
            return null;
        }

        string content = File.ReadAllText (fullSavePath);
        FrameInfos texInfos = JsonConvert.DeserializeObject<FrameInfos> (content);
        return texInfos;
    }

    public static DrawedCharacterInfos LoadDrawedCharacterInfos (string saveName)
    {
        if (!System.IO.Directory.Exists (SavesPaths.CharacterSavePath))
        {
            Debug.LogWarning ("No save availables at " + SavesPaths.CharacterSavePath);
            return null;
        }

        string fullSavePath = SavesPaths.CharacterSavePath + saveName;

        if (!System.IO.File.Exists (fullSavePath))
        {
            Debug.LogWarning ("No save available at " + fullSavePath);
            return null;
        }

        string content = File.ReadAllText (fullSavePath);
        DrawedCharacterInfos infos = JsonConvert.DeserializeObject<DrawedCharacterInfos> (content);
        return infos;
    }
}