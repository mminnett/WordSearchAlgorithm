using UnityEngine;
using System.Collections;
//---------------------------------------------------------------------------
//Stores row and column information
//---------------------------------------------------------------------------
public class PointType
{
    public int row;
    public  int col;
    public PointType()
    {
        row = 0;
        col = 0;
    }
}

//---------------------------------------------------------------------------
// Stores information for each word object including the word, direction to 
// search color and position
//---------------------------------------------------------------------------
public class WordType
{
 #region PublicDataMembers
    public string word;
    public Color wordColor;
 #endregion

#region PublicMembers

    public WordType()
    {
        word = "";
        wordColor = Color.yellow;
    }

    public void Flash()
    {
        if (wordColor == Color.green)
            wordColor = Color.yellow;
        else
            wordColor = Color.green;
    }

#endregion

}
