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
    enum gameScreens { mainMenuScreen, gamePlayScreen, chooseSideOverlay, winnersOverlay, helpScreen, pauseOverlay, continueOverlay, settingsOverlay };

    

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
        String SAVEFILESETTINGS = "settings";
        int highScore = 14429;
        int currentGoatsIntoBoard;
        int goatsCaptured;

        Texture2D tigerpuck;
        Texture2D goatpuck;
        Texture2D nonepuck;
        Texture2D boardtexture;
        Texture2D mainMenuBackground, tigersTurnText, goatsTurnText;

        Texture2D overlayBGtexture, overlayBG1texture,overlayBG2texture, tigersWonText, goatsWonText, pausedText, continueText, gameDrawnText, settingsText, levelText, SfxText;
        gButton menuBtn,newGameBtn,resumeBtn, okBtn;

        GameState gameState, gameStateVsGoat,gameStateVsTiger, gameStateTwoPlayer;

        int screenWidth;
        int screenHeight;

        Board currentBoard, currentBoardVsGoat, currentBoardVsTiger, currentBoardTwoPlayer, nextBoard;
        nodeState winner;
        bool newMoveDone;
        bool puckTouched;
        Point touchedPos;
        gameMode currentMode;
        bool touching;
        gameScreens currentScreen,nextScreen;
        bool sfxStateOn = true;
        //default difficulty level
        int level = 2;

        bool settingsClosing = false, pauseClosing= false, continueClosing = false, winnersClosing=false;

        gButton twoPlayerBtn;
        gButton onePlayerBtnGoat;
        gButton onePlayerBtnTiger;
        gButton undoBtn,settingsBtn,sfxOnBtn,sfxOffBtn,helpBtn;
        gButton levelBtn1, levelBtn2, levelBtn3;

        ScrollContainer helpSection;

        SpriteFont goatsCountFont;
        Vector2 goatsRemainTextPos, goatsCapturedTextPos, overlayBG1Pos, overlayBG2Pos, tigersWonTextPos, goatsWonTextPos, pausedTextPos, continueTextPos, gameDrawnTextPos, settingsTextPos, levelTextPos, sfxTextPos, settingsVelocity;

        StarParticleEngine starParticleEngine;
        Vector2 EmitterLoc;

        private SoundEffect effect;
        GameTime prevtime;
        double prevTotSeconds;
        bool pieceTransition=false;
        

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
            //TargetElapsedTime = TimeSpan.FromTicks(333333);

            graphics.SynchronizeWithVerticalRetrace = false;
            //settingsBtn 50 fps
            IsFixedTimeStep = false;
            TargetElapsedTime = TimeSpan.FromMilliseconds(20); 

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
            // initialization logic
            
            initPosition();
            
            
            currentBoardVsGoat = new Board();
            currentBoardVsTiger = new Board();
            currentBoardTwoPlayer = new Board();

            fetchSavedState();
            fetchSavedSettings();

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

            helpSection = new ScrollContainer();

            onePlayerBtnGoat = new gButton(430, 120);
            onePlayerBtnTiger = new gButton(410, 220);
            twoPlayerBtn = new gButton(390,320);
            settingsBtn = new gButton(10,10);
            helpBtn = new gButton(100, 10);


            sfxOnBtn = new gButton(380,175);
            sfxOffBtn = new gButton(380,175);
            undoBtn = new gButton(660, 370);

            //level
            levelBtn1 = new gButton(375, 265);
            levelBtn2 = new gButton(455, 265);
            levelBtn3 = new gButton(535, 265);

            menuBtn = new gButton(280,240);
            newGameBtn = new gButton(440, 240);

            resumeBtn = new gButton(440,240);
            okBtn = new gButton(640,320);
            
            //settings level
            switch (level)
            {
                case 1: levelBtn1.pressed = true; break;
                case 2: levelBtn2.pressed = true; break;
                case 3: levelBtn3.pressed = true; break;
                default: levelBtn1.pressed = true; break;

            }
            
            
            base.Initialize();
        }
        void fetchSavedSettings()
        {
               
            // open isolated storage, and load data from the savefile if it exists.
            #if WINDOWS_PHONE
                        using (IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication())
            #else
                        using (IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain())
            #endif
            {
                if (savegameStorage.FileExists(SAVEFILESETTINGS))
                {
                    using (IsolatedStorageFileStream fs = savegameStorage.OpenFile(SAVEFILESETTINGS, System.IO.FileMode.Open))
                    {
                        if (fs != null){
                            //todo: settings save
                            byte[] saveBytes = new byte[4];
                            int count = 0;
                            count = fs.Read(saveBytes, 0, 4);
                            if (count > 0)
                            {
                                level = System.BitConverter.ToInt32(saveBytes, 0);
                            }

                            count = fs.Read(saveBytes, 0, 4);
                            if (count > 0)
                            {
                                sfxStateOn = System.BitConverter.ToBoolean(saveBytes, 0);
                            }
                        }
                    }
                }
            }

        }

        void saveSettings()
        {
            //todo:
            // Save the game state (in this case, the high score).
            #if WINDOWS_PHONE
                        IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication();
            #else
                        IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain();
            #endif

            // open isolated storage, and write the savefile.
            IsolatedStorageFileStream fs = null;
            using (fs = savegameStorage.CreateFile(SAVEFILESETTINGS))
            {
                if (fs != null)
                {
                    //just overwrite the existing info for this example.

                    byte[] ret = new byte[2 * 4];

                    byte[] bytes1;
                    int offSet = 0;

                    bytes1 = System.BitConverter.GetBytes((int)level);
                    Buffer.BlockCopy(bytes1, 0, ret, offSet, bytes1.Length);
                    offSet += bytes1.Length;

                    bytes1 = System.BitConverter.GetBytes((bool)sfxStateOn);
                    Buffer.BlockCopy(bytes1, 0, ret, offSet, bytes1.Length);
                    offSet += bytes1.Length;

                    fs.Write(ret, 0, ret.Length);

                }
            }
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

        void resetCurrentGame()
        {
            currentBoard = new Board();
            goatsCaptured = currentBoard.mGoatsIntoBoard - getGoatCount();
            gameState.positionslist.Clear();
            gameState.mGoatsIntoBoardList.Clear();
            DeleteCurrentSavedFile();
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
            newMoveDone = true;//to test for winning state
            winner = nodeState.none;
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

            overlayBGtexture = Content.Load<Texture2D>("overlayBG");
            overlayBG1texture = Content.Load<Texture2D>("overlayBG1");
            overlayBG2texture = Content.Load<Texture2D>("overlayBG2");
            tigersWonText = Content.Load<Texture2D>("tigersWon");
            goatsWonText = Content.Load<Texture2D>("goatsWon");
            gameDrawnText = Content.Load<Texture2D>("gameDrawnText");
            pausedText = Content.Load<Texture2D>("pausedText");
            continueText = Content.Load<Texture2D>("continueText");
            settingsText = Content.Load<Texture2D>("settingsText");
            levelText = Content.Load<Texture2D>("levelText");
            SfxText = Content.Load<Texture2D>("SFXText");

            goatsTurnText = Content.Load<Texture2D>("goatsTurn");
            tigersTurnText = Content.Load<Texture2D>("tigersTurn");

            twoPlayerBtn.load("twoPlayerBtnShow", "twoPlayerBtnPressed", Content);
            onePlayerBtnGoat.load("asGoatBtnShow", "asGoatBtnPressed", Content);
            onePlayerBtnTiger.load("asTigerBtnShow", "asTigerBtnPressed", Content);

            helpSection.load(Content);


            settingsBtn.load("settingsBtn", "settingsBtnPressed", Content);
            settingsBtn.setRect(new Rectangle(620, 20, 65, 65));

            helpBtn.load("helpBtn","helpBtnPressed",Content);
            helpBtn.setRect(new Rectangle(700, 20, 65, 65));

            sfxOnBtn.load("sfxBtn", "sfxBtnPressed", Content);
            sfxOffBtn.load("sfxOffBtn", "sfxOffBtnPressed", Content);
            levelBtn1.load("radioBtn", "radioBtnPressed", Content);
            levelBtn2.load("radioBtn", "radioBtnPressed", Content);
            levelBtn3.load("radioBtn", "radioBtnPressed", Content);

            undoBtn.load("undoBtnShow", "undoBtnPressed", Content);

            menuBtn.load("menuBtn", "menuBtnPressed", Content);
            newGameBtn.load("newGameBtn", "newGameBtnPressed", Content);
            resumeBtn.load("resumeBtn", "resumeBtnPressed", Content);
            okBtn.load("OKBtn", "OKBtnPressed", Content);


            //
            goatsRemainTextPos = new Vector2(740, 190);
            goatsCapturedTextPos = new Vector2(740, 310);
            
            goatsCountFont = Content.Load<SpriteFont>("GoatsCount");

            
            screenWidth = graphics.GraphicsDevice.PresentationParameters.BackBufferWidth;
            screenHeight = graphics.GraphicsDevice.PresentationParameters.BackBufferHeight;

            overlayBG1Pos = new Vector2((screenWidth - 385) / 2, (screenHeight - 245) / 2);
            overlayBG2Pos = new Vector2((screenWidth - 564) / 2, (screenHeight - 347) / 2);
            tigersWonTextPos = new Vector2(overlayBG1Pos.X + (385 - 340)/2, (screenHeight - 245) / 2 + 30);
            goatsWonTextPos = new Vector2(overlayBG1Pos.X + (385 - 339) / 2, (screenHeight - 245) / 2 + 30);
            gameDrawnTextPos = new Vector2(overlayBG1Pos.X + (385 - 319) / 2, (screenHeight - 245) / 2 + 30);
            pausedTextPos = new Vector2(overlayBG1Pos.X + (385 - 228) / 2, (screenHeight - 245) / 2 + 30);
            continueTextPos = new Vector2(overlayBG1Pos.X + (385 - 293) / 2, (screenHeight - 245) / 2 + 30);
            settingsTextPos = new Vector2(overlayBG2Pos.X + (564 - 240) / 2, (screenHeight - 370) / 2 + 30);
            
            sfxTextPos = new Vector2(overlayBG2Pos.X + (100 ) / 2, (screenHeight - 160) / 2 + 30);
            levelTextPos = new Vector2(overlayBG2Pos.X + (100) / 2, (screenHeight) / 2 + 35);

            settingsVelocity = new Vector2(0,2000);

            List<Texture2D> textures = new List<Texture2D>();
            textures.Add(Content.Load<Texture2D>("circle"));
            textures.Add(Content.Load<Texture2D>("star"));
            textures.Add(Content.Load<Texture2D>("diamond"));
            starParticleEngine = new StarParticleEngine(textures, new Vector2(400, 240));
            EmitterLoc = Vector2.Zero;

            //sound efx
            effect = Content.Load<SoundEffect>("63531__florian-reinke__click1");

            
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

        void DeleteCurrentSavedFile()
        {
             // Save the game state (in this case, the high score).
            #if WINDOWS_PHONE
                        IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication();
            #else
                        IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain();
            #endif

            if (currentMode == gameMode.vsGoat)
            {
                savegameStorage.DeleteFile(SAVEFILENAMEVSGOAT);
            }
            else if (currentMode == gameMode.vsTiger)
            {
                savegameStorage.DeleteFile(SAVEFILENAMEVSTIGER);
            }
            else if(currentMode == gameMode.twoPlayers)
            {
                savegameStorage.DeleteFile(SAVEFILENAMETWOPLAYERS);
            }
            

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
                if (currentScreen == gameScreens.mainMenuScreen)
                {
                    this.Exit();
                }
                else if (currentScreen == gameScreens.settingsOverlay){
                    settingsClosing = true;
                    //currentScreen = gameScreens.mainMenuScreen;
                }
                else if (currentScreen == gameScreens.gamePlayScreen)
                {
                    resetPause();
                    currentScreen = gameScreens.pauseOverlay;
                }
                else if (currentScreen == gameScreens.pauseOverlay)
                {
                    pauseClosing = true;
                    nextScreen = gameScreens.gamePlayScreen;
                    //currentScreen = gameScreens.gamePlayScreen;
                }
                else if (currentScreen == gameScreens.winnersOverlay)
                {
                    resetCurrentGame();
                    winnersClosing = true;
                    nextScreen = gameScreens.gamePlayScreen;
                }
                else if (currentScreen == gameScreens.continueOverlay)
                {
                    continueClosing = true;
                    nextScreen = gameScreens.mainMenuScreen;
                }
                else if (currentScreen == gameScreens.helpScreen)
                {
                    currentScreen = gameScreens.mainMenuScreen;
                }
            }

            if (currentScreen == gameScreens.mainMenuScreen)
            {
                mainScreenTouchHanlder();
            }
            else if (currentScreen == gameScreens.settingsOverlay)
            {
                settingsOverlayTouchInputHandler(gameTime);
            }
            else if (currentScreen == gameScreens.gamePlayScreen)
            {
                getInputAndUpdateGame(gameTime);

            }
            else if (currentScreen == gameScreens.pauseOverlay)
            {
                pausedOverlayTouchInputHandler(gameTime);
            }
            else if (currentScreen == gameScreens.winnersOverlay)
            {
                winnersOverlayTouchInputHandler(gameTime);
            }
            else if (currentScreen == gameScreens.continueOverlay)
            {
                continueOverlayTouchInputHandler(gameTime);
            }
            else if (currentScreen == gameScreens.helpScreen)
            {
                helpScreenTouchHandler(gameTime);
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
                settingsBtn.handeTouch(touch);
                helpBtn.handeTouch(touch);

            }
            else
            {
                if (twoPlayerBtn.pressed)
                {
                    twoPlayerBtn.pressed = false;
                    currentMode = gameMode.twoPlayers;
                    
                    gameState = gameStateTwoPlayer;
                    currentBoard = currentBoardTwoPlayer;
                    goatsCaptured = currentBoard.mGoatsIntoBoard - getGoatCount();
                    newMoveDone = true;//to check who won
                    switchToGamePlayScreen();

                }
                if (onePlayerBtnGoat.pressed)
                {
                    onePlayerBtnGoat.pressed = false;
                    currentMode = gameMode.vsGoat;
                    
                    System.Diagnostics.Debug.WriteLine("current screen" + currentMode);
                    gameState = gameStateVsGoat;
                    currentBoard = currentBoardVsGoat;
                    goatsCaptured = currentBoard.mGoatsIntoBoard - getGoatCount();
                    newMoveDone = true;//to check who won
                    switchToGamePlayScreen();
                }
                if (onePlayerBtnTiger.pressed)
                {
                    onePlayerBtnTiger.pressed = false;
                    currentMode = gameMode.vsTiger;
                    
                    gameState = gameStateVsTiger;
                    currentBoard = currentBoardVsTiger;
                    goatsCaptured = currentBoard.mGoatsIntoBoard - getGoatCount();
                    newMoveDone = true;//to check who won
                    switchToGamePlayScreen();
                }
                if (settingsBtn.pressed)
                {
                    System.Diagnostics.Debug.WriteLine("settings press");
                    settingsBtn.pressed = false;
                    showSettingsOverlay();
                }
                if(helpBtn.pressed){
                    System.Diagnostics.Debug.WriteLine("help button press");
                    helpBtn.pressed = false;
                    showHelpScreen();
                }

                


            }
               

        }

        void helpScreenTouchHandler(GameTime gameTime)
        {

            helpSection.handleTouch(gameTime);
            
        }

        void resetSettings()
        {
            overlayBG2Pos.Y = -400f;
            settingsTextPos.Y = overlayBG2Pos.Y + 20;
            levelTextPos.Y = overlayBG2Pos.Y + 210;
            sfxTextPos.Y = overlayBG2Pos.Y + 125;
            sfxOnBtn.setRectByPos(sfxOnBtn.X, (int)overlayBG2Pos.Y + 110);
            sfxOffBtn.setRectByPos(sfxOffBtn.X, (int)overlayBG2Pos.Y + 110);

            okBtn.setRectByPos(okBtn.X, (int)overlayBG2Pos.Y + 255);

            levelBtn1.setRectByPos(levelBtn1.X, (int)overlayBG2Pos.Y + 200);
            levelBtn2.setRectByPos(levelBtn2.X, (int)overlayBG2Pos.Y + 200);
            levelBtn3.setRectByPos(levelBtn3.X, (int)overlayBG2Pos.Y + 200);

            settingsClosing = false;
        }

        void updateSettings(GameTime gameTime)
        {
            if (settingsClosing)
            {
                overlayBG2Pos -= settingsVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (overlayBG2Pos.Y <= -400)
                {
                    overlayBG2Pos.Y = -400;
                    currentScreen = gameScreens.mainMenuScreen;

                }
            }
            else
            {
                overlayBG2Pos += settingsVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (overlayBG2Pos.Y >= 66)
                {
                    overlayBG2Pos.Y = 66;
                }
            }
            System.Diagnostics.Debug.WriteLine("move by cpu: " + gameTime.ElapsedGameTime.TotalSeconds);

            settingsTextPos.Y = overlayBG2Pos.Y + 20;
            levelTextPos.Y = overlayBG2Pos.Y + 210;
            sfxTextPos.Y = overlayBG2Pos.Y + 125;
            sfxOnBtn.setRectByPos(sfxOnBtn.X, (int)overlayBG2Pos.Y + 110);
            sfxOffBtn.setRectByPos(sfxOffBtn.X, (int)overlayBG2Pos.Y + 110);

            okBtn.setRectByPos(okBtn.X, (int)overlayBG2Pos.Y + 255);

            levelBtn1.setRectByPos(levelBtn1.X, (int)overlayBG2Pos.Y + 200);
            levelBtn2.setRectByPos(levelBtn2.X, (int)overlayBG2Pos.Y + 200);
            levelBtn3.setRectByPos(levelBtn3.X, (int)overlayBG2Pos.Y + 200);


        }

        void settingsOverlayTouchInputHandler(GameTime gameTime)
        {

            //update settings
            updateSettings(gameTime);

            TouchCollection touches = TouchPanel.GetState();

            if (touches.Count > 0)
            {

                TouchLocation touch = touches.First();

                okBtn.handeTouch(touch);

                //level btn
                levelBtn1.handeTouch(touch);
                levelBtn2.handeTouch(touch);
                levelBtn3.handeTouch(touch);

                if (levelBtn1.pressed)
                {
                    level = 1;
                    levelBtn1.pressed = false;
                }
                if (levelBtn2.pressed)
                {
                    level = 2;
                    levelBtn2.pressed = false;
                }
                if (levelBtn3.pressed)
                {
                    level = 3;
                    levelBtn3.pressed = false;
                }

                switch (level)
                {
                    case 1: levelBtn1.pressed = true;break;
                    case 2: levelBtn2.pressed = true; break;
                    case 3: levelBtn3.pressed = true; break;
                    default: levelBtn1.pressed = true;break;

                }

                if (sfxStateOn)
                {
                    sfxOnBtn.handeTouch(touch);
                }
                else
                {
                    sfxOffBtn.handeTouch(touch);
                }

            }
            else
            {
                if (okBtn.pressed)
                {
                    okBtn.pressed = false;
                    saveSettings();
                    settingsClosing = true;
                    //currentScreen = gameScreens.mainMenuScreen;
                }
                if (sfxOnBtn.pressed)
                {
                    sfxOnBtn.pressed = false;
                    sfxStateOn = false;
                    //toggle sound state
                }
                if (sfxOffBtn.pressed)
                {
                    sfxOffBtn.pressed = false;
                    sfxStateOn = true;
                }

                
            }
        }

        void showSettingsOverlay()
        {
            currentScreen = gameScreens.settingsOverlay;
            resetSettings();
        }

        void showHelpScreen()
        {
            currentScreen = gameScreens.helpScreen;
        }


        void switchToGamePlayScreen()
        {
            if (currentBoard.mGoatsIntoBoard == 0)
            {
                currentScreen = gameScreens.gamePlayScreen;
            }
            else
            {
                resetContinueOverlay();
                currentScreen = gameScreens.continueOverlay;
            }
        }


        void resetWinnersOverlay()
        {
            overlayBG1Pos.Y = -400f;
            tigersWonTextPos.Y = overlayBG1Pos.Y + 20;
            goatsWonTextPos.Y = overlayBG1Pos.Y + 20;
            gameDrawnTextPos.Y = overlayBG1Pos.Y + 20;

            menuBtn.setRectByPos(280, (int)overlayBG1Pos.Y + 110);
            newGameBtn.setRectByPos(440, (int)overlayBG1Pos.Y + 110);

            winnersClosing = false;

        }

        void updateWinnersOverlay(GameTime gameTime)
        {
            if (winnersClosing)
            {
                overlayBG1Pos -= settingsVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (overlayBG1Pos.Y <= -400)
                {
                    overlayBG1Pos.Y = -400;
                    currentScreen = nextScreen;
                }
            }
            else
            {
                overlayBG1Pos += settingsVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (overlayBG1Pos.Y >= 110)
                {
                    overlayBG1Pos.Y = 110;
                }
            }
            System.Diagnostics.Debug.WriteLine("move by cpu: " + gameTime.ElapsedGameTime.TotalSeconds);

            tigersWonTextPos.Y = overlayBG1Pos.Y + 20;
            goatsWonTextPos.Y = overlayBG1Pos.Y + 20;
            gameDrawnTextPos.Y = overlayBG1Pos.Y + 20;

            menuBtn.setRectByPos(280, (int)overlayBG1Pos.Y + 110);
            newGameBtn.setRectByPos(440, (int)overlayBG1Pos.Y + 110);

        }

        void winnersOverlayTouchInputHandler(GameTime gameTime)
        {

            updateWinnersOverlay(gameTime);

            EmitterLoc.X += 30;
            EmitterLoc.Y += 30;
            if (EmitterLoc.X > 800)
            {
                EmitterLoc.X = 0;
            }
            if (EmitterLoc.Y > 480)
            {
                EmitterLoc.Y = 0;
            }
            starParticleEngine.EmitterLocation = EmitterLoc;
            starParticleEngine.Update();


            TouchCollection touches = TouchPanel.GetState();
            
            if (touches.Count > 0)
            {

                TouchLocation touch = touches.First();

                menuBtn.handeTouch(touch);
                newGameBtn.handeTouch(touch);

            }
            else
            {
                if (newGameBtn.pressed)
                {
                    newGameBtn.pressed = false;
                    resetCurrentGame();

                    winnersClosing = true;
                    nextScreen = gameScreens.gamePlayScreen;
                }
                if (menuBtn.pressed)
                {
                    menuBtn.pressed = false;
                    
                    resetCurrentGame();
                    stashCurrentGame();

                    winnersClosing = true;
                    nextScreen = gameScreens.mainMenuScreen;
                }
            }
        }

        void resetContinueOverlay()
        {
            overlayBG1Pos.Y = -400f;
            continueTextPos.Y = overlayBG1Pos.Y + 20;

            newGameBtn.setRectByPos(280, (int)overlayBG1Pos.Y + 110);
            resumeBtn.setRectByPos(440, (int)overlayBG1Pos.Y + 110);

            continueClosing = false;

        }

        void updateContinueOverlay(GameTime gameTime)
        {
            if (continueClosing)
            {
                overlayBG1Pos -= settingsVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (overlayBG1Pos.Y <= -400)
                {
                    overlayBG1Pos.Y = -400;
                    currentScreen = nextScreen;
                }
            }
            else
            {
                overlayBG1Pos += settingsVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (overlayBG1Pos.Y >= 110)
                {
                    overlayBG1Pos.Y = 110;
                }
            }
            System.Diagnostics.Debug.WriteLine("move by cpu: " + gameTime.ElapsedGameTime.TotalSeconds);

            continueTextPos.Y = overlayBG1Pos.Y + 20;

            newGameBtn.setRectByPos(280, (int)overlayBG1Pos.Y + 110);
            resumeBtn.setRectByPos(440, (int)overlayBG1Pos.Y + 110);
            

        }
        void continueOverlayTouchInputHandler(GameTime gameTime)
        {

            updateContinueOverlay(gameTime);

            TouchCollection touches = TouchPanel.GetState();

            if (touches.Count > 0)
            {

                TouchLocation touch = touches.First();

                newGameBtn.handeTouch(touch);
                resumeBtn.handeTouch(touch);

            }
            else
            {
                if (newGameBtn.pressed)
                {
                    newGameBtn.pressed = false;
                    resetCurrentGame();

                    continueClosing = true;
                    nextScreen = gameScreens.gamePlayScreen;
                }
                
                if (resumeBtn.pressed)
                {
                    resumeBtn.pressed = false;

                    continueClosing = true;
                    nextScreen = gameScreens.gamePlayScreen;
                }
            }
        }

        void resetPause(){

            //spriteBatch.Draw(overlayBGtexture, Vector2.Zero, Color.White);
            //spriteBatch.Draw(overlayBG1texture, overlayBG1Pos, Color.White);    
            //spriteBatch.Draw(pausedText, pausedTextPos, Color.White);

            overlayBG1Pos.Y = -400f;
            //menuBtn.setRectByPos(240, 240);
            //newGameBtn.setRectByPos(360, 240);
            //resumeBtn.setRectByPos(480, 240);
            pausedTextPos.Y = overlayBG1Pos.Y + 20;
            menuBtn.setRectByPos(240, (int)overlayBG1Pos.Y + 110);
            newGameBtn.setRectByPos(360, (int)overlayBG1Pos.Y + 110);
            resumeBtn.setRectByPos(480, (int)overlayBG1Pos.Y + 110);

            pauseClosing = false;

        }

        void updatePause(GameTime gameTime)
        {
            if (pauseClosing)
            {
                overlayBG1Pos -= settingsVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (overlayBG1Pos.Y <= -400)
                {
                    overlayBG1Pos.Y = -400;
                    currentScreen = nextScreen;
                }
            }
            else
            {
                overlayBG1Pos += settingsVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (overlayBG1Pos.Y >= 110)
                {
                    overlayBG1Pos.Y = 110;
                }
            }
            System.Diagnostics.Debug.WriteLine("move by cpu: " + gameTime.ElapsedGameTime.TotalSeconds);

            pausedTextPos.Y = overlayBG1Pos.Y + 20;
            menuBtn.setRectByPos(240, (int)overlayBG1Pos.Y + 110);
            newGameBtn.setRectByPos(360, (int)overlayBG1Pos.Y + 110);
            resumeBtn.setRectByPos(480, (int)overlayBG1Pos.Y + 110);


        }

        void pausedOverlayTouchInputHandler(GameTime gameTime)
        {

            //toto
            updatePause(gameTime);

            TouchCollection touches = TouchPanel.GetState();
            
            if ( touches.Count > 0)
            {
                
                TouchLocation touch = touches.First();
                
                menuBtn.handeTouch(touch);
                newGameBtn.handeTouch(touch);
                resumeBtn.handeTouch(touch);

            }
            else
            {
                if (newGameBtn.pressed)
                {
                    newGameBtn.pressed = false;
                    resetCurrentGame();

                    pauseClosing = true;
                    nextScreen = gameScreens.gamePlayScreen;
                }
                if (menuBtn.pressed)
                {
                    menuBtn.pressed = false;
                    
                    stashCurrentGame();

                    pauseClosing = true;
                    nextScreen = gameScreens.mainMenuScreen;
                }
                if (resumeBtn.pressed)
                {
                    resumeBtn.pressed = false;

                    pauseClosing = true;
                    nextScreen = gameScreens.gamePlayScreen;
                    //currentScreen = gameScreens.gamePlayScreen;
                }
            }
        }

        void getInputAndUpdateGame(GameTime gameTime)
        {
            //todo
            if (newMoveDone)
            {
                currentBoard.gameWon = CheckForWin();
                effect.Play();
                newMoveDone = false;
                //if (prevtime != null)
                //{
                //    System.Diagnostics.Debug.WriteLine(prevTotSeconds + "move by cpu prev time " + (gameTime.TotalGameTime.TotalSeconds - prevTotSeconds));
                //    double diff = gameTime.TotalGameTime.TotalSeconds - prevTotSeconds;
                //    if (diff > 0.5)
                //    {
                
                //        currentBoard = nextBoard;
                //        effect.Play();
                //    }
                //}
                //else
                //{
                //    newMoveDone = false;
                //}
                
            }

            if (currentBoard.gameWon)
            {
                currentScreen = gameScreens.winnersOverlay;
                resetWinnersOverlay();
                DeleteCurrentSavedFile();
                return;
            }

            if ((currentMode == gameMode.vsTiger && currentBoard.mTurnForPlayer) || (currentMode == gameMode.vsGoat && !currentBoard.mTurnForPlayer))
            {
                System.Diagnostics.Debug.WriteLine("move by cpu: " + currentBoard.mTurnForPlayer);

                /*
                int movesDepth = 4;
                if (currentBoard.mGoatsIntoBoard > 14)
                {
                    movesDepth = 4;
                }
                */


                Board next = currentBoard.FindNextMove(level+1);
                gameState.positionslist.Add(currentBoard.mValues);
                gameState.mGoatsIntoBoardList.Add(currentBoard.mGoatsIntoBoard);
                //find diff of position
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        if (currentBoard.mValues[i, j] == next.mValues[i, j])
                        {

                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("value: " + next.mValues[i,j] + "chaange i:" + i + "j:" + j);
                        }
                    }
                    
                }
                prevtime = gameTime;
                //prevTotSeconds = gameTime.TotalGameTime.TotalSeconds;
                System.Diagnostics.Debug.WriteLine("value time elapse"+gameTime.ElapsedGameTime.TotalSeconds);
                System.Diagnostics.Debug.WriteLine("value time elapse" + gameTime.TotalGameTime.TotalSeconds);
                //find diff of position
                currentBoard = next;
                //nextBoard = next;
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
                                    System.Diagnostics.Debug.WriteLine("value time elapse" + gameTime.TotalGameTime.TotalSeconds);
                                    prevtime = gameTime;
                                    prevTotSeconds = gameTime.TotalGameTime.TotalSeconds;

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
                                        if (currentBoard.mGoatsIntoBoard == 15 && grid[i, j] == nodeState.goat)
                                        {
                                            
                                            foreach (var move in currentBoard.GetMovesForGoatPuck(currentBoard.mValues, new Point(i, j)))
                                            {
                                                System.Diagnostics.Debug.WriteLine("possible pos:" + move.X + move.Y + "puck" + grid[i, j]);
                                                puckTouched = true;
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
                                            prevTotSeconds = gameTime.TotalGameTime.TotalSeconds;
                                            System.Diagnostics.Debug.WriteLine("value time elapse" + gameTime.TotalGameTime.TotalSeconds);
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
                    goatsCaptured = currentBoard.mGoatsIntoBoard - getGoatCount();
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
            if (currentBoard.mGoatsIntoBoard - getGoatCount() >= 5)
            {
                    winner = nodeState.tiger;
                    return true;
            }
            else if (hasGoatsWon())
            {
                winner = nodeState.goat;
                return true;
            }
            else if (gameDrawn())
            {
                System.Diagnostics.Debug.WriteLine("drawn");
                winner = nodeState.none;
                return true;
            }
            System.Diagnostics.Debug.WriteLine("no draw");
            return false;
            
        }

        bool gameDrawn()
        {
            if (currentBoard.mGoatsIntoBoard<15)
            {
                return false;
            }
            int historyCount = gameState.positionslist.Count-1;
            if (gameState.positionslist.Count >= 12)
            {
                for (int i = historyCount - 4; i >= historyCount - 8; i-=4)
                {
                    if (!ComparePositions(gameState.positionslist[i],gameState.positionslist[historyCount]))
                    {
                        return false;
                    }
                }
                historyCount -= 1;
                for (int i = historyCount - 4; i >= historyCount - 8; i -= 4)
                {
                    if (!ComparePositions(gameState.positionslist[i], gameState.positionslist[historyCount]))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        bool ComparePositions(nodeState[,] list1,nodeState[,] list2)
        {
            
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (list1[i, j] != list2[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

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
            if (currentScreen == gameScreens.mainMenuScreen
                || currentScreen == gameScreens.settingsOverlay)
            {
                DrawMainScreen(gameTime);
            }
            else if (currentScreen == gameScreens.helpScreen)
            {

                DrawHelpScreen(gameTime);
            }
            else if (currentScreen == gameScreens.gamePlayScreen
                || currentScreen == gameScreens.pauseOverlay
                || currentScreen == gameScreens.winnersOverlay
                || currentScreen == gameScreens.continueOverlay)
            {
                DrawBoard();

                DrawPieces();
                DrawGoatsCount();
                DrawPlayerTurn();

                if (currentScreen == gameScreens.winnersOverlay)
                {
                    DrawWonOverlay();
                }
                if (currentScreen == gameScreens.pauseOverlay)
                {
                    DrawPauseOverlay();
                }
                if (currentScreen == gameScreens.continueOverlay)
                {
                    DrawContinueOverlay();
                }

            }
            
            
            spriteBatch.End();
            

            base.Draw(gameTime);
        }
        void DrawMainScreen(GameTime gameTime)
        {
            
            spriteBatch.Draw(mainMenuBackground, Vector2.Zero, Color.White);
            twoPlayerBtn.draw(spriteBatch);
            onePlayerBtnGoat.draw(spriteBatch);
            onePlayerBtnTiger.draw(spriteBatch);
            settingsBtn.draw(spriteBatch);
            helpBtn.draw(spriteBatch);

            if (currentScreen == gameScreens.settingsOverlay)
            {
                //show settings overlay
                DrawSettingsOverlay(gameTime);

            }
            

            

            

        }
        void DrawSettingsOverlay(GameTime gameTime)
        {
            System.Diagnostics.Debug.WriteLine("move by cpu draw seconds: " + gameTime.ElapsedGameTime.TotalSeconds);
            //toto
            //Rectangle screenArea = new Rectangle(0, 0, screenWidth, screenHeight);
            spriteBatch.Draw(overlayBGtexture, Vector2.Zero, Color.White);
            
            spriteBatch.Draw(overlayBG2texture,overlayBG2Pos,Color.White);
            spriteBatch.Draw(settingsText, settingsTextPos, Color.White);
            spriteBatch.Draw(levelText, levelTextPos, Color.White);
            spriteBatch.Draw(SfxText, sfxTextPos, Color.White);

            okBtn.draw(spriteBatch);
            //radio button
            levelBtn1.draw(spriteBatch);
            levelBtn2.draw(spriteBatch);
            levelBtn3.draw(spriteBatch);

            if (sfxStateOn)
            {
                sfxOnBtn.draw(spriteBatch);
            }
            else
            {
                sfxOffBtn.draw(spriteBatch);
            }
           /*
            newGameBtn.setRectByPos(280, 240);
            newGameBtn.draw(spriteBatch);
            resumeBtn.setRectByPos(440, 240);
            resumeBtn.draw(spriteBatch);
            */
        }

        void DrawHelpScreen(GameTime gameTime)
        {
            helpSection.draw(gameTime,spriteBatch);
        }
        void DrawBoard()
        {
            Rectangle screenRectangle = new Rectangle(0, 0, boardtexture.Width,boardtexture.Height);
            spriteBatch.Draw(boardtexture, screenRectangle, Color.White);
            undoBtn.draw(spriteBatch);
        }
        void DrawWonOverlay()
        {
            
            spriteBatch.Draw(overlayBGtexture, Vector2.Zero, Color.White);

            starParticleEngine.Draw(spriteBatch);
            spriteBatch.Draw(overlayBG1texture, overlayBG1Pos, Color.White);
            if ( winner == nodeState.tiger)
            {
                spriteBatch.Draw(tigersWonText, tigersWonTextPos, Color.White);
            }
            else if (winner == nodeState.goat)
            {
                spriteBatch.Draw(goatsWonText, goatsWonTextPos, Color.White);
            }
            else if (winner == nodeState.none)
            {
                spriteBatch.Draw(gameDrawnText, gameDrawnTextPos, Color.White);
            }
            
            menuBtn.draw(spriteBatch);
            newGameBtn.draw(spriteBatch);

            
        }

        void DrawContinueOverlay()
        {
            
            spriteBatch.Draw(overlayBGtexture, Vector2.Zero, Color.White);
            spriteBatch.Draw(overlayBG1texture, overlayBG1Pos, Color.White);

            spriteBatch.Draw(continueText, continueTextPos, Color.White);

            newGameBtn.draw(spriteBatch);
            resumeBtn.draw(spriteBatch);
        }

        void DrawPauseOverlay()
        {
            spriteBatch.Draw(overlayBGtexture, Vector2.Zero, Color.White);
            spriteBatch.Draw(overlayBG1texture, overlayBG1Pos, Color.White);
            
            spriteBatch.Draw(pausedText, pausedTextPos, Color.White);

            menuBtn.draw(spriteBatch);
            newGameBtn.draw(spriteBatch);
            resumeBtn.draw(spriteBatch);

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
