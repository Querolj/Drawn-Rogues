using System;
using UnityEngine;

public class TextureLoader
{
    private static System.Collections.Generic.Dictionary<string, Texture2D> _loadedTexByPath = new System.Collections.Generic.Dictionary<string, Texture2D> ();

    public static Texture2D Load (string texPath)
    {
        if (string.IsNullOrEmpty (texPath))
            throw new ArgumentNullException (nameof (texPath));

        if (_loadedTexByPath.ContainsKey (texPath))
            return _loadedTexByPath[texPath];

        Sprite sprite = Resources.Load<Sprite> (texPath);
        if (sprite == null)
            throw new Exception ("No texture found in path " + texPath);

        _loadedTexByPath.Add (texPath, sprite.texture);

        return sprite.texture;
    }
}