using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProyectoFinalCobito
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        GamePadState lastState = GamePad.GetState(PlayerIndex.One);

        GamePadState currentState;

        //Informacion de la camara
        Vector3 camPos = new Vector3(0.0f, 0.0f, GameConstants.CamHeight);
        Matrix projectionMatrix;
        Matrix viewMatrix;

        //Componentes de audio
        SoundEffect sEngine;
        SoundEffectInstance sEngineIns;
        SoundEffect sHyperSpaceActivation;

        SoundEffect sExplo2;
        SoundEffect sExplo3;
        SoundEffect sWeapFire;

        Model asteroidModel;
        Model bulletModel;
        Model shipModel;
        Matrix[] asteroidTransforms;
        Matrix[] bulletTransforms;
        Matrix[] shipTransforms;

        //Componentes Visuales
        Nave ship = new Nave();

        Model astMod;
        Matrix[] astTras;
        Asteroid[] astList = new Asteroid[GameConstants.NumAsteroids];
        Random random = new Random();

        Model bullMod;
        Matrix[] bullTras;
        Bullet[] bullList = new Bullet[GameConstants.NumBull];

        Texture2D stars;

        SpriteFont font;
        int score = 0;
        Vector2 scrPos = new Vector2(10, 10);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                GraphicsDevice.DisplayMode.AspectRatio,
                GameConstants.CamHeight - 1000.0f,
                GameConstants.CamHeight + 1000.0f);

            viewMatrix = Matrix.CreateLookAt(camPos, Vector3.Zero, Vector3.Up);

            ResetAsteroids();

            base.Initialize();
        }

        private Matrix[] SetupEffectDefaults(Model myModel)
        {
            Matrix[] absTra = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(absTra);

            foreach (ModelMesh mesh in myModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Projection = projectionMatrix;
                    effect.View = viewMatrix;
                }
            }
            return absTra;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Nave
            shipModel = Content.Load<Model>("Models/p1_wedge");
            shipTransforms = SetupEffectDefaults(shipModel);

            sEngine = Content.Load<SoundEffect>("Audio/Waves/engine_2");
            sEngineIns = sEngine.CreateInstance();
            sHyperSpaceActivation = Content.Load<SoundEffect>("Audio/Waves/hyperspace_activate");

            //yeah
            //yeah = Content.Load<Song>("Audio/Waves/yeah");
            //MediaPlayer.Play(yeah);

            //Asteroide
            astMod = Content.Load<Model>("Models/asteroid1");
            astTras = SetupEffectDefaults(astMod);

            //Balas
            bullMod = Content.Load<Model>("Models/pea_proj");
            bullTras = SetupEffectDefaults(bullMod);

            sExplo2 = Content.Load<SoundEffect>("Audio/Waves/explosion2");
            sExplo3 = Content.Load<SoundEffect>("Audio/Waves/explosion3");
            sWeapFire = Content.Load<SoundEffect>("Audio/Waves/tx0_fire1");

            //Fondo
            stars = Content.Load<Texture2D>("Textures/B1_stars");

            //Fuente
            font = Content.Load<SpriteFont>("Fonts/Lucida Console");
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // Input
            UpdateInput();

            //A?ade velocidad a la posicion 
            ship.Position += ship.Velocity;

            //Reduce velocidad con tiempo
            ship.Velocity *= 0.95f;

            //Update asteroid
            for (int i = 0; i < GameConstants.NumAsteroids; i++)
            {
                astList[i].Update(timeDelta);
            }

            for (int i = 0; i < GameConstants.NumBull; i++)
            {
                if (bullList[i].isAct)
                {
                    bullList[i].Update(timeDelta);
                }
            }

            //Colision bala asteroide
            if (CheckBullAsCol(bullMod.Meshes[0].BoundingSphere.Radius,
                astMod.Meshes[0].BoundingSphere.Radius))
            {
                sExplo2.Play();
            }

            //Colison nave asteroide
            bool shipDes = CheckShipAstCol(shipModel.Meshes[0].BoundingSphere.Radius,
                astMod.Meshes[0].BoundingSphere.Radius);
            if (shipDes)
            {
                sExplo3.Play();
            }

            base.Update(gameTime);
        }

        protected void UpdateInput()
        {
            //Obtener estado de GamePad
            GamePadState currState = GamePad.GetState(PlayerIndex.One);
            if (currState.IsConnected)
            {
                if (ship.isAct)
                {
                    ship.Update(currState);
                    PlayEngineSound(currState);
                }

                GamePad.SetVibration(PlayerIndex.One,
                currState.Triggers.Right,
                currState.Triggers.Right);


                //Teletransporta la nave al centro
                if (currState.Buttons.B == ButtonState.Pressed && lastState.Buttons.B == ButtonState.Released)
                {
                    WarptoCenter();
                    sHyperSpaceActivation.Play();
                }

                //Dispara
                if (ship.isAct && IsButtonPressed(Buttons.A))
                {
                    ShootBullet();
                    sWeapFire.Play();
                    bool isFiring = true;
                }

                lastState = currState;
            }
        }

        private void ResetAsteroids()
        {
            float xStart;
            float yStart;
            for (int i = 0; i < GameConstants.NumAsteroids; i++)
            {
                astList[i].isAct = true;
                if (random.Next(2) == 0)
                {
                    xStart = (float)-GameConstants.PlayfieldSizeX;
                }
                else
                {
                    xStart = (float)GameConstants.PlayfieldSizeX;
                }
                yStart = (float)random.NextDouble() * GameConstants.PlayfieldSizeY;
                astList[i].position = new Vector3(xStart, yStart, 0.0f);
                double angle = random.NextDouble() * 2 * Math.PI;
                astList[i].direction.X = -(float)Math.Sin(angle);
                astList[i].direction.Y = (float)Math.Cos(angle);
                astList[i].speed = GameConstants.AsteroidMinSpeed +
                (float)random.NextDouble() * GameConstants.AsteroidMaxSpeed;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            spriteBatch.Draw(stars, new Rectangle(0, 0, 800, 600), Color.White);
            spriteBatch.End();

            if (ship.isAct)
            {
                Matrix shipTransformMatriz = ship.RotationMatrix * Matrix.CreateTranslation(ship.Position);
                DrawModel(ship.Modelo, shipTransformMatriz, ship.Transforms);
            }

            for (int i = 0; i < GameConstants.NumAsteroids; i++)
            {
                if (astList[i].isAct)
                {
                    Matrix astTrans = Matrix.CreateTranslation(astList[i].position);
                    DrawModel(astMod, astTrans, astTras);
                }
            }

            for (int i = 0; i < GameConstants.NumBull; i++)
            {
                if (bullList[i].isAct)
                {
                    Matrix bullTrans = Matrix.CreateTranslation(bullList[i].position);
                    DrawModel(bullMod, bullTrans, bullTras);
                }
            }
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatch.DrawString(font, "Score: " + score, scrPos, Color.LightGreen);
            spriteBatch.End();



            base.Draw(gameTime);
        }

        bool CheckBullAsCol(float bRadio, float aRadio)
        {
            for (int i = 0; i < astList.Length; i++)
            {
                if (astList[i].isAct)
                {
                    BoundingSphere astSph = new BoundingSphere(astList[i].position,
                        aRadio *
                        GameConstants.AsteroidBoundingSphereScale);
                    for (int j = 0; j < bullList.Length; j++)
                    {
                        if (bullList[j].isAct)
                        {
                            BoundingSphere bullSph = new BoundingSphere(bullList[j].position,
                                bRadio);
                            if (astSph.Intersects(bullSph))
                            {
                                sExplo2.Play();
                                astList[i].isAct = false;
                                bullList[j].isAct = false;
                                score += GameConstants.KillBonus;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public bool CheckShipAstCol(float sRadio, float aRadio)
        {
            if (ship.isAct)
            {
                BoundingSphere shSphere = new BoundingSphere(ship.Position,
                sRadio *
                GameConstants.ShipBoundingSphereScale);
                for (int i = 0; i < astList.Length; i++)
                {
                    BoundingSphere b = new BoundingSphere(astList[i].position,
                        aRadio *
                        GameConstants.AsteroidBoundingSphereScale);
                    if (b.Intersects(shSphere))
                    {
                        //Boom boom
                        sExplo3.Play();
                        ship.isAct = false;
                        astList[i].isAct = false;
                        score -= GameConstants.DeathPenalty;
                        return true;
                    }
                }
            }
            return false;
        }

        public void ShootBullet()
        {
            //a?adir balla en slot de bala incativo
            //No se a?adira ballas si todos los espacios estan en uso
            for (int i = 0; i < GameConstants.NumBull; i++)
            {
                if (!bullList[i].isAct)
                {
                    bullList[i].direction = ship.RotationMatrix.Forward;
                    bullList[i].speed = GameConstants.BullSpeedAdj;
                    bullList[i].position = ship.Position
                        + (200 * bullList[i].direction);
                    bullList[i].isAct = true;
                    score -= GameConstants.ShotPenalty;
                    return;
                }
            }
        }

        public void WarptoCenter()
        {
            ship.Position = Vector3.Zero;
            ship.Velocity = Vector3.Zero;
            ship.Rotation = 0.0f;
            ship.isAct = true;
        }

        void PlayEngineSound(GamePadState currState)
        {
            //Reproduce sonido de motor cuando se enciende
            if (currState.Triggers.Right > 0)
            {
                if (sEngineIns.State == SoundState.Stopped)
                {
                    sEngineIns.Volume = 0.75f;
                    sEngineIns.IsLooped = true;
                    sEngineIns.Play();
                }
                else
                    sEngineIns.Resume();
            }
            else if (currState.Triggers.Right == 0)
            {
                if (sEngineIns.State == SoundState.Playing)
                    sEngineIns.Pause();
            }
        }

        bool IsButtonPressed(Buttons button)
        {
            switch (button)
            {
                case Buttons.A:
                    return (currentState.Buttons.A == ButtonState.Pressed && lastState.Buttons.A == ButtonState.Released);
                case Buttons.B:
                    return (currentState.Buttons.B == ButtonState.Pressed && lastState.Buttons.B == ButtonState.Released);
                case Buttons.X:
                    return (currentState.Buttons.X == ButtonState.Pressed && lastState.Buttons.X == ButtonState.Released);
                case Buttons.Back:
                    return (currentState.Buttons.Back == ButtonState.Pressed && lastState.Buttons.Back == ButtonState.Released);
                case Buttons.DPadUp:
                    return (currentState.DPad.Up == ButtonState.Pressed && lastState.DPad.Up == ButtonState.Released);
                case Buttons.DPadDown:
                    return (currentState.DPad.Down == ButtonState.Pressed && lastState.DPad.Down == ButtonState.Released);
            }
            return false;
        }

        public static void DrawModel(Model model, Matrix modelTra, Matrix[] absBonTra)
        {
            //Dibuja el modelo en ciclo en caso de multiples meshes
            foreach (ModelMesh mesh in model.Meshes)
            {
                //Orientacion del mesh
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = absBonTra[mesh.ParentBone.Index] * modelTra;
                }
                //Dibuja mesh apartir de los efectos anteriores
                mesh.Draw();
            }
        }

    }
}
