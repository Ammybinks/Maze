////////////////////////////////////////////////////////////////
// Copyright 2013, CompuScholar, Inc.
//
// This source code is for use by the students and teachers who 
// have purchased the corresponding TeenCoder or KidCoder product.
// It may not be transmitted to other parties for any reason
// without the written consent of CompuScholar, Inc.
// This source is provided as-is for educational purposes only.
// CompuScholar, Inc. makes no warranty and assumes
// no liability regarding the functionality of this program.
//
////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Maze
{

    // this class tells us which walls in a square are
    // still up (true) or knocked down (false)
    class WallDirections
    {
        public bool North = false;
        public bool East = false;
        public bool South = false;
        public bool West = false;
    }

    // a Cell represents one square on the maze grid
    class Cell
    {
        // this object tracks which walls are up or down
        public WallDirections Walls = new WallDirections();

        // this property determins the display location of the cell
        public Vector2 UpperLeft;

        // this property should be set to true if a backtracking 
        // algorithm visits the cell at any point 
        public bool Visited = false;

        // this property should be set to true if a backtracking
        // algorithm determins the cell is part of the solution
        public bool Solution = false;

    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 
    
    public class Maze : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // the entire maze is drawn with a single pixel that is 
        // stretched and tinted as needed!
        Texture2D pixelTexture;

        // mazeGrid represent the entire maze as a 2D array of Cells
        private Cell[,] mazeGrid;

        // these constants control how large the cells appear on screen
        private const int CELL_HEIGHT = 50;
        private const int CELL_WIDTH = 50;

        // the number of columns and rows is calculated based on
        // the screen size and the CELL_HEIGHT and CELL_WIDTH
        private int numCols;
        private int numRows;

        // these values are set by the main program to indicate
        // which is the starting cell (0, 0) and ending cell (numCols-1, numRows-1)
        private int startingCol;
        private int startingRow;
        private int endingCol;
        private int endingRow;

        // this enumeration describes a direction that the next step will take
        enum Direction { North, East, South, West }

        // random number generator used to select next step
        Random randomNumGen = new Random(DateTime.Now.Millisecond);

        // when set to true, indicates walkMaze() has found the solution
        bool isDone = false;

        // used to detect user input
        KeyboardState oldKeyboardState;

        // This method is provided complete as part of the activity starter.
        public Maze()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        // This method is provided complete as part of the activity starter.
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // load the pixel image that will be used to draw the maze
            pixelTexture = Content.Load<Texture2D>("pixel");

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        // This method is provided complete as part of the activity starter.
        protected override void UnloadContent()
        {
            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        // This method is provided complete as part of the activity starter.
        protected override void Initialize()
        {

            // get the textures loaded first by calling LoadContent
            base.Initialize();

            // figure out how many columns and rows we can fit with the current screen size
            numCols = (graphics.GraphicsDevice.Viewport.Width / CELL_WIDTH) -1;
            numRows = (graphics.GraphicsDevice.Viewport.Height / CELL_HEIGHT) -1;

            // initialize the starting position as (0,0) and the ending position
            // as the bottom-right square
            startingCol = 0;
            startingRow = 0;
            endingCol = numCols - 1;
            endingRow = numRows - 1;

            // go ahead and automatically generate and solve the first maze
            generateAndWalkMaze();

        }

        // This method is provided complete as part of the activity starter.
        private void generateAndWalkMaze()
        {
            // start by resetting the done flag -- this is a new maze!
            isDone = false;

            // clear the entire maze grid and reset all walls to "up"
            initializeMazeGrid();

            // call student-implemented method to generate a maze
            generateMaze(startingCol, startingRow);

            // reset all of the "visited" flags but leave the maze intact
            resetMazeGrid();

            // call student-implemented method to solve a maze
            walkMaze(startingCol, startingRow);
        }


        // This method is provided complete as part of the activity starter.
        private void initializeMazeGrid()
        {
            // allocate new maze grid
            mazeGrid = new Cell[numCols, numRows];

            // for each cell in the grid
            for (int col = 0; col < numCols; col++)
            {
                for (int row = 0; row < numRows ; row++)
                {
                    // create new cell and initialize the upper left corner
                    mazeGrid[col, row] = new Cell();
                    mazeGrid[col, row].UpperLeft = new Vector2(CELL_WIDTH / 2 + col * CELL_WIDTH, 
                                                               CELL_HEIGHT/ 2 + row * CELL_HEIGHT);

                    //initially all cells have all four walls
                    mazeGrid[col, row].Walls.North = true;
                    mazeGrid[col, row].Walls.East = true;
                    mazeGrid[col, row].Walls.South = true;
                    mazeGrid[col, row].Walls.West = true;
                 }
            }
        }

        // This method is provided complete as part of the activity starter.
        private void resetMazeGrid()
        {
            // reset the visited flag on all cells to false
            for (int col = 0; col < numCols; col++)
            {
                for (int row = 0; row < numRows; row++)
                {
                    mazeGrid[col, row].Visited = false;
                }
            }
        }


        // the student will complete this method as part of the chapter activity
        private void generateMaze(int currentCol, int currentRow)
        {
            mazeGrid[currentCol, currentRow].Visited = true;

            LinkedList<Direction> AvailableDirections = getAvailableDirections(currentCol, currentRow);

            while (AvailableDirections.Count > 0)
            {
                Direction CurrentDirection = AvailableDirections.ElementAt(randomNumGen.Next(0, AvailableDirections.Count));

                int NewCol = currentCol;
                int NewRow = currentRow;

                if (CurrentDirection == Direction.East)
                {
                    // remove east wall
                    mazeGrid[currentCol, currentRow].Walls.East = false;

                    // take a step to adjacent block
                    NewCol += 1;

                    // remove west wall from adjacent block
                    mazeGrid[NewCol, NewRow].Walls.West = false;
                }

                if (CurrentDirection == Direction.West)
                {
                    // remove west wall
                    mazeGrid[currentCol, currentRow].Walls.West = false;

                    // take a step to adjacent block
                    NewCol -= 1;

                    // remove east wall from adjacent block
                    mazeGrid[NewCol, NewRow].Walls.East = false;
                }

                if (CurrentDirection == Direction.North)
                {
                    // remove north wall
                    mazeGrid[currentCol, currentRow].Walls.North = false;

                    // take a step to adjacent block
                    NewRow -= 1;

                    // remove south wall from adjacent block
                    mazeGrid[NewCol, NewRow].Walls.South = false;
                }

                if (CurrentDirection == Direction.South)
                {
                    // remove north wall
                    mazeGrid[currentCol, currentRow].Walls.South = false;

                    // take a step to adjacent block
                    NewRow += 1;

                    // remove south wall from adjacent block
                    mazeGrid[NewCol, NewRow].Walls.North = false;
                }

                generateMaze(NewCol, NewRow);
                AvailableDirections = getAvailableDirections(currentCol, currentRow);
            }

        }

    
        // the student will complete this method as part of the chapter activity
        private void walkMaze(int currentCol, int currentRow)
        {
            if (currentCol == endingCol && currentRow == endingRow)
            {
                isDone = true;
                return;
            }

            mazeGrid[currentCol, currentRow].Visited = true;
            mazeGrid[currentCol, currentRow].Solution = true;

            LinkedList<Direction> AvailableDirections = getAvailableSolutions(currentCol, currentRow);

            while (AvailableDirections.Count > 0)
            {
                Direction CurrentDirection = AvailableDirections.ElementAt(randomNumGen.Next(0, AvailableDirections.Count));

                int NewCol = currentCol;
                int NewRow = currentRow;

                if (CurrentDirection == Direction.East)
                    NewCol++;

                if (CurrentDirection == Direction.West)
                    NewCol--;

                if (CurrentDirection == Direction.North)
                    NewRow--;

                if (CurrentDirection == Direction.South)
                    NewRow++;

                walkMaze(NewCol, NewRow);

                if (isDone == true)
                    return;

                AvailableDirections = getAvailableSolutions(currentCol, currentRow);

            }

            mazeGrid[currentCol, currentRow].Solution = false;
            return;
        }

        // This method is provided complete as part of the activity starter.
        private LinkedList<Direction> getAvailableSolutions(int currentCol, int currentRow)
        {
            LinkedList<Direction> result = new LinkedList<Direction>();

            // Can we step to the east without hitting a wall
            // or previously visited square?
            if ((currentCol + 1 < numCols ) && (mazeGrid[currentCol + 1, currentRow].Visited == false) && (mazeGrid[currentCol, currentRow].Walls.East == false))
            {
                // add this direction to the list of available directions
                result.AddLast(Direction.East);
            }

            // Can we step to the west without hitting a wall
            // or previously visited square?
            if ((currentCol - 1 >= 0) && (mazeGrid[currentCol - 1, currentRow].Visited == false) && (mazeGrid[currentCol, currentRow].Walls.West == false))
            {
                // add this direction to the list of available directions
                result.AddLast(Direction.West);
            }

            // Can we step to the south without hitting a wall
            // or previously visited square?
            if ((currentRow + 1 < numRows) && (mazeGrid[currentCol, currentRow + 1].Visited == false) && (mazeGrid[currentCol, currentRow].Walls.South == false))
            {
                // add this direction to the list of available directions
                result.AddLast(Direction.South);
            }

            // Can we step to the north without hitting a wall
            // or previously visited square?
            if ((currentRow - 1 >= 0) && (mazeGrid[currentCol, currentRow - 1].Visited == false) && (mazeGrid[currentCol, currentRow].Walls.North == false))
            {
                // add this direction to the list of available directions
                result.AddLast(Direction.North);
            }

            // return whatever list we have made, containing
            // 0, 1, 2, 3, or 4 directions
            return result;
        }

        // This method is provided complete as part of the activity starter.
        private LinkedList<Direction> getAvailableDirections(int currentCol, int currentRow)
        {
            LinkedList<Direction> result = new LinkedList<Direction>();

            // Can we step to the east without hitting a previously visited square?
            if ((currentCol + 1 < numCols) && (mazeGrid[currentCol + 1, currentRow].Visited == false))
            {
                // add this direction to the list of available directions
                result.AddLast(Direction.East);
            }

            // Can we step to the west without hitting a previously visited square?
            if ((currentCol - 1 >= 0) && (mazeGrid[currentCol - 1, currentRow].Visited == false))
            {
                // add this direction to the list of available directions
                result.AddLast(Direction.West);
            }

            // Can we step to the south without hitting a previously visited square?
            if ((currentRow + 1 < numRows) && (mazeGrid[currentCol, currentRow + 1].Visited == false))
            {
                // add this direction to the list of available directions
                result.AddLast(Direction.South);
            }

            // Can we step to the north without hitting a previously visited square?
            if ((currentRow - 1 >= 0) && (mazeGrid[currentCol, currentRow - 1].Visited == false))
            {
                // add this direction to the list of available directions
                result.AddLast(Direction.North);
            }

            // return whatever list we have made, containing
            // 0, 1, 2, 3, or 4 directions
            return result;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        // This method is provided complete as part of the activity starter.
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // initialize keyboard states
            KeyboardState currentKeyboard = Keyboard.GetState();
            if (oldKeyboardState == null)
                oldKeyboardState = currentKeyboard;

            // if space bar was just released
            if (oldKeyboardState.IsKeyDown(Keys.Space) && currentKeyboard.IsKeyUp(Keys.Space))
            {
                // generate and solve a new maze!
                generateAndWalkMaze();
            }

            // if escape key was just released
            if (oldKeyboardState.IsKeyDown(Keys.Escape) && currentKeyboard.IsKeyUp(Keys.Escape))
            {
                // exit program!
                this.Exit();
            }

            // update old keyboard state for next time
            oldKeyboardState = currentKeyboard;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        // This method is provided complete as part of the activity starter.
        protected override void Draw(GameTime gameTime)
        {
            // create white background
            GraphicsDevice.Clear(Color.White);

            // determine how thick the maze walls will be
            int lineWidth = 4;

            // calculate the size and location offset of the large solution square
            int solutionBlockWidth = CELL_WIDTH / 2;
            int solutionBlockHeight = CELL_HEIGHT / 2;
            int solutionBlockXOffset = (CELL_WIDTH - solutionBlockWidth) / 2 + lineWidth / 2;
            int solutionBlockYOffset = (CELL_HEIGHT - solutionBlockHeight) / 2 + lineWidth / 2;

            // calculate the size and location offset of the smaller visited square
            int visitedBlockWidth = CELL_WIDTH / 4;
            int visitedBlockHeight = CELL_HEIGHT / 4;
            int visitedBlockXOffset = (CELL_WIDTH - visitedBlockWidth) / 2 + lineWidth / 2;
            int visitedBlockYOffset = (CELL_HEIGHT - visitedBlockHeight) / 2 + lineWidth / 2;

            // for a grayscale image suitable for black and white printing, uncomment the lines below
            // and comment the second set of colors
            //Color startColor = Color.Black;
            //Color endColor = Color.Black;
            //Color solutionColor = Color.DarkGray;
            //Color visitedColor = Color.LightGray;

            // for a color image suitable for viewing onscreen, uncomment the lines below
            // and comment the first set of colors
            Color startColor = Color.Red;
            Color endColor = Color.Green;
            Color solutionColor = Color.Blue;
            Color visitedColor = Color.DarkGray;

            spriteBatch.Begin();

            // for each cell in the maze
            for (int col = 0; col < numCols; col++)
            {
                for (int row = 0; row < numRows; row++)
                {
                    // if this is the starting square, draw it in the starting color
                    if ((col == startingCol) && (row == startingRow))
                        spriteBatch.Draw(pixelTexture, new Rectangle((int)mazeGrid[col, row].UpperLeft.X + lineWidth + 1,
                                                                     (int)mazeGrid[col, row].UpperLeft.Y + lineWidth + 1,
                                                                     CELL_WIDTH - lineWidth - 2, CELL_HEIGHT - lineWidth - 2), null, startColor);

                    // if this is the ending square, draw it in the ending color
                    else if ((col == endingCol) && (row == endingRow))
                        spriteBatch.Draw(pixelTexture, new Rectangle((int)mazeGrid[col, row].UpperLeft.X + lineWidth + 1,
                                                                     (int)mazeGrid[col, row].UpperLeft.Y + lineWidth + 1,
                                                                     CELL_WIDTH - lineWidth - 2, CELL_HEIGHT - lineWidth - 2), null, endColor);

                    // if this square is part of the solution, draw the large interior square
                    else if (mazeGrid[col, row].Solution)
                        spriteBatch.Draw(pixelTexture, new Rectangle((int)mazeGrid[col, row].UpperLeft.X + solutionBlockXOffset,
                                                                     (int)mazeGrid[col, row].UpperLeft.Y + solutionBlockYOffset,
                                                                     solutionBlockWidth, solutionBlockHeight), null, solutionColor);

                    // if this square was visited, draw the small interior square
                    else if (mazeGrid[col, row].Visited)
                        spriteBatch.Draw(pixelTexture, new Rectangle((int)mazeGrid[col, row].UpperLeft.X + visitedBlockXOffset,
                                                                     (int)mazeGrid[col, row].UpperLeft.Y + visitedBlockYOffset,
                                                                     visitedBlockWidth, visitedBlockHeight), null, visitedColor);

                    // if the west wall is up, draw the wall
                    if (mazeGrid[col, row].Walls.West)
                        spriteBatch.Draw(pixelTexture, new Rectangle((int)mazeGrid[col, row].UpperLeft.X,
                                                                     (int)mazeGrid[col, row].UpperLeft.Y,
                                                                     lineWidth,CELL_HEIGHT), null,Color.Black);

                    // if the east wall is up, draw the wall
                    if (mazeGrid[col, row].Walls.East)
                        spriteBatch.Draw(pixelTexture, new Rectangle((int)mazeGrid[col, row].UpperLeft.X + CELL_WIDTH,
                                                                     (int)mazeGrid[col, row].UpperLeft.Y,
                                                                     lineWidth, CELL_HEIGHT+ lineWidth), null, Color.Black);

                    // if the north wall is up, draw the wall
                    if (mazeGrid[col, row].Walls.North)
                        spriteBatch.Draw(pixelTexture, new Rectangle((int)mazeGrid[col, row].UpperLeft.X,
                                                                     (int)mazeGrid[col, row].UpperLeft.Y,
                                                                     CELL_WIDTH, lineWidth), null, Color.Black);

                    // if the south wall is up, draw the wall
                    if (mazeGrid[col, row].Walls.South)
                        spriteBatch.Draw(pixelTexture, new Rectangle((int)mazeGrid[col, row].UpperLeft.X,
                                                                     (int)mazeGrid[col, row].UpperLeft.Y + CELL_HEIGHT,
                                                                     CELL_WIDTH + lineWidth, lineWidth), null, Color.Black);
                }
            }
            
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
