/*
 * Author: Matthew Minnett
 * Date: 4/21/2023
 * Desc: Uses different data structures to search through grid of letters and find all words from a list
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;

//---------------------------------------------------------------------------
// This class is the main class for the Word Search Game.
// It is responsible for reading in the list of searchable words and the 
// solvable grid of letters.
// Once the data is loaded it displays all the words to be searched on the left
// of the screen and 
// displays the puzzle grid to the right of the screen.
// Once all the words are found it flashes each search word and highlights the 
// found word in the grid (one at a time)
// Once all the found words are highglighted on the grid the remaining "unused" 
// letters are put together to form a word -- this word is the soloution of the puzzle
// The solution is displayed at the bottom of the screen
//---------------------------------------------------------------------------

public class PuzzleType : MonoBehaviour
{
    #region PublicMembers

    public GUIStyle backgroundStyle;        // GUIStyles used to control the appearance of the words and letters
    public GUIStyle LetterStyle;
    public GUIStyle WordStyle;
    public GUIStyle ToggleStyle;

    public TextAsset puzzleWordsAsset;  // text file listing all the words in the puzzle
    public TextAsset puzzleGridAsset;   // text file listing the puzzle grid

    #endregion

    #region PrivateMembers
    //square grid (of letters) dimension
    const int GRID_SIZE = 20;

    //Position Constants
    //Grid Dimensions
    const int X_OFFSET = 0;
    const int Y_OFFSET = 300;
    const int LETTER_WIDTH = 30;
    const int LETTER_HEIGHT = 24;

    //WordDimensions
    const int X_OFFSET_WRAP = 300;
    const int Y_SPACING = 20;

    //Solution  Word Dimensions
    const int Y_OFFSET_SOL = 100;

    //To animate or not
    private bool bAnimate;

    //Timer variables
    HiRezTimer timer = new HiRezTimer();    //create a timer
    private string status;                  //display final results for timer
    private bool bStopTimer;                //decide when to stop timer

    // store directions to search for columns and rows
    int[] row = new int[] { -1, -1, 0, +1, +1, +1, 0, -1 };
    int[] column = new int[] { 0, -1, -1, -1, 0, +1, +1, +1 };

    [SerializeField]
    [Tooltip("Time between each word search")]
    float animTime = 0.05f;

    bool searchComplete = false;
    string solveWord = "";

    LetterType[,] grid = new LetterType[GRID_SIZE, GRID_SIZE];

    //Define your words to find list data structure (you can use WordType as the type for the data structure you choose)
    List<WordType> puzzleWords = new List<WordType>();

    // tracks letter count for each letter
    Dictionary<char, int> letterTracker = new Dictionary<char, int>();

    #endregion

    //---------------------------------------------------------------------------
    //Initializes Data for game
    void Initialize()
    {
        bAnimate = false;
        status = " ";
        bStopTimer = true;

        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                grid[row, col] = new LetterType();
            }
        }

        puzzleWords.Clear();
        letterTracker.Clear();
        searchComplete = false;
        solveWord = "";

        LoadData();
    }

    //-----------------------------------------------------------------------
    // Read in the text assets
    void Awake()
    {
        // ALL yours, hint - look below to LoadData
        Initialize();

    }

    //-----------------------------------------------------------------------
    // Load data (word list and puzzle letter grid from files
    void LoadData()
    {
        //read all words from puzzleWordsAsset asset into a data structure of your choice of WordType
        foreach (string word in puzzleWordsAsset.text.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries))
        {
            WordType newWord = new WordType();

            // newWord is one word to find
            newWord.word = word.ToUpper();

            puzzleWords.Add(newWord);
        }

        //read all lines of the puzzle from the puzzleGridAsset, one line represents one string of characters for one row in the puzzle
        int row = 0;
        foreach (string letterRow in puzzleGridAsset.text.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries))
        {
            int col = 0;

            while (col < letterRow.Length)
            {
                grid[row, col].letter = letterRow[col];
                col++;
            }

            row++;
        }
    }

    //-----------------------------------------------------------------------
    // Update is called once per frame, Esc used to quit the game
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    //-----------------------------------------------------------------------
    // Display
    void OnGUI()
    {
        //Draw label for background
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), " ", backgroundStyle);

        DisplayGrid();
        DisplayWords();

        // button to start search
        if (GUI.Button(new Rect(Screen.width / 2, Screen.height / 2 + Y_OFFSET + 150, 150, 50), "Start Search"))
        {
            timer.Start(); // start the timer

            for(char c = 'A'; c <= 'Z'; c++)
            {
                letterTracker.Add(c, 0); // add each letter to letter tracker
            }

            StartCoroutine(SearchForWords()); // start the word search
        }

        // button to reset search
        if (GUI.Button(new Rect(Screen.width / 2 + 300, Screen.height / 2 + Y_OFFSET + 150, 150, 50), "Reset"))
        {
            for(int i = 0; i < GRID_SIZE; i++) // loop through rows
            { 
                for(int j = 0; j < GRID_SIZE; j++) // loop through columns
                {
                    grid[i, j].color = Color.yellow; // change color back to yellow
                }
            }
            for(int i = 0; i < puzzleWords.Count; i++) // loop through each word
            {
                puzzleWords[i].wordColor = Color.yellow; // set word color back to yellow
            }

            letterTracker.Clear(); // clear the letter tracker
            searchComplete = false; // search is no complete
            solveWord = "";
            status = "";
        }

        bAnimate = GUI.Toggle(new Rect(Screen.width / 2, Screen.height / 2 + Y_OFFSET + 100, 18, 18), bAnimate, "", ToggleStyle); // animate word search toggle
        GUI.Label(new Rect(Screen.width / 2 + 36, Screen.height / 2 + Y_OFFSET + 100, 400, 50), "Animate On/Off", WordStyle);

        if (!bStopTimer) // if timer not stopped
        {
            bStopTimer = true; // stop timer true
            status = "It took " + timer.Stop().ToString("##.##") + " seconds to find all words!";
        }
        GUI.Label(new Rect(Screen.width / 2 + 300, Screen.height / 2 + Y_OFFSET + 100, 400, 50), status, WordStyle); // display time taken to complete search

        if (searchComplete) // if finished searching
        {
            int yPos = 50; // initial position for letter display
            foreach(var v in letterTracker) // for each letter 
            {
                GUI.Label(new Rect(Screen.width / 2 - 200, yPos, 40, 40), v.Key + " : " + v.Value, WordStyle); // display letter with amount of letters in search
                yPos += 25; // add to y so next letter is displayed lower
            }

            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 + Y_OFFSET + 50, 400, 50), solveWord, WordStyle); // display the solved word (adjustable)
        }
    }

    //---------------------------------------------------------------------------
    //Displays the words to search for
    private void DisplayWords()
    {
        int y = Y_SPACING;
        int columnIndex = 4;
        int columnWidth = (Screen.width / 2) / 4;
        int x = (Screen.width / 2) - columnWidth * columnIndex; //start from center and moving from there as offset
        GUI.Label(new Rect(0, 0, 250, 20), "WORDS TO SEARCH: ", WordStyle);

        for (int i = 0; i < puzzleWords.Count; i++)
        {
            WordStyle.normal.textColor = puzzleWords[i].wordColor;

            string temp = puzzleWords[i].word;

            GUI.Label(new Rect(x, y, 200, 20), temp, WordStyle);
            y += Y_SPACING;
            if (y >= (Screen.height - Y_SPACING))
            {
                columnIndex--;
                y = Y_SPACING;
                x = (Screen.width / 2) - columnWidth * columnIndex;
            }
        }
    }

    //---------------------------------------------------------------------------
    // Travels through the answer grid and displays the grid letters
    private void DisplayGrid()
    {
        for (int r = 0; r < GRID_SIZE; r++)       // Build solution word
        {
            int yStart = (Screen.height / 2 - Y_OFFSET) + r * LETTER_HEIGHT;
            for (int c = 0; c < GRID_SIZE; c++)
            {
                int xStart = (Screen.width / 2 - X_OFFSET) + c * LETTER_WIDTH;

                LetterStyle.normal.textColor = grid[r, c].color;

                string charToShow = grid[r, c].letter.ToString();

                GUI.Label(new Rect(xStart, yStart, 200, 20), charToShow, LetterStyle);
            }
        }
    }

    /// <summary>
    /// Finds all the words in the word list
    /// </summary>
    /// <returns></returns>
    IEnumerator SearchForWords()
    {
        for (int i = 0; i < puzzleWords.Count; i++) // loop through all words
        {
            string temp = puzzleWords[i].word.ToString(); // store word at index i
            char tempChar = temp.ToCharArray()[0]; // store first letter of stored word

            string foundWord = ""; // letters are added as word is found

            for (int r = 0; r < GRID_SIZE; r++) // loop for rows
            {
                for (int c = 0; c < GRID_SIZE; c++) // loop for columns
                {
                    char startLetter = grid[r, c].letter; // letter to start with

                    if (startLetter == tempChar) // if letter is equal to start letter of word
                    {
                        int curRow;
                        int curCol;

                        for (int j = 0; j < 8; j++) // loop through all directions
                        {
                            int index = 1; // starting at index 1 as first letter has already been found

                            while (index < temp.Length) // while index is less than length of word being searched for
                            {
                                curRow = r + (row[j] * index); // current row to search
                                curCol = c + (column[j] * index); // current column to search

                                if (curRow >= 0 && curRow < GRID_SIZE && curCol >= 0 && curCol < GRID_SIZE)// if current row and current column are not out of bounds of the grid
                                {
                                    if (grid[curRow, curCol].letter.ToString() != temp[index].ToString()) // if grid location letter is not equal to the next letter in the word
                                    {
                                        break; // break out of loop
                                    }
                                    else
                                    {
                                        index++; // add to index
                                        if (index == temp.Length) // if index is equal to length of word
                                        {
                                            // store variables
                                            int wordLength = temp.Length;
                                            int startRow = r;
                                            int startCol = c;
                                            int rowDir = row[j];
                                            int colDir = column[j];
                                            index = 1; 

                                            while (wordLength > 0) // while the word length is greater than 0
                                            {
                                                if (bAnimate == true)
                                                {
                                                    // animation on
                                                    grid[startRow, startCol].color = Color.blue; // set found letter to blue
                                                    foundWord += grid[startRow, startCol].letter.ToString(); // add found letter to found word

                                                    startRow = startRow + (rowDir * index);
                                                    startCol = startCol + (colDir * index);

                                                    wordLength--; // subtract from word length

                                                    yield return new WaitForSeconds(animTime); // wait before coloring next letter
                                                }
                                                else
                                                {
                                                    // animation off
                                                    grid[startRow, startCol].color = Color.blue;

                                                    foundWord += grid[startRow, startCol].letter.ToString(); // add found letter to found word

                                                    startRow = startRow + (rowDir * index);
                                                    startCol = startCol + (colDir * index);

                                                    wordLength--; // subtract from word length
                                                }
                                            }

                                            if (foundWord == temp) // if complete word found
                                            {
                                                puzzleWords[i].Flash(); // change color of found word
                                                for (int k = 0; k < foundWord.Length; k++) // loop through all letters of found word
                                                {
                                                    if (!letterTracker.ContainsKey(foundWord[k])) // if letter tracker does not contain letter
                                                    {
                                                        letterTracker.Add(foundWord[k], 1); // add it as first entry
                                                    }
                                                    else
                                                    {
                                                        letterTracker[foundWord[k]]++; // increment letter count
                                                    }
                                                }
                                            }

                                            break;
                                        }
                                    }
                                }
                                else // if outside of grid bounds
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        searchComplete = true; // search is complete
        bStopTimer = false; // stop the timer
        StartCoroutine(SolveWord()); // start coroutine to find last word
    }

    /// <summary>
    /// Finds the final word of the word list
    /// </summary>
    /// <returns></returns>
    IEnumerator SolveWord()
    {
        for (int i = 0; i < GRID_SIZE; i++) // through rows
        {
            for (int j = 0; j < GRID_SIZE; j++) // through columns
            {
                if (grid[i, j].color == Color.yellow) // if letter at grid pos is yellow
                {
                    grid[i, j].color = Color.black; // change to black
                    solveWord += grid[i, j].letter; // add letter to word

                    yield return new WaitForSeconds(animTime); // wait before searching for next letter
                }
            }
        }
    }
}
