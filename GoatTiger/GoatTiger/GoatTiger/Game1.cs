using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace GoatTiger
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 

    enum nodeState { none, goat, tiger };
    enum gameMode { twoPlayers, vsTiger, vsGoat };
    enum gameScreens { mainMenuScreen, gamePlayScreen, chooseSideOverlay,winnersOverlay,helpScreen };
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        nodeState[,] grid;
        Rectangle[,] positions;
        List<Point> possiblePositions = new List<Point>();
        int currentGoatsIntoBoard;
        Texture2D tigerpuck;
        Texture2D goatpuck;
        Texture2D nonepuck;
        Texture2D boardtexture;
        Texture2D mainMenuBackground;

        GameState gameState;

        int screenWidth;
        int screenHeight;

        Board currentBoard;
        nodeState winner;
        bool puckTouched;
        Point touchedPos;
        gameMode currentMode;
        bool touching;
        gameScreens currentScreen;

        bool twoPlayerBtnTouched;
        gButton twoPlayerBtn;
        gButton onePlayerBtnGoat;
        gButton onePlayerBtnTiger;
        gButton undoBtn;

        //history
        BoardHistory boardHistory = new BoardHistory();
        

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;

            this.graphics.IsFullScreen = true;

            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft |
                                        DisplayOrientation.LandscapeRight;
   
            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Extend battery life under lock.
            InactiveSleepTime = TimeSpan.FromSeconds(1);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            initPosition();
            currentBoard = new Board(grid, false, currentGoatsIntoBoard);
            gameState = new GameState();

            winner = nodeState.none;
            puckTouched = false;
            currentMode = gameMode.vsTiger;
            currentScreen = gameScreens.mainMenuScreen;
            //currentMode = gameMode.vsGoat;

            onePlayerBtnGoat = new gButton(430, 120);
            onePlayerBtnTiger = new gButton(410, 220);
            twoPlayerBtn = new gButton(390,320);
            undoBtn = new gButton(660, 370);
            
            
            base.Initialize();
        }

        void initPosition()
        {
            grid = new nodeState[5, 6];
            positions = new Rectangle[5, 6];
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    grid[i, j] = nodeState.none;
                }
                
            }
            grid[0, 0] = nodeState.tiger;
            grid[1, 2] = nodeState.tiger;
            grid[1, 3] = nodeState.tiger;

            //grid[0, 0] = nodeState.goat;
            //grid[1, 1] = nodeState.goat;
            //grid[1, 2] = nodeState.goat;
            //grid[2, 3] = nodeState.goat;
            //grid[2, 4] = nodeState.goat;



            //grid[3, 0] = nodeState.goat;
            //grid[2, 0] = nodeState.goat;
            //grid[3, 1] = nodeState.goat;
            //grid[3, 2] = nodeState.goat;
            //grid[1, 3] = nodeState.goat;

            //grid[3, 4] = nodeState.goat;
            //grid[3, 5] = nodeState.goat;
            //grid[4, 2] = nodeState.goat;
            //grid[4, 1] = nodeState.goat;
            //grid[1, 5] = nodeState.goat;

            currentGoatsIntoBoard = 0;

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            tigerpuck = Content.Load<Texture2D>("tigerpuck");
            goatpuck = Content.Load<Texture2D>("goatpuck");
            nonepuck = Content.Load<Texture2D>("none");
            boardtexture = Content.Load<Texture2D>("GamePlayBoard");
            mainMenuBackground = Content.Load<Texture2D>("mainmenuscreen");

            twoPlayerBtn.load("twoPlayerBtnShow", "twoPlayerBtnPressed", Content);
            onePlayerBtnGoat.load("asGoatBtnShow", "asGoatBtnPressed", Content);
            onePlayerBtnTiger.load("asTigerBtnShow", "asTigerBtnPressed", Content);
            undoBtn.load("undoBtnShow", "undoBtnPressed", Content);
            
            screenWidth = graphics.GraphicsDevice.PresentationParameters.BackBufferWidth;
            screenHeight = graphics.GraphicsDevice.PresentationParameters.BackBufferHeight;

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                if( currentScreen == gameScreens.mainMenuScreen)
                this.Exit();
                if (currentScreen == gameScreens.gamePlayScreen)
                {
                    currentScreen = gameScreens.mainMenuScreen;
                }
            }

            if (currentScreen == gameScreens.mainMenuScreen)
            {
                mainScreenTouchHanlder();
            }
            else if (currentScreen == gameScreens.gamePlayScreen)
            {
                getInputAndUpdateGame();

            }

         
            base.Update(gameTime);
        }
        void mainScreenTouchHanlder()
        {

            TouchCollection touches = TouchPanel.GetState();

            if (/*!touching &&*/ touches.Count > 0)
            {
                touching = true;
                TouchLocation touch = touches.First();
                System.Diagnostics.Debug.WriteLine("X" + touch.Position.X + "Y" + touch.Position.Y);
                //make a move by computer


                // twoPlayerBtn.setPos(400,200);
                twoPlayerBtn.handeTouch(touch);
                onePlayerBtnGoat.handeTouch(touch);
                onePlayerBtnTiger.handeTouch(touch);

            }
            else
            {
                if (twoPlayerBtn.pressed)
                {
                    twoPlayerBtn.pressed = false;
                    currentMode = gameMode.twoPlayers;
                    currentScreen = gameScreens.gamePlayScreen;
                }
                if (onePlayerBtnGoat.pressed)
                {
                    onePlayerBtnGoat.pressed = false;
                    currentMode = gameMode.vsTiger;
                    currentScreen = gameScreens.gamePlayScreen;
                }
                if (onePlayerBtnTiger.pressed)
                {
                    onePlayerBtnTiger.pressed = false;
                    currentMode = gameMode.vsGoat;
                    currentScreen = gameScreens.gamePlayScreen;
                }

            }
               

        }

        void getInputAndUpdateGame()
        {
            if ((currentMode == gameMode.vsTiger && currentBoard.mTurnForPlayer) || (currentMode == gameMode.vsGoat && !currentBoard.mTurnForPlayer))
            {
                System.Diagnostics.Debug.WriteLine("move by cpu: " + currentBoard.mTurnForPlayer);

                int movesDepth = 4;
                if (currentBoard.mGoatsIntoBoard > 14)
                {
                    movesDepth = 4;
                }
                //for (int k = BoardHistory.history.Count - 8; k < BoardHistory.history.Count; k++)
                //{
                //    if (k > 0)
                //    {
                //        nodeState [,] data1 = currentBoard.mValues;
                //        nodeState[,] data2 = BoardHistory.history.ElementAt(k);

                //        //if (currentBoard.mValues.Equals(BoardHistory.history.ElementAt(k)))
                //        if (data1.Rank == data2.Rank &&
                //            Enumerable.Range(0, data1.Rank).All(dimension => data1.GetLength(dimension) == data2.GetLength(dimension)) && data1.Cast<nodeState>().SequenceEqual(data2.Cast<nodeState>())) 
                //        {
                //            System.Diagnostics.Debug.WriteLine("Equal found");
                //        }
                //    }
                //}


                Board next = currentBoard.FindNextMove(movesDepth);
                gameState.positionslist.Add(currentBoard.mValues);
                gameState.mGoatsIntoBoardList.Add(currentBoard.mGoatsIntoBoard);
                BoardHistory.history.Add(currentBoard.mValues);
                currentBoard = next;


            }
            else
            {
                TouchCollection touches = TouchPanel.GetState();

                if (!touching && touches.Count > 0)
                {
                    touching = true;
                    TouchLocation touch = touches.First();
                    System.Diagnostics.Debug.WriteLine("X" + touch.Position.X + "Y" + touch.Position.Y);
                    //make a move by computer

                    undoBtn.handeTouch(touch);

                    if (currentBoard.mTurnForPlayer)
                        System.Diagnostics.Debug.WriteLine("moving: tiger");
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("moving: goat");
                    }
                    if (currentBoard.mTurnForPlayer)
                    {
                        if (puckTouched)
                        {
                            foreach (var tobemovedpos in possiblePositions)
                            {
                                System.Diagnostics.Debug.WriteLine("actual moving:" + tobemovedpos.X + tobemovedpos.Y + "puck" + grid[tobemovedpos.X, tobemovedpos.Y]);
                                if (getTouchArea(positions[tobemovedpos.X, tobemovedpos.Y]).Contains((int)touch.Position.X, (int)touch.Position.Y))
                                {
                                    System.Diagnostics.Debug.WriteLine("moving:" + tobemovedpos.X + tobemovedpos.Y + "puck" + grid[tobemovedpos.X, tobemovedpos.Y]);
                                    Board next = currentBoard.MakeMove(touchedPos, tobemovedpos);
                                    gameState.positionslist.Add(currentBoard.mValues);
                                    gameState.mGoatsIntoBoardList.Add(currentBoard.mGoatsIntoBoard);
                                    BoardHistory.history.Add(currentBoard.mValues);
                                    currentBoard = next;
                                }


                            }
                            possiblePositions.Clear();
                            puckTouched = false;
                        }
                        else
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                for (int j = 0; j < 6; j++)
                                {

                                    if (getTouchArea(positions[i, j]).Contains((int)touch.Position.X, (int)touch.Position.Y))
                                    {
                                        System.Diagnostics.Debug.WriteLine("touchedpos:" + i + j + "puck" + grid[i, j]);
                                        touchedPos = new Point(i, j);
                                        if (grid[i, j] == nodeState.tiger)
                                        {
                                            puckTouched = true;
                                            foreach (var move in currentBoard.GetShortMovesForTigerPuck(currentBoard.mValues, new Point(i, j)))
                                            {
                                                System.Diagnostics.Debug.WriteLine("possible pos:" + move.X + move.Y + "puck" + grid[i, j]);
                                                possiblePositions.Add(new Point(move.X, move.Y));

                                            }
                                            foreach (var move in currentBoard.GetCaptureMovesForTigerPuck(currentBoard.mValues, new Point(i, j)))
                                            {
                                                System.Diagnostics.Debug.WriteLine("possible pos:" + move.X + move.Y + "puck" + grid[i, j]);
                                                possiblePositions.Add(new Point(move.X, move.Y));

                                            }
                                        }

                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        if (puckTouched)
                        {
                            foreach (var tobemovedpos in possiblePositions)
                            {
                                System.Diagnostics.Debug.WriteLine("actual moving:" + tobemovedpos.X + tobemovedpos.Y + "puck" + grid[tobemovedpos.X, tobemovedpos.Y]);
                                if (getTouchArea(positions[tobemovedpos.X, tobemovedpos.Y]).Contains((int)touch.Position.X, (int)touch.Position.Y))
                                {
                                    System.Diagnostics.Debug.WriteLine("moving:" + tobemovedpos.X + tobemovedpos.Y + "puck" + grid[tobemovedpos.X, tobemovedpos.Y]);
                                    Board next = currentBoard.MakeMove(touchedPos, tobemovedpos);
                                    gameState.positionslist.Add(currentBoard.mValues);
                                    gameState.mGoatsIntoBoardList.Add(currentBoard.mGoatsIntoBoard);
                                    BoardHistory.history.Add(currentBoard.mValues);
                                    currentBoard = next;
                                }


                            }
                            possiblePositions.Clear();
                            puckTouched = false;
                        }
                        else
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                for (int j = 0; j < 6; j++)
                                {

                                    if (getTouchArea(positions[i, j]).Contains((int)touch.Position.X, (int)touch.Position.Y))
                                    {
                                        System.Diagnostics.Debug.WriteLine("touchedpos:" + i + j + "puck" + grid[i, j]);
                                        touchedPos = new Point(i, j);
                                        if (grid[i, j] == nodeState.goat)
                                        {
                                            puckTouched = true;
                                            foreach (var move in currentBoard.GetMovesForGoatPuck(currentBoard.mValues, new Point(i, j)))
                                            {
                                                System.Diagnostics.Debug.WriteLine("possible pos:" + move.X + move.Y + "puck" + grid[i, j]);
                                                possiblePositions.Add(new Point(move.X, move.Y));

                                            }
                                        }
                                        else if (currentBoard.mGoatsIntoBoard < 15 && grid[i, j] == nodeState.none)
                                        {
                                            Board next = currentBoard.MakeMove(new Point(i, j), new Point(i, j));
                                            gameState.positionslist.Add(currentBoard.mValues);
                                            gameState.mGoatsIntoBoardList.Add(currentBoard.mGoatsIntoBoard);
                                            BoardHistory.history.Add(currentBoard.mValues);
                                            currentBoard = next;
                                        }
                                    }
                                }

                            }
                        }
                    }



                    //System.Diagnostics.Debug.WriteLine("MOVE DONE FOR");

                }
                else if (touches.Count == 0)
                {
                    if (undoBtn.pressed)
                    {
                        
                        if (gameState.positionslist.Count != 0 && !(gameState.positionslist.Count == 1 && (currentMode == gameMode.vsGoat || currentMode == gameMode.vsTiger)))
                        {


                            if (currentMode == gameMode.vsGoat || currentMode == gameMode.vsTiger)
                            {
                                gameState.positionslist.RemoveAt(gameState.positionslist.Count - 1);
                                gameState.mGoatsIntoBoardList.RemoveAt(gameState.mGoatsIntoBoardList.Count - 1);
                            }
                            currentBoard.mValues = gameState.positionslist.Last();
                            gameState.positionslist.RemoveAt(gameState.positionslist.Count - 1);
                            
                            currentBoard.mGoatsIntoBoard = gameState.mGoatsIntoBoardList.Last();
                            gameState.mGoatsIntoBoardList.RemoveAt(gameState.mGoatsIntoBoardList.Count - 1);

                            if (currentMode == gameMode.twoPlayers)
                            {
                                currentBoard.mTurnForPlayer = !currentBoard.mTurnForPlayer;
                            }

                        }
                    }
                    undoBtn.pressed = false;
                    touching = false;

                }
            }

            CheckForWin();
        }

        Rectangle getTouchArea(Rectangle puckArea)
        {
            
            puckArea.X -= puckArea.Width / 2;
            puckArea.Y -= puckArea.Height / 2;
            puckArea.Height *= 2;
            puckArea.Width *= 2;
            return puckArea;

        }
        void CheckForWin()
        {
            //check if tiger won
            if (getGoatCount() <= 5)
            {
                winner = nodeState.goat;
            }
            else
            {
                didTigerWin();
            }

            //check if goat won
        }

        //GetMovesForTiger()

        bool didTigerWin()
        {
            List<Point> tigerPositions = new List<Point>();
            
            List<nodeState[,]> listOfMoves = new List<nodeState[,]>();

            //listOfMoves = GetMovesForTiger();
            if (grid[0, 0] == nodeState.tiger)
            {
                tigerPositions.Add(new Point(0, 0));
            }
            for (int i = 1; i < 3; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (grid[i, j] == nodeState.tiger)
                    {
                        tigerPositions.Add(new Point(i, j));
                    }
                }
            }
            for (int i = 0; i < 4; i++)
            {
                if (grid[4, i] == nodeState.tiger)
                {
                    tigerPositions.Add(new Point(4, i));
                }
            }

            foreach (var item in tigerPositions)
	        {
                IsBlocked(item);
	        }
            
            

            return true;
        }

        bool IsBlocked(Point node)
        {
            
            return true;
        }

        int getGoatCount()
        {
            int count = 0;
            
            for (int i = 0; i < 5; i++)
			{
			    for (int j = 0; j < 6; j++)
			    {
                    if ( grid[i,j] == nodeState.goat )
			            count++;
			    } 
			}
            
            return count;
        }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin();
            if (currentScreen == gameScreens.mainMenuScreen)
            {
                DrawMainScreen();
            }
            else if (currentScreen == gameScreens.gamePlayScreen)
            {
                DrawBoard();
                DrawPieces();


            }
            
            //todo
            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
        void DrawMainScreen()
        {
            Rectangle screenRectangle = new Rectangle(0, 0, screenWidth,screenHeight);
            //spriteBatch.Draw(mainMenuBackground, screenRectangle, Color.White);
            spriteBatch.Draw(mainMenuBackground, new Vector2(0, 0), Color.White);
            twoPlayerBtn.draw(spriteBatch);
            onePlayerBtnGoat.draw(spriteBatch);
            onePlayerBtnTiger.draw(spriteBatch);
            

            

            

        }
        void DrawBoard()
        {
            Rectangle screenRectangle = new Rectangle(0, 0, boardtexture.Width,boardtexture.Height);
            spriteBatch.Draw(boardtexture, screenRectangle, Color.White);
            undoBtn.draw(spriteBatch);
        }

        void DrawPieces(){

            Rectangle position;
            grid = currentBoard.mValues;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (i == 0 && j > 0)
                        continue;
                    if ( i == 4 && ( j ==0 || j == 5 ) )
                        continue;
                    
                    switch (grid[i,j])
                    {
                            
                        case nodeState.none:
                            position = GetGridSpace(i,j,nonepuck.Width,nonepuck.Height);
                            positions[i,j] = position;
                            spriteBatch.Draw(nonepuck,position,new Color(0,0,0,0));
                            break;
                        case nodeState.goat:
                            position = GetGridSpace(i, j, goatpuck.Width, goatpuck.Height);
                            positions[i, j] = position;
                            spriteBatch.Draw(goatpuck, position, Color.White);
                            break;
                        case nodeState.tiger:
                            position = GetGridSpace(i, j, tigerpuck.Width, tigerpuck.Height);
                            positions[i, j] = position;
                            spriteBatch.Draw(tigerpuck, position, Color.White);
                            break;
                    }
                    
                }
                
            }

            foreach (var poi in possiblePositions)
            {
                position = GetGridSpace(poi.X, poi.Y, goatpuck.Width, goatpuck.Height);
                spriteBatch.Draw(nonepuck, position, Color.Blue);
            }
        }
        private Rectangle GetGridSpace(int row, int column, int width, int height)
        {
            int centerX = spriteBatch.GraphicsDevice.Viewport.Width / 2;
            int centerY = spriteBatch.GraphicsDevice.Viewport.Height / 2;
            int boardWidth = 400;
            int pieceWidth = 0;

            int x = 0;
            int y = 0;
            switch (row )
            {
                case 0:
                    boardWidth = 300;
                    pieceWidth = boardWidth / 1;
                    //x = ((column+1)*pieceWidth) - (pieceWidth/2)- (width / 2) + ((spriteBatch.GraphicsDevice.Viewport.Width - boardWidth)/2);
                    x = 310 ;
                    y = 54 ;
                    break;
                case 1:
                    y = 149 ;
                    switch (column)
                    {
                        case 0:x = 118; break;
                        case 1:x=206; break;
                        case 2:x=276; break;
                        case 3:x=348; break;
                        case 4:x=415; break;
                        case 5: x = 510; break;
                        default:y = 0;break;
                    }
                    break;
                case 2:
                    y = 235 ;
                    switch (column)
                    {
                        case 0:x = 76; break;
                        case 1:x=161; break;
                        case 2:x=260; break;
                        case 3:x=365; break;
                        case 4:x=456; break;
                        case 5: x = 553; break;
                        default:y = 0;break;
                    }
                    break;
                case 3:
                    y = 332 ;
                    switch (column)
                    {
                        case 0:x = 46; break;
                        case 1:x=140; break;
                        case 2:x=246; break;
                        case 3:x=375; break;
                        case 4:x=483; break;
                        case 5: x = 576; break;
                        default:y = 0;break;
                    }
                    break;
                case 4:
                    y = 412 ;
                    switch (column)
                    {
                        
                        case 1:x=123; break;
                        case 2:x=241; break;
                        case 3:x=381; break;
                        case 4:x=496; break;
                        default:y = 0;break;
                    }
                    break;
                default:   
                    boardWidth = 300;
                    pieceWidth = boardWidth / 4;
                    x = ((column+1)*pieceWidth) - (pieceWidth/2)- (width / 2) + ((spriteBatch.GraphicsDevice.Viewport.Width - boardWidth)/2);
                    break;
            }
            
            //int y = centerY + ((row - 3) * 60) - (height / 2);
            x -= width / 2;
            y -= height / 2;
            return new Rectangle(x, y, width, height);
        }
    }
}
