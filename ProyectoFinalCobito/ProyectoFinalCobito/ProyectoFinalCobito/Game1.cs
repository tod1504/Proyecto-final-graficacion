using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
//ejemplo para subida
namespace ProyectoFinalCobito
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //Player player = new Player();

        Viewport rightViewport;
        Viewport leftViewport;
        Viewport mainViewport;


        GamePadState lastState;

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

        //Model asteroidModel;
        //Model bulletModel;
        Model shipModel;
        //Matrix[] asteroidTransforms;
        //Matrix[] bulletTransforms;
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

        float aspectRatio;


        NetworkSession networkSession;
        AvailableNetworkSessionCollection availableSessions;
        int selectedSessionIndex;
        PacketReader packetReader = new PacketReader();
        PacketWriter packetWriter = new PacketWriter();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //Split screen
            aspectRatio = (float)GraphicsDeviceManager.DefaultBackBufferWidth /
                (2 * GraphicsDeviceManager.DefaultBackBufferHeight);

            //Add Gamer Services
            Components.Add(new GamerServicesComponent(this));

            //Respond to the SignedInGamer event
            SignedInGamer.SignedIn += new EventHandler<SignedInEventArgs>(SignedInGamer_SignedIn);
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


            //Puertos de vista
            mainViewport = GraphicsDevice.Viewport;
            leftViewport = mainViewport;
            rightViewport = mainViewport;
            leftViewport.Width = leftViewport.Width / 2;
            rightViewport.Width = rightViewport.Width / 2;
            rightViewport.X = leftViewport.Width + 1;
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {

            if (!Guide.IsVisible)
            {
                foreach (SignedInGamer signedInGamer in SignedInGamer.SignedInGamers)
                {
                    Player player = signedInGamer.Tag as Player; lastState = player.lastState;
                    currentState = GamePad.GetState(signedInGamer.PlayerIndex);

                    if (networkSession != null)
                    {
                        if (networkSession.SessionState == NetworkSessionState.Lobby) HandleLobbyInput();
                        else
                            HandleGameplayInput(player, gameTime);
                    }
                    else if (availableSessions != null)
                    {
                        HandleAvailableSessionsInput();
                    }
                    else
                    {
                        HandleTitleScreenInput();
                    }
                    player.lastState = currentState;
                }
            }
            base.Update(gameTime);
        }

        private void DrawLobby()
        {
            GraphicsDevice.Clear(Color.CornflowerBlue); spriteBatch.Begin();
            float y = 100;

            spriteBatch.DrawString(font, "Lobby (A=ready, B=leave)", new Vector2(101, y + 1), Color.Black);
            spriteBatch.DrawString(font, "Lobby (A=ready, B=leave)", new Vector2(101, y), Color.White);

            y += font.LineSpacing * 2;

            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                string text = gamer.Gamertag; Player player = gamer.Tag as Player;
                if (player.picture == null)
                {
                    GamerProfile gamerProfile = gamer.GetProfile();
                    player.picture = gamerProfile.GamerPicture;
                }
                if (gamer.IsReady)
                    text += " - ready!";

                spriteBatch.Draw(player.picture, new Vector2(100, y), Color.White);
                spriteBatch.DrawString(font, text, new Vector2(170, y), Color.White);

                y += font.LineSpacing + 64;
            }
            spriteBatch.End();

        }
        protected void HandleLobbyInput()
        {
            // Signal I'm ready to play! 
            if (IsButtonPressed(Buttons.A))
            {
                foreach (LocalNetworkGamer gamer in networkSession.LocalGamers) gamer.IsReady = true;
            }

            if (IsButtonPressed(Buttons.B))
            {
                networkSession = null; availableSessions = null;
            }

            // The host checks if everyone is ready, and moves
            // to game play if true. 
            if (networkSession.IsHost)
            {
                if (networkSession.IsEveryoneReady) networkSession.StartGame();
            }

            // Pump the underlying session object. 
            networkSession.Update();

        }

        protected void UpdateInput(Player player, GameTime gameTime)
        {
            bool isFiring = false; 
            bool shipDestroyed = false;

            foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
            {
                ReceiveNetworkData(gamer, gameTime);

                // this code is the same code we have been
                // using to update the player input 
                if (currentState.IsConnected)
                {

                    if (player.ship.isAct)
                    {
                        player.ship.Update(currentState); PlayEngineSound(currentState);
                    }
                    // In case you get lost, press B to warp back to the center. 
                    if (IsButtonPressed(Buttons.B))
                    {
                        player.WarpToCenter();
                        // Make a sound when we warp. 
                        sHyperSpaceActivation.Play();
                    }

                    //are we shooting?
                    if (player.ship.isAct && IsButtonPressed(Buttons.A))
                    {
                        player.ShootBullet(); 
                        sWeapFire.Play(); 
                        isFiring = true;
                    }

                    if (player.CheckForBulletAsteroidCollision(bullMod.Meshes[0].BoundingSphere.Radius, astMod.Meshes[0].BoundingSphere.Radius))
                    {
                        sExplo2.Play();
                    }

                    shipDestroyed = player.CheckForShipAsteroidCollision(shipModel.Meshes[0].BoundingSphere.Radius, astMod.Meshes[0].BoundingSphere.Radius);

                    if (shipDestroyed)
                    {
                        sExplo3.Play();
                    }
                }
                packetWriter.Write(player.ship.isAct);
                packetWriter.Write(player.ship.Position);
                packetWriter.Write(player.ship.Rotation);
                packetWriter.Write(player.score);
                packetWriter.Write(isFiring);
                packetWriter.Write(shipDestroyed);

                for (int i = 0; i < GameConstants.NumAsteroids; i++)
                {
                    packetWriter.Write(player.asteroidList[i].isAct);
                    packetWriter.Write(player.asteroidList[i].position);
                }

                gamer.SendData(packetWriter, SendDataOptions.None);
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

        private void DrawGameplay(GameTime gameTime)
        {
            GraphicsDevice.Viewport = mainViewport;
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Player player;
            if (networkSession != null)
            {

                foreach (NetworkGamer networkGamer in networkSession.AllGamers)
                {
                    player = networkGamer.Tag as Player;
                    if (networkGamer.IsLocal)
                    {
                        DrawPlayer(player, leftViewport);
                    }
                    else
                    {
                        DrawPlayer(player, rightViewport);
                    }
                }
            }
        }

        private void DrawTitleScreen()
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            string message = "";

            if (SignedInGamer.SignedInGamers.Count == 0)
            {
                message = "No profile signed in! \n" +
                "Press the Home key on the keyboard or \n" +
                "the Xbox Guide Button on the controller to sign in.";
            }
            else
            {
                message += "Press A to create a new session\n" +
                    "X to search for sessions\nB to quit\n\n";
            }
            spriteBatch.Begin();
            spriteBatch.DrawString(font, message,
                new Vector2(101, 101), Color.Black);
            spriteBatch.DrawString(font, message,
                new Vector2(100, 100), Color.White);
            spriteBatch.End();
        }

        protected void HandleTitleScreenInput()
        {
            if (IsButtonPressed(Buttons.A))
            {
                CreateSession();
            }
            else if (IsButtonPressed(Buttons.X))
            {
                availableSessions = NetworkSession.Find(
                    NetworkSessionType.SystemLink, 1, null);

                selectedSessionIndex = 0;
            }
            else if (IsButtonPressed(Buttons.B))
            {
                Exit();
            }
        }

        void CreateSession()
        {
            networkSession = NetworkSession.Create(NetworkSessionType.SystemLink, 1, 8, 2, null);

            networkSession.AllowHostMigration = true;
            networkSession.AllowJoinInProgress = true;

            HookSessionEvents();
        }
        private void HookSessionEvents()
        {
            networkSession.GamerJoined +=
            new EventHandler<GamerJoinedEventArgs>(networkSession_GamerJoined);
        }

        void networkSession_GamerJoined(object sender, GamerJoinedEventArgs e)
        {
            if (!e.Gamer.IsLocal)
            {
                e.Gamer.Tag = new Player();
            }
            else
            {
                e.Gamer.Tag = GetPlayer(e.Gamer.Gamertag);
            }
        }
        Player GetPlayer(String gamertag)
        {
            foreach (SignedInGamer signedInGamer in
                SignedInGamer.SignedInGamers)
            {
                if (signedInGamer.Gamertag == gamertag)
                {
                    return signedInGamer.Tag as Player;
                }
            }

            return new Player();
        }

        private void DrawAvailableSessions()
        {
            GraphicsDevice.Clear(Color.CornflowerBlue); spriteBatch.Begin();
            float y = 100;

            spriteBatch.DrawString(font, "Available sessions (A=join, B=back)", new Vector2(101, y + 1), Color.Black);
            spriteBatch.DrawString(font, "Available sessions (A=join, B=back)", new Vector2(100, y), Color.White);

            y += font.LineSpacing * 2; int selectedSessionIndex = 0;
            for (
            int sessionIndex = 0;
            sessionIndex < availableSessions.Count; sessionIndex++)
            {
                Color color = Color.Black;

                if (sessionIndex == selectedSessionIndex) color = Color.Yellow;

                spriteBatch.DrawString(font, availableSessions[sessionIndex].HostGamertag, new Vector2(100, y), color);

                y += font.LineSpacing;
            }
            spriteBatch.End();
        }
        protected void HandleAvailableSessionsInput()
        {
            if (IsButtonPressed(Buttons.A))
            {
                // Join the selected session. 
                if (availableSessions.Count > 0)
                {
                    networkSession = NetworkSession.Join(availableSessions[selectedSessionIndex]);
                    HookSessionEvents();

                    availableSessions.Dispose(); availableSessions = null;
                }
            }
            else if (IsButtonPressed(Buttons.DPadUp))
            {
                // Select the previous session from the list. 
                if (selectedSessionIndex > 0)
                    selectedSessionIndex--;
            }
            else if (IsButtonPressed(Buttons.DPadDown))
            {
                // Select the next session from the list.
                if (selectedSessionIndex < availableSessions.Count - 1) selectedSessionIndex++;
            }
            else if (IsButtonPressed(Buttons.B))
            {
                // Go back to the title screen.
                availableSessions.Dispose();
                availableSessions = null;
            }

        }

        protected override void Draw(GameTime gameTime)
        {
            if (networkSession != null)
            {
                // Draw the Lobby
                if (networkSession.SessionState == NetworkSessionState.Lobby) DrawLobby();
                else
                    DrawGameplay(gameTime);
            }
            else if (availableSessions != null)
            {
                DrawAvailableSessions();
            }
            else
            {
                DrawTitleScreen();
            }

            base.Draw(gameTime);
        }


        void DrawPlayer(Player player, Viewport viewport)
        {
            graphics.GraphicsDevice.Viewport = viewport;
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            spriteBatch.Draw(stars, new Rectangle(0, 0, 800, 600), Color.White);
            spriteBatch.End();

            Matrix shipTransformMatriz = player.ship.RotationMatrix * Matrix.CreateTranslation(player.ship.Position);
            if (player.ship.isAct)
            {
                DrawModel(shipModel, shipTransformMatriz, shipTransforms);
            }

            for (int i = 0; i < GameConstants.NumAsteroids; i++)
            {
                Matrix astTrans = Matrix.CreateTranslation(player.asteroidList[i].position);

                if (astList[i].isAct)
                {
                    DrawModel(astMod, astTrans, astTras);
                }
            }

            for (int i = 0; i < GameConstants.NumBull; i++)
            {
                if (player.bulletList[i].isAct)
                {
                    Matrix bullTrans = Matrix.CreateTranslation(player.bulletList[i].position);
                    DrawModel(bullMod, bullTrans, bullTras);
                }
            }
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatch.DrawString(font, "Score: " + player.score, scrPos, Color.LightGreen);
            spriteBatch.End();
        }

        /*bool CheckBullAsCol(float bRadio, float aRadio)
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
        */
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
            //añadir balla en slot de bala incativo
            //No se añadira ballas si todos los espacios estan en uso
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

        void SignedInGamer_SignedIn(object sender, SignedInEventArgs e)
        {
            e.Gamer.Tag = new Player();
        }

        private void HandleGameplayInput(Player player, GameTime gameTime)
        {
            if (IsButtonPressed(Buttons.Back))
                this.Exit();

            // change UpdateInput to take a Player 
            UpdateInput(player, gameTime);

            player.Update(gameTime);

            networkSession.Update();

            //base.Update(gameTime);
        }

        void ReceiveNetworkData(LocalNetworkGamer gamer, GameTime gameTime)
        {
            while (gamer.IsDataAvailable)
            {
                NetworkGamer sender; gamer.ReceiveData(packetReader, out sender);

                if (!sender.IsLocal)
                {
                    Player player = sender.Tag as Player; player.ship.isAct = packetReader.ReadBoolean(); player.ship.Position = packetReader.ReadVector3(); player.ship.Rotation = packetReader.ReadSingle(); player.score = packetReader.ReadInt32();
                    if (packetReader.ReadBoolean())
                    {
                        player.ShootBullet();
                    }
                    if (packetReader.ReadBoolean())
                    {
                        player.ship.isAct = false;
                    }
                    for (int i = 0; i < GameConstants.NumAsteroids; i++)
                    {
                        player.asteroidList[i].isAct = packetReader.ReadBoolean();
                        player.asteroidList[i].position = packetReader.ReadVector3();
                    }
                    player.Update(gameTime);
                }
            }
        }

    }
}
