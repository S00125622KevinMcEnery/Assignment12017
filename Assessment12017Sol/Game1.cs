using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sprites;
using System;
using Utilities;

namespace Assessment12017Sol
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SoundEffect collectSound;

        TimeSpan countDown = new TimeSpan(0, 0, 10);
        SpriteFont timeFont;
        SpriteFont playerFont;

        string timeMessage = string.Empty;

        Collectable[] Collectables = new Collectable[10];

        int collectablesAliveCount;

        Vector2 BackCameraPos = Vector2.Zero;
        Player player;
        Vector2 PrevPlayerPosition;
        ChasingEnemy[] chasers = new ChasingEnemy[5];

        Vector2 worldSize = new Vector2(2000, 2000);
        Rectangle worldRect;
        private Texture2D txbackground;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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
            worldRect = new Rectangle(new Point(0, 0), worldSize.ToPoint());

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            txbackground = Content.Load<Texture2D>(@"Textures\background");
            timeFont = Content.Load<SpriteFont>(@"Fonts\ScoreFont");
            playerFont = Content.Load<SpriteFont>(@"Fonts\PlayerFont");
            collectSound = Content.Load<SoundEffect>(@"Audio\2b");

            #region Player Setup

            Texture2D[] txs = new Texture2D[5];
            SoundEffect[] sounds = new SoundEffect[5];
            txs[(int)Player.DIRECTION.LEFT] = Content.Load<Texture2D>(@"Textures\right");
            txs[(int)Player.DIRECTION.RIGHT] = Content.Load<Texture2D>(@"Textures\right");
            txs[(int)Player.DIRECTION.UP] = Content.Load<Texture2D>(@"Textures\up");
            txs[(int)Player.DIRECTION.DOWN] = Content.Load<Texture2D>(@"Textures\down");
            txs[(int)Player.DIRECTION.STANDING] = Content.Load<Texture2D>(@"Textures\stand");

            
            for (int i = 0; i < sounds.Length; i++)
            {
                sounds[i] = Content.Load<SoundEffect>(@"Audio\PlayerDirection\" + i.ToString());
            }


            player = new Player(txs, sounds, new Vector2(0,0), 8, 0, 5);
            player.position = new Vector2(GraphicsDevice.Viewport.Width / 2 - player.SpriteWidth / 2,
                                          GraphicsDevice.Viewport.Height / 2 - player.SpriteHeight / 2);

            PrevPlayerPosition = player.position;

            #endregion Player Setup

            for (int i = 0; i < chasers.Length; i++)
            {
                chasers[i] = new ChasingEnemy(this, Content.Load<Texture2D>(@"Textures\chaser"), 
                    new Vector2(Utility.NextRandom(0, (int)worldSize.X), (Utility.NextRandom(0, (int)worldSize.Y))), 1);
            }
            
            for (int i = 0; i < Collectables.Length; i++)
            {
                Collectables[i] = new Collectable(Content.Load<Texture2D>(@"Textures\spindash"),
                    new Vector2(Utility.NextRandom(0, (int)worldSize.X), (Utility.NextRandom(0, (int)worldSize.Y))), 6);
            }
            collectablesAliveCount = Collectables.Length;
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            #region update player
            player.Update(gameTime);
            player.position = Vector2.Clamp(player.position, Vector2.Zero, (worldSize - new Vector2(player.SpriteWidth, player.SpriteHeight)));
            #endregion
            #region Chasing
            
            for (int i = 0; i < chasers.Length; i++)
                    {
                        if (chasers[i].Alive)
                        {
                                chasers[i].follow(player);
                                if (player.collisionDetect(chasers[i]))
                                {
                                    player.Health -= 10;
                                    chasers[i].Alive = false;
                                }
                                    chasers[i].Update(gameTime);
                         }
                
                    }
            #endregion Chasing Enemy
            #region Update Collectables

            for (int i = 0; i < Collectables.Length; i++)
            {
                Collectables[i].Update(gameTime);
            }

            
            foreach (Collectable item in Collectables)
            {
                if (item.Alive && player.collisionDetect(item))
                {
                    collectablesAliveCount--;
                    player.Score += item.Score;
                    item.Alive = false;
                    collectSound.Play();
                }
                item.Update(gameTime);
            }
            #endregion Collectables
            #region Camera Control

            

            // if the player moves the camera follows but only if the player is past his starting position which is 
            // in the Center of th viewport
            if (player.position != PrevPlayerPosition)
            {
                
                if(player.position.X > GraphicsDevice.Viewport.Width/2 && 
                    player.position.X < worldSize.X - GraphicsDevice.Viewport.Width / 2)
                        BackCameraPos.X +=  player.position.X - PrevPlayerPosition.X;

                if (player.position.Y > GraphicsDevice.Viewport.Height/2 &&
                    player.position.Y < worldSize.Y - GraphicsDevice.Viewport.Height/ 2)
                            BackCameraPos.Y += player.position.Y - PrevPlayerPosition.Y;
            }

            BackCameraPos = Vector2.Clamp(BackCameraPos,
                    Vector2.Zero,
                    new Vector2(worldSize.X - GraphicsDevice.Viewport.Width,
                                worldSize.Y - GraphicsDevice.Viewport.Height));
            
            // possible change in player position
            PrevPlayerPosition = player.position;
            #endregion
            #region update timer
            int secs;
            if ((secs = (int)(countDown - gameTime.TotalGameTime).TotalSeconds) > 0)
            {
                timeMessage = "Time to Go " + secs.ToString();
            }
            else
            { 
                // Game Over state. Show the scoreboard 

            }
            #endregion

            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Matrix BackgroundTransform = Matrix.Identity * Matrix.CreateTranslation(-BackCameraPos.X, -BackCameraPos.Y, 0);
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, BackgroundTransform);
            spriteBatch.Draw(txbackground, worldRect, Color.White);
            player.Draw(spriteBatch);

            for (int i = 0; i < chasers.Length; i++)
                chasers[i].Draw(spriteBatch);

            for (int i = 0; i < Collectables.Length; i++)
                Collectables[i].Draw(spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin();
            spriteBatch.DrawString(timeFont, timeMessage, new Vector2(10, 10), Color.White);
            spriteBatch.DrawString(playerFont, "Player Health : " + player.Health.ToString(), new Vector2(10, 30), Color.White);
            spriteBatch.DrawString(playerFont, "Player Score : " + player.Score.ToString(), new Vector2(10, 50), Color.White);
            spriteBatch.DrawString(playerFont, "Collectables remaining: " + collectablesAliveCount.ToString(), new Vector2(10, 70), Color.White);
            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
