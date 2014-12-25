using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    class gButton
    {
        public bool pressed;
        Texture2D textureShow,texturePressed;
        Rectangle rect;
        Vector2 pos;
        int X;
        int Y;

        public gButton(int X,int Y)
        {
            setPos(X, Y);
        }
        
        public void load(String resShow, String resPressed,ContentManager Content)
        {
            textureShow = Content.Load<Texture2D>(resShow);
            texturePressed = Content.Load<Texture2D>(resPressed);
            this.rect = new Rectangle(X, Y, textureShow.Width, textureShow.Height);
        }
        public void handeTouch( TouchLocation touch )
        {
           
            if (this.rect.Contains((int)touch.Position.X, (int)touch.Position.Y))
            {
                pressed = true;
            }
            else
            {
                pressed = false;
            }
        }
        public void setRect(Rectangle rect)
        {
            this.rect = rect;
        }
        public void setPos(int X,int Y)
        {
            this.X = X;
            this.Y = Y;
            
        }
        public void setRectByPos(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
            this.rect = new Rectangle(X, Y, textureShow.Width, textureShow.Height);
        }

        public void draw(SpriteBatch spriteBatch)
        {
            
            if (pressed)
            {
                spriteBatch.Draw(texturePressed, rect, Color.White);
            }
            else
            {
                spriteBatch.Draw(textureShow, rect, Color.White);
            }
        }
    }
}
