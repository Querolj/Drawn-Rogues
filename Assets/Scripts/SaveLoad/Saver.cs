using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class Saver
{
    // public static void SaveFrame (Frame2D frame, string saveName)
    // {
    //     if (!System.IO.Directory.Exists (SavesPaths.FrameSavePath))
    //     {
    //         System.IO.Directory.CreateDirectory (SavesPaths.FrameSavePath);
    //     }

    //     FrameInfos texInfos = new FrameInfos (frame);
    //     string output = JsonConvert.SerializeObject (texInfos);
    //     Debug.Log ("saving : " + output);

    //     if (System.IO.File.Exists (SavesPaths.FrameSavePath + saveName))
    //     {
    //         Debug.Log ("overwrite save");
    //     }

    //     using (StreamWriter writer = new StreamWriter (SavesPaths.FrameSavePath + saveName))
    //     {
    //         writer.Write (output);
    //     }
    // }

    public static void SaveDrawedCharacterInfos (DrawedCharacterInfos infos)
    {
        if (!System.IO.Directory.Exists (SavesPaths.CharacterSavePath))
        {
            System.IO.Directory.CreateDirectory (SavesPaths.CharacterSavePath);
        }

        string output = JsonConvert.SerializeObject (infos, Formatting.Indented, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        Debug.Log ("saving : " + output);

        if (System.IO.File.Exists (SavesPaths.CharacterSavePath + infos.Name))
        {
            Debug.Log ("overwrite save");
        }

        using (StreamWriter writer = new StreamWriter (SavesPaths.CharacterSavePath + infos.Name))
        {
            writer.Write (output);
        }
    }
}