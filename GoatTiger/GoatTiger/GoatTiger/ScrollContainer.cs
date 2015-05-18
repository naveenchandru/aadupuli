using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;


namespace GoatTiger
{
    class ScrollContainer
    {

        gButton leftArrow, rightArrow;
        List<Texture2D> screens;
        int currentScreen;
        Vector2 currentScreenPos, preScreenPos, nextScreenPos, prepreScreenPos,nextnextScreenPos;
        Texture2D helpborder;
        bool transitionStarted = false, rightTransitionStarted=false;
        GameTime gametime;
        const int pageWidth = 720;
        //todo: this class accepts screens and provides a scrolling content
        public ScrollContainer()
        {
            this.leftArrow = new gButton(10,200);
            this.rightArrow = new gButton(713, 200);
            this.screens = new List<Texture2D>();
            currentScreenPos = new Vector2(40,0);
            preScreenPos = new Vector2(currentScreenPos.X - pageWidth, 0);
            nextScreenPos = new Vector2(currentScreenPos.X + pageWidth, 0);
            prepreScreenPos = new Vector2(currentScreenPos.X - 2*pageWidth, 0);
            nextnextScreenPos = new Vector2(currentScreenPos.X + 2*pageWidth, 0);
            currentScreen = 0;
        }
        public void load(ContentManager Content)
        {
            rightArrow.load("nextBtn", "nextBtnPressed", Content);
            //rightArrow.setRect(new Rectangle(800-60-10, 200, 60, 80));
            leftArrow.load("prevBtn", "prevBtnPressed", Content);
            //leftArrow.setRect(new Rectangle(10, 200, 60, 80));
            screens.Add(Content.Load<Texture2D>("slide1"));
            screens.Add(Content.Load<Texture2D>("slide2"));
            screens.Add(Content.Load<Texture2D>("slide3"));
            screens.Add(Content.Load<Texture2D>("slide4"));
            screens.Add(Content.Load<Texture2D>("slide5"));
            screens.Add(Content.Load<Texture2D>("slide6"));
            
            //screens.Add(Content.Load<Texture2D>("slide2"));
            //screens.Add(Content.Load<Texture2D>("slide3"));
            //screens.Add(Content.Load<Texture2D>("slide1")); 
            //screens.Add(Content.Load<Texture2D>("slide2"));
            //screens.Add(Content.Load<Texture2D>("slide3"));
            //screens.Add(Content.Load<Texture2D>("slide1"));

            helpborder = Content.Load<Texture2D>("borderhelp");
        }
        public void draw(GameTime gameTime,SpriteBatch spriteBatch)
        {
            
            Rectangle screenArea = new Rectangle(0, 0, 800,480);
            //spriteBatch.Draw(screens[currentScreen], screenArea, Color.White);
            spriteBatch.Draw(screens[currentScreen], currentScreenPos, Color.White);
            if (currentScreen>0)
            {
            spriteBatch.Draw(screens[currentScreen-1], preScreenPos, Color.White);
            }
            if (currentScreen-1 > 0)
            {
                spriteBatch.Draw(screens[currentScreen - 2], prepreScreenPos, Color.White);
            }
            if (currentScreen+1 < screens.Count)
            {
                spriteBatch.Draw(screens[currentScreen+1], nextScreenPos, Color.White);
            }
            if (currentScreen + 2 < screens.Count)
            {
                spriteBatch.Draw(screens[currentScreen + 2], nextnextScreenPos, Color.White);
            }
            //slideIn(spriteBatch);

            spriteBatch.Draw(helpborder, Vector2.Zero, Color.White);

            if (currentScreen + 1 < screens.Count)
            {
                rightArrow.draw(spriteBatch);
            }
            if (currentScreen > 0)
            {
                leftArrow.draw(spriteBatch);
            }

            
            

            System.Diagnostics.Debug.WriteLine("lapsed::" + gameTime.ElapsedGameTime);

        }
        void slideIn(SpriteBatch spriteBatch)
        {
           // GameTime gametime = new GameTime();
            System.Diagnostics.Debug.WriteLine("lapsed::" + gametime.ElapsedGameTime);
            
        }
        public void setScreens()
        {
            
        }
        public void handleTouch(GameTime gameTime)
        {
            TouchCollection touches = TouchPanel.GetState();

            if (touches.Count > 0)
            {
                TouchLocation touch = touches.First();

                if (currentScreen+1 < screens.Count)
                {
                    rightArrow.handeTouch(touch);
                }
                if (currentScreen > 0){
                    leftArrow.handeTouch(touch);
                }
                
            }
            else
            {
                

                if (rightArrow.pressed)
                {
                    currentScreen++;
                    if (currentScreen > screens.Count - 1)
                    {
                        currentScreen = screens.Count - 1;
                    }

                    rightArrow.pressed = false;
                    currentScreenPos.X = 800-40;
                    preScreenPos.X = currentScreenPos.X - pageWidth;
                    nextScreenPos.X = currentScreenPos.X + pageWidth;
                    prepreScreenPos.X = currentScreenPos.X - 2 * pageWidth;
                    nextnextScreenPos.X = currentScreenPos.X + 2*pageWidth;
                    
                    
                    System.Diagnostics.Debug.WriteLine("leftscroll" + currentScreen);
                    transitionStarted = true;
                    
                }
                if (leftArrow.pressed)
                {
                    currentScreen--;
                    if (currentScreen < 0)
                    {
                        currentScreen = 0;
                    }
                    
                    currentScreenPos.X = 40-800;
                    preScreenPos.X = currentScreenPos.X - pageWidth;
                    nextScreenPos.X = currentScreenPos.X + pageWidth;
                    prepreScreenPos.X = currentScreenPos.X - 2 * pageWidth;
                    nextnextScreenPos.X = currentScreenPos.X + 2 * pageWidth;

                    System.Diagnostics.Debug.WriteLine("leftscroll" + currentScreen);
                    rightTransitionStarted = true;

                    leftArrow.pressed = false;
                    System.Diagnostics.Debug.WriteLine("right scroll" + currentScreen);
                }
            }

            if (transitionStarted)
            {
                currentScreenPos.X -= (float)gameTime.ElapsedGameTime.TotalSeconds * 2000f;
                if (currentScreenPos.X <= 40)
                {
                    currentScreenPos.X = 40;
                    transitionStarted = false;
                }
                preScreenPos.X = currentScreenPos.X - pageWidth;
                nextScreenPos.X = currentScreenPos.X + pageWidth;
                prepreScreenPos.X = currentScreenPos.X - 2 * pageWidth;
                nextnextScreenPos.X = currentScreenPos.X + 2 * pageWidth;
            }
            if (rightTransitionStarted)
            {
                currentScreenPos.X += (float)gameTime.ElapsedGameTime.TotalSeconds * 2000f;
                if (currentScreenPos.X >= 40)
                {
                    currentScreenPos.X = 40;
                    rightTransitionStarted = false;
                }
                preScreenPos.X = currentScreenPos.X - pageWidth;
                nextScreenPos.X = currentScreenPos.X + pageWidth;
                prepreScreenPos.X = currentScreenPos.X - 2 * pageWidth;
                nextnextScreenPos.X = currentScreenPos.X + 2 * pageWidth;
            }
            

        }
    }
}
