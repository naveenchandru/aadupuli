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
using System.IO.IsolatedStorage;

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

        int TOTAL_GOATS_COUNT = 15;
        String SAVEFILENAMEVSTIGER = "vsTigerState";
        String SAVEFILENAMEVSGOAT = "vsGoatState";
        String SAVEFILENAMETWOPLAYERS = "twoPlayersState";
        int highScore = 14429;
        int currentGoatsIntoBoard;
        int goatsCaptured;

        Texture2D tigerpuck;
        Texture2D goatpuck;
        Texture2D nonepuck;
        Texture2D boardtexture;
        Texture2D mainMenuBackground, tigersTurnText, goatsTurnText;

        GameState gameState, gameStateVsGoat,gameStateVsTiger, gameStateTwoPlayer;

        int screenWidth;
        int screenHeight;

        Board currentBoard, currentBoardVsGoat, currentBoardVsTiger, currentBoardTwoPlayer;
        nodeState winner;
        bool newMoveDone;
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

        SpriteFont goatsCountFont;
        Vector2 goatsRemainTextPos,goatsCapturedTextPos;



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
            
            
            currentBoardVsGoat = new Board();
            currentBoardVsTiger = new Board();
            currentBoardTwoPlayer = new Board();

            fetchSavedState();

            currentBoard = currentBoardVsGoat;
            goatsCaptured = currentBoard.mGoatsIntoBoard - getGoatCount();
            gameStateVsGoat = new GameState();
            gameStateVsTiger = new GameState();
            gameStateTwoPlayer = new GameState();

            winner = nodeState.none;
            
            newMoveDone = false;
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
        void fetchSavedState()
        {
            
            // open isolated storage, and load data from the savefile if it exists.
            #if WINDOWS_PHONE
                        using (IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication())
            #else
                        using (IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain())
            #endif
            {
                if (savegameStorage.FileExists(SAVEFILENAMEVSGOAT))
                {
                    using (IsolatedStorageFileStream fs = savegameStorage.OpenFile(SAVEFILENAMEVSGOAT, System.IO.FileMode.Open))
                    {
                        if (fs != null)
                        {

                            byte[] saveBytes = new byte[4];
                            int count = 0;
                            int puck = 0;
                            int goatsIntoBoard = 0;

                            for (int i = 0; i < 5; i++)
                            {
                                for (int j = 0; j < 6; j++)
                                {
                                    count = fs.Read(saveBytes, 0, 4);

                                    if (count > 0)
                                    {
                                        puck = System.BitConverter.ToInt32(saveBytes, 0);
                                        currentBoardVsGoat.mValues[i, j] = (nodeState)puck;
                         //               System.Diagnostics.Debug.WriteLine("high saved" + highScore);
                                    }
                                }
                            }

                            count = fs.Read(saveBytes, 0, 4);
                            if (count > 0)
                            {
                                goatsIntoBoard = System.BitConverter.ToInt32(saveBytes, 0);
                                currentBoardVsGoat.mGoatsIntoBoard = goatsIntoBoard;
                            }

                            count = fs.Read(saveBytes, 0, 4);
                            if (count > 0)
                            {
                                bool turnForPlayer = System.BitConverter.ToBoolean(saveBytes, 0);
                                currentBoardVsGoat.mTurnForPlayer = turnForPlayer;
                            }

                            // Reload the saved high-score data.
                            
                            
                        }
                    }
                }


                if (savegameStorage.FileExists(SAVEFILENAMEVSTIGER))
                {
                    using (IsolatedStorageFileStream fs = savegameStorage.OpenFile(SAVEFILENAMEVSTIGER, System.IO.FileMode.Open))
                    {
                        if (fs != null)
                        {

                            byte[] saveBytes = new byte[4];
                            int count = 0;
                            int puck = 0;
                            int goatsIntoBoard = 0;

                            for (int i = 0; i < 5; i++)
                            {
                                for (int j = 0; j < 6; j++)
                                {
                                    count = fs.Read(saveBytes, 0, 4);

                                    if (count > 0)
                                    {
                                        puck = System.BitConverter.ToInt32(saveBytes, 0);
                                        currentBoardVsTiger.mValues[i, j] = (nodeState)puck;
                                        //               System.Diagnostics.Debug.WriteLine("high saved" + highScore);
                                    }
                                }
                            }

                            count = fs.Read(saveBytes, 0, 4);
                            if (count > 0)
                            {
                                goatsIntoBoard = System.BitConverter.ToInt32(saveBytes, 0);
                                currentBoardVsTiger.mGoatsIntoBoard = goatsIntoBoard;
                            }

                            count = fs.Read(saveBytes, 0, 4);
                            if (count > 0)
                            {
                                bool turnForPlayer = System.BitConverter.ToBoolean(saveBytes, 0);
                                currentBoardVsTiger.mTurnForPlayer = turnForPlayer;
                            }

                            // Reload the saved high-score data.


                        }
                    }
                }

                if (savegameStorage.FileExists(SAVEFILENAMETWOPLAYERS))
                {
                    using (IsolatedStorageFileStream fs = savegameStorage.OpenFile(SAVEFILENAMETWOPLAYERS, System.IO.FileMode.Open))
                    {
                        if (fs != null)
                        {

                            byte[] saveBytes = new byte[4];
                            int count = 0;
                            int puck = 0;
                            int goatsIntoBoard = 0;

                            for (int i = 0; i < 5; i++)
                            {
                                for (int j = 0; j < 6; j++)
                                {
                                    count = fs.Read(saveBytes, 0, 4);

                                    if (count > 0)
                                    {
                                        puck = System.BitConverter.ToInt32(saveBytes, 0);
                                        currentBoardTwoPlayer.mValues[i, j] = (nodeState)puck;
                                        //               System.Diagnostics.Debug.WriteLine("high saved" + highScore);
                                    }
                                }
                            }

                            count = fs.Read(saveBytes, 0, 4);
                            if (count > 0)
                            {
                                goatsIntoBoard = System.BitConverter.ToInt32(saveBytes, 0);
                                currentBoardTwoPlayer.mGoatsIntoBoard = goatsIntoBoard;
                            }

                            count = fs.Read(saveBytes, 0, 4);
                            if (count > 0)
                            {
                                bool turnForPlayer = System.BitConverter.ToBoolean(saveBytes, 0);
                                currentBoardTwoPlayer.mTurnForPlayer = turnForPlayer;
                            }

                            // Reload the saved high-score data.


                        }
                    }
                }
            }
            
        }

        void stashCurrentGame()
        {
            if (currentMode == gameMode.vsGoat)
            {
                currentBoardVsGoat = currentBoard;
            }
            if (currentMode == gameMode.vsTiger)
            {
                currentBoardVsTiger = currentBoard;
            }
            if (currentMode == gameMode.twoPlayers)
            {
                currentBoardTwoPlayer = currentBoard;
            }
        }

        void gameSaveState()
        {

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

            goatsTurnText = Content.Load<Texture2D>("goatsTurn");
            tigersTurnText = Content.Load<Texture2D>("tigersTurn");

            twoPlayerBtn.load("twoPlayerBtnShow", "twoPlayerBtnPressed", Content);
            onePlayerBtnGoat.load("asGoatBtnShow", "asGoatBtnPressed", Content);
            onePlayerBtnTiger.load("asTigerBtnShow", "asTigerBtnPressed", Content);
            undoBtn.load("undoBtnShow", "undoBtnPressed", Content);

            //
            goatsRemainTextPos = new Vector2(740, 190);
            goatsCapturedTextPos = new Vector2(740, 310);
            goatsCountFont = Content.Load<SpriteFont>("GoatsCount");

            
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

        protected override void OnExiting(object sender, System.EventArgs args)
        {
            // Save the game state (in this case, the high score).
            #if WINDOWS_PHONE
                        IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication();
            #else
                        IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain();
            #endif

            // open isolated storage, and write the savefile.
            IsolatedStorageFileStream fs = null;
            using (fs = savegameStorage.CreateFile(SAVEFILENAMEVSGOAT))
            {
                if (fs != null)
                {
                    // just overwrite the existing info for this example.

                    byte[] ret = new byte[42*4];
                        //System.BitConverter.GetBytes(highScore);

                    byte[] bytes1;
                    int offSet = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        for (int j = 0; j < 6; j++)
                        {
                            bytes1 = System.BitConverter.GetBytes((int)currentBoardVsGoat.mValues[i, j]);
                            Buffer.BlockCopy(bytes1, 0, ret, offSet, bytes1.Length);
                            offSet += bytes1.Length;
                        }
                    }

                    bytes1 = System.BitConverter.GetBytes(currentBoardVsGoat.mGoatsIntoBoard);
                    Buffer.BlockCopy(bytes1, 0, ret, offSet, bytes1.Length);
                    offSet += bytes1.Length;

                    bytes1 = System.BitConverter.GetBytes(currentBoardVsGoat.mTurnForPlayer);
                    Buffer.BlockCopy(bytes1, 0, ret, offSet, bytes1.Length);
                    offSet += bytes1.Length;

                   
                   fs.Write(ret, 0, ret.Length);

                }
            }

            // open isolated storage, and write the savefile.
            fs = null;
            using (fs = savegameStorage.CreateFile(SAVEFILENAMEVSTIGER))
            {
                if (fs != null)
                {
                    // just overwrite the existing info for this example.

                    byte[] ret = new byte[42 * 4];
                    //System.BitConverter.GetBytes(highScore);

                    byte[] bytes1;
                    int offSet = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        for (int j = 0; j < 6; j++)
                        {
                            bytes1 = System.BitConverter.GetBytes((int)currentBoardVsTiger.mValues[i, j]);
                            Buffer.BlockCopy(bytes1, 0, ret, offSet, bytes1.Length);
                            offSet += bytes1.Length;
                        }
                    }

                    bytes1 = System.BitConverter.GetBytes(currentBoardVsTiger.mGoatsIntoBoard);
                    Buffer.BlockCopy(bytes1, 0, ret, offSet, bytes1.Length);
                    offSet += bytes1.Length;

                    bytes1 = System.BitConverter.GetBytes(currentBoardVsTiger.mTurnForPlayer);
                    Buffer.BlockCopy(bytes1, 0, ret, offSet, bytes1.Length);
                    offSet += bytes1.Length;


                    fs.Write(ret, 0, ret.Length);

                }
            }

            // open isolated storage, and write the savefile.
            fs = null;
            using (fs = savegameStorage.CreateFile(SAVEFILENAMETWOPLAYERS))
            {
                if (fs != null)
                {
                    // just overwrite the existing info for this example.

                    byte[] ret = new byte[42 * 4];
                    //System.BitConverter.GetBytes(highScore);

                    byte[] bytes1;
                    int offSet = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        for (int j = 0; j < 6; j++)
                        {
                            bytes1 = System.BitConverter.GetBytes((int)currentBoardTwoPlayer.mValues[i, j]);
                            Buffer.BlockCopy(bytes1, 0, ret, offSet, bytes1.Length);
                            offSet += bytes1.Length;
                        }
                    }

                    bytes1 = System.BitConverter.GetBytes(currentBoardTwoPlayer.mGoatsIntoBoard);
                    Buffer.BlockCopy(bytes1, 0, ret, offSet, bytes1.Length);
                    offSet += bytes1.Length;

                    bytes1 = System.BitConverter.GetBytes(currentBoardTwoPlayer.mTurnForPlayer);
                    Buffer.BlockCopy(bytes1, 0, ret, offSet, bytes1.Length);
                    offSet += bytes1.Length;


                    fs.Write(ret, 0, ret.Length);

                }
            }

            base.OnExiting(sender, args);
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
                    stashCurrentGame();
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
                    gameState = gameStateTwoPlayer;
                    currentBoard = currentBoardTwoPlayer;
                    goatsCaptured = currentBoard.mGoatsIntoBoard - getGoatCount();

                }
                if (onePlayerBtnGoat.pressed)
                {
                    onePlayerBtnGoat.pressed = false;
                    currentMode = gameMode.vsGoat;
                    currentScreen = gameScreens.gamePlayScreen;
                    System.Diagnostics.Debug.WriteLine("current screen" + currentMode);
                    gameState = gameStateVsGoat;
                    currentBoard = currentBoardVsGoat;
                    goatsCaptured = currentBoard.mGoatsIntoBoard - getGoatCount();
                }
                if (onePlayerBtnTiger.pressed)
                {
                    onePlayerBtnTiger.pressed = false;
                    currentMode = gameMode.vsTiger;
                    currentScreen = gameScreens.gamePlayScreen;
                    gameState = gameStateVsTiger;
                    currentBoard = currentBoardVsTiger;
                    goatsCaptured = currentBoard.mGoatsIntoBoard - getGoatCount();
                }

                


            }
               

        }

        void getInputAndUpdateGame()
        {
            
            if (newMoveDone)
            {
                currentBoard.gameWon = CheckForWin();
                newMoveDone = false;
            }
            if (currentBoard.gameWon)
            {
                System.Diagnostics.Debug.WriteLine("Game won already",winner);
                return;
            }

            if ((currentMode == gameMode.vsTiger && currentBoard.mTurnForPlayer) || (currentMode == gameMode.vsGoat && !currentBoard.mTurnForPlayer))
            {
                System.Diagnostics.Debug.WriteLine("move by cpu: " + currentBoard.mTurnForPlayer);

                int movesDepth = 4;
                if (currentBoard.mGoatsIntoBoard > 14)
                {
                    movesDepth = 4;
                }
                


                Board next = currentBoard.FindNextMove(movesDepth);
                gameState.positionslist.Add(currentBoard.mValues);
                gameState.mGoatsIntoBoardList.Add(currentBoard.mGoatsIntoBoard);
                
                currentBoard = next;
                newMoveDone = true;
                goatsCaptured = currentBoard.mGoatsIntoBoard - getGoatCount();


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
                                    currentBoard = next;
                                    newMoveDone = true;
                                    goatsCaptured = currentBoard.mGoatsIntoBoard - getGoatCount();
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
                                    
                                    currentBoard = next;
                                    newMoveDone = true;
                                    goatsCaptured = currentBoard.mGoatsIntoBoard - getGoatCount();
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
                                            currentBoard = next;
                                            newMoveDone = true;
                                            goatsCaptured = currentBoard.mGoatsIntoBoard - getGoatCount();
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

            
        }

        Rectangle getTouchArea(Rectangle puckArea)
        {
            
            puckArea.X -= puckArea.Width / 2;
            puckArea.Y -= puckArea.Height / 2;
            puckArea.Height *= 2;
            puckArea.Width *= 2;
            return puckArea;

        }
        bool CheckForWin()
        {
            //check if tiger won
            if (currentBoard.mGoatsIntoBoard == 15 && getGoatCount() <= 5)
            {
                    winner = nodeState.tiger;
                    return true;
            }
            else if (hasGoatsWon())
            {
                winner = nodeState.goat;
                return true;

            }

            return false;
            
        }

        //GetMovesForTiger()

        bool hasGoatsWon()
        {
            return currentBoard.hasGoatsWon();
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
                    if ( currentBoard.mValues[i,j] == nodeState.goat )
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
                DrawGoatsCount();
                DrawPlayerTurn();

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

        void DrawPlayerTurn()
        {
            Rectangle screenRectangle ;

            if (currentBoard.mTurnForPlayer)
            {
                screenRectangle = new Rectangle(800 - 25 - tigersTurnText.Width, 25, tigersTurnText.Width, tigersTurnText.Height);
                spriteBatch.Draw(tigersTurnText, screenRectangle, Color.White);
            }
            else
            {
                screenRectangle = new Rectangle(800 - 25 - goatsTurnText.Width, 25, goatsTurnText.Width, goatsTurnText.Height);
                spriteBatch.Draw(goatsTurnText, screenRectangle, Color.White);
            }
        }

        void DrawGoatsCount()
        {
            string output1 = (TOTAL_GOATS_COUNT - currentBoard.mGoatsIntoBoard).ToString();
            string output2 = goatsCaptured.ToString();

            // Find the center of the string
            Vector2 FontOrigin1 = goatsCountFont.MeasureString(output1) / 2;
            Vector2 FontOrigin2 = goatsCountFont.MeasureString(output2) / 2;
            // Draw the string
            spriteBatch.DrawString(goatsCountFont, output1, goatsRemainTextPos, Color.Black,
                0, FontOrigin1, 1.0f, SpriteEffects.None, 0.5f);
            spriteBatch.DrawString(goatsCountFont, output2, goatsCapturedTextPos, Color.Black,
                0, FontOrigin2, 1.0f, SpriteEffects.None, 0.5f);


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
