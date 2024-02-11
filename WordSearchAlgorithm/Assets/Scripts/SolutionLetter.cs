using UnityEngine;
using System.Collections;

//---------------------------------------------------------------------------
// Used to represent a grid element (letter, position, colour etc..)
//---------------------------------------------------------------------------
class LetterType			
{
	public  char	letter;
    public  Vector2 screenPosition;
    public  Color color;

	public	LetterType()
    {
        letter = '*';
        screenPosition = new Vector2(0,0);
        color = Color.yellow;
    }
};

//---------------------------------------------------------------------------
// Used to represent a solution letter (including destination and speed 
// for animating)
//---------------------------------------------------------------------------
class SolutionLetterType : LetterType			
{
    public Vector2 destPosition;
    public Vector2 speed;

	public SolutionLetterType()
    {
        destPosition    = new Vector2(0,0);
        speed           = new Vector2(0, 0);
    }
};

