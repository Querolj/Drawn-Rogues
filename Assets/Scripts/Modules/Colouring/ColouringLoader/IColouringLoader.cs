using System.Collections.Generic;

public interface IColouringLoader
{
    Dictionary<BaseColor, List<Colouring>> GetColourings (ColouringType colouringType);
    int GetMaxColouringList (ColouringType colouringType);
}