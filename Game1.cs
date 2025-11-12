using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace erstes_MonoGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont scoreFont;
        private int score = 0;
        private Texture2D buttonTexture;
        private Rectangle buttonRect = new Rectangle(10, 50, 150, 40);
        private MouseState previousMouseState;

        // Modelle
        private Model monkeyModel;
        private Model bananaModel;
        private Vector3 monkeyPosition = new(0, 0, 0);
        private float monkeyRotationY = 0f;
        private List<Vector3> bananaPositions = new();

        // Kamera
        private Matrix viewMatrix;
        private Matrix projectionMatrix;
        private Vector3 cameraPosition = new(0, 20, 30);
        private Vector3 cameraTarget = new(0, 0, 0);

        // Hintergrund
        private Color backgroundColor = Color.CornflowerBlue;

        // Timer & Game Over
        private double remainingTime = 60;
        private bool isGameOver = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
           // _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(60f),
                _graphics.GraphicsDevice.Viewport.AspectRatio,
                0.1f, 100f);

            GenerateBananaPositions();
            base.Initialize();
        }

        private void GenerateBananaPositions()
        {
            bananaPositions.Clear();
            Random rand = new Random();
            for (int i = 0; i < 10; i++)
            {
                float x = rand.Next(-10, 11);
                float z = rand.Next(-10, 11);
                bananaPositions.Add(new Vector3(x, 0.5f, z));
            }
        }

        private void ResetGame()
        {
            score = 0;
            remainingTime = 60;
            isGameOver = false;
            monkeyPosition = new Vector3(0, 0, 0);
            monkeyRotationY = 0f;
            GenerateBananaPositions();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            monkeyModel = Content.Load<Model>("monkey");
            bananaModel = Content.Load<Model>("banana");
            scoreFont = Content.Load<SpriteFont>("ScoreFont");
            buttonTexture = Content.Load<Texture2D>("button");
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.Escape))
                Exit();

            if (isGameOver)
            {
                if (keyState.IsKeyDown(Keys.Enter))
                    ResetGame();
                return;
            }

            remainingTime -= gameTime.ElapsedGameTime.TotalSeconds;
            if (remainingTime <= 0)
            {
                remainingTime = 0;
                isGameOver = true;
            }

            // Bewegung X/Z
            if (keyState.IsKeyDown(Keys.Left))
                monkeyPosition.X -= 0.1f;
            if (keyState.IsKeyDown(Keys.Right))
                monkeyPosition.X += 0.1f;
            if (keyState.IsKeyDown(Keys.Up))
                monkeyPosition.Z -= 0.1f;
            if (keyState.IsKeyDown(Keys.Down))
                monkeyPosition.Z += 0.1f;

            // Y-Achse
            if (keyState.IsKeyDown(Keys.W))
                monkeyPosition.Y += 0.1f;
            if (keyState.IsKeyDown(Keys.S))
                monkeyPosition.Y -= 0.1f;

            // Rotation
            if (keyState.IsKeyDown(Keys.A))
                monkeyRotationY -= 0.05f;
            if (keyState.IsKeyDown(Keys.D))
                monkeyRotationY += 0.05f;

            // Farbe wechseln
            if (keyState.IsKeyDown(Keys.Space))
                backgroundColor = backgroundColor == Color.CornflowerBlue ? Color.Yellow : Color.CornflowerBlue;

            // Bananen einsammeln
            for (int i = bananaPositions.Count - 1; i >= 0; i--)
            {
                if (Vector3.Distance(monkeyPosition, bananaPositions[i]) < 1f)
                {
                    bananaPositions.RemoveAt(i);
                    score++;
                    if (score >= 10)
                        isGameOver = true;
                }
            }

            MouseState mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed &&
                previousMouseState.LeftButton == ButtonState.Released &&
                buttonRect.Contains(mouse.Position))
            {
                backgroundColor = backgroundColor == Color.CornflowerBlue ? Color.Yellow : Color.CornflowerBlue;
            }

            previousMouseState = mouse;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(backgroundColor);

            DrawMonkey();
            DrawBananas();

            _spriteBatch.Begin();

            _spriteBatch.DrawString(scoreFont, $"Score: {score}", new Vector2(10, 10), Color.Black);

            TimeSpan time = TimeSpan.FromSeconds(remainingTime);
            string timeText = $"Time: {time.Minutes:D2}:{time.Seconds:D2}";
            _spriteBatch.DrawString(scoreFont, timeText, new Vector2(10, 100), Color.Black);

            if (isGameOver)
            {
                string gameOverText = "Game Over\nPress Enter to Restart";
                Vector2 size = scoreFont.MeasureString(gameOverText);
                Vector2 position = new Vector2(
                    (_graphics.PreferredBackBufferWidth - size.X) / 2,
                    (_graphics.PreferredBackBufferHeight - size.Y) / 2
                );
                _spriteBatch.DrawString(scoreFont, gameOverText, position, Color.Red);
            }
            
            _spriteBatch.Draw(buttonTexture, buttonRect, Color.White);
            
            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawMonkey()
        {
            foreach (var mesh in monkeyModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;
                    effect.World =
                        Matrix.CreateScale(0.03f) *
                        Matrix.CreateRotationY(monkeyRotationY) *
                        Matrix.CreateTranslation(monkeyPosition);
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }
        }

        private void DrawBananas()
        {
            foreach (var pos in bananaPositions)
            {
                foreach (var mesh in bananaModel.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.View = viewMatrix;
                        effect.Projection = projectionMatrix;
                        effect.World = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(pos);
                        effect.EnableDefaultLighting();
                    }
                    mesh.Draw();
                }
            }
        }
    }
}
