using System.Collections.Generic;
using UnityEngine;

public enum PixelUsage
{
    None = 0,
    Head = 1,
    Body = 2,
    Arm = 3,
    Leg = 4,
    Map = 5,
    Combat = 6
}

public enum BaseColor
{
    Red,
    Green,
    Blue,
    Yellow,
    Purple,
    Brown,
    Grey,
    White,
    Black
}

public class BaseColorUtils
{
    private static Dictionary<BaseColor, Color> _baseColorToColor = new Dictionary<BaseColor, Color> ()
    { { BaseColor.Black, Color.black }, { BaseColor.Blue, Color.blue }, { BaseColor.Brown, new Color (0.5f, 0.25f, 0f) }, { BaseColor.Green, Color.green }, { BaseColor.Purple, new Color (0.5f, 0f, 0.5f) }, { BaseColor.Red, Color.red }, { BaseColor.White, Color.white }, { BaseColor.Yellow, Color.yellow }, { BaseColor.Grey, Color.grey }
    };

    public static Color GetColor (BaseColor bc)
    {
        if (!_baseColorToColor.ContainsKey (bc))
            throw new System.Exception ("No color for base color " + bc);

        return _baseColorToColor[bc];
    }

    public static string ColorText (string text, BaseColor bc)
    {
        switch (bc)
        {
            case BaseColor.Black:
                return "<color=#000000>" + text + "</color>";
            case BaseColor.Blue:
                return "<color=#0000ff>" + text + "</color>";
            case BaseColor.Brown:
                return "<color=#7f3f00>" + text + "</color>";
            case BaseColor.Green:
                return "<color=#00ff00>" + text + "</color>";
            case BaseColor.Purple:
                return "<color=#7f007f>" + text + "</color>";
            case BaseColor.Red:
                return "<color=#ff0000>" + text + "</color>";
            case BaseColor.White:
                return "<color=#ffffff>" + text + "</color>";
            case BaseColor.Yellow:
                return "<color=#ffff00>" + text + "</color>";
            case BaseColor.Grey:
                return "<color=#7f7f7f>" + text + "</color>";
            default:
                throw new System.Exception ("No color for base color " + bc);
        }
    }

    public static string GetColorName (BaseColor bc)
    {
        switch (bc)
        {
            case BaseColor.Black:
                return "Black";
            case BaseColor.Blue:
                return "Blue";
            case BaseColor.Brown:
                return "Brown";
            case BaseColor.Green:
                return "Green";
            case BaseColor.Purple:
                return "Purple";
            case BaseColor.Red:
                return "Red";
            case BaseColor.White:
                return "White";
            case BaseColor.Yellow:
                return "Yellow";
            case BaseColor.Grey:
                return "Grey";
            default:
                throw new System.Exception ("No color for base color " + bc);
        }
    }

    public static string GetColoredColorName (BaseColor bc)
    {
        return ColorText (GetColorName (bc), bc);
    }
}