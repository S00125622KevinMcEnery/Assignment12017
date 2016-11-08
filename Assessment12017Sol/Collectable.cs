using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Utilities;

namespace Sprites
{
    class Collectable : AnimatedSprite
    {

        public int Score = 0;

        public Collectable(Texture2D texture, Vector2 userPosition, int framecount) : base(texture, userPosition, framecount)
        {
            Score = Utility.NextRandom(20, 50);
        }

        


    }
}
