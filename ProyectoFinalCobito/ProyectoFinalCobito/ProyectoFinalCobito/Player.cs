using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProyectoFinalCobito
{
    public class Player
    {
        internal GamePadState lastState;
        internal Nave ship = new Nave();
        internal Asteroid[] asteroidList = new Asteroid[GameConstants.NumAsteroids];
        internal Bullet[] bulletList = new Bullet[GameConstants.NumBull];
        internal int score;

        Random random = new Random();

        public Player()
        {
            ResetAsteroids();

        }
        internal void Update(GameTime gameTime)
        {
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            //ADD VELOCITY TO THE CURRENT POSITION
            ship.Position += ship.Velocity;

            for (int i = 0; i < GameConstants.NumAsteroids; i++)
            {
                asteroidList[i].Update(timeDelta);
            }
            for (int i = 0; i < GameConstants.NumBull; i++)
            {
                if (bulletList[i].isAct)
                {
                    bulletList[i].Update(timeDelta);
                }
            }
        }
        private void ResetAsteroids()
        {
            float xStar;
            float yStar;
            for (int i = 0; i < GameConstants.NumAsteroids; i++)
            {
                if (random.Next(2) == 0)
                {
                    xStar = (float)-GameConstants.PlayfieldSizeX;
                }
                else
                {
                    xStar = (float)GameConstants.PlayfieldSizeX;
                }
                yStar = (float)random.NextDouble() * GameConstants.PlayfieldSizeY;
                asteroidList[i].position = new Vector3(xStar, yStar, 0.0f);
                double angle = random.NextDouble() * 2 * Math.PI;
                asteroidList[i].direction.X = -(float)Math.Sin(angle);
                asteroidList[i].direction.Y = (float)Math.Cos(angle);
                asteroidList[i].speed = GameConstants.AsteroidMinSpeed + (float)random.NextDouble() * GameConstants.AsteroidMaxSpeed;
                asteroidList[i].isAct = true;
            }
        }
        internal void ShootBullet()
        {
            //ADD ANOTHER BULLET. FIND AN INACTIVE BULLET SLOT AND USE IT
            //IF ALL BULLET SLOTS ARE USED, IGNORE THE USER INPUT
            for (int i = 0; i < GameConstants.NumBull; i++)
            {
                if (!bulletList[i].isAct)
                {
                    bulletList[i].direction = ship.RotationMatrix.Forward;
                    bulletList[i].speed = GameConstants.BullSpeedAdj;
                    bulletList[i].position = ship.Position + (200 * bulletList[i].direction);
                    bulletList[i].isAct = true;
                    score -= GameConstants.ShotPenalty;
                    return;
                }
            }
        }
        internal void WarpToCenter()
        {
            ship.Position = Vector3.Zero;
            ship.Velocity = Vector3.Zero;
            ship.Rotation = 0.0f;
            ship.isAct = true;
            score -= GameConstants.WarpPenalty;
        }
        internal bool CheckForBulletAsteroidCollision(float bulletRadius, float asteroidRadius)
        {
            for (int i = 0; i < asteroidList.Length; i++)
            {
                if (asteroidList[i].isAct)
                {
                    BoundingSphere asteroidSphere =
                    new BoundingSphere(
                    asteroidList[i].position, asteroidRadius *
                    GameConstants.AsteroidBoundingSphereScale);
                    for (int j = 0; j < bulletList.Length; j++)
                    {
                        if (bulletList[j].isAct)
                        {
                            BoundingSphere bulletSphere =
                            new BoundingSphere(bulletList[j].position,
                            bulletRadius);
                            if (asteroidSphere.Intersects(bulletSphere))
                            {
                                asteroidList[i].isAct = false;
                                bulletList[j].isAct = false;
                                score += GameConstants.KillBonus;
                                return true; //no need to check other bullets
                            }
                        }
                    }
                }
            }
            return false;
        }
        internal bool CheckForShipAsteroidCollision(float shipRadius, float asteroidRadius)
        {
            //ship-asteroid collision check
            if (ship.isAct)
            {
                BoundingSphere shipSphere =
                new BoundingSphere(ship.Position, shipRadius *
                GameConstants.ShipBoundingSphereScale);
                for (int i = 0; i < asteroidList.Length; i++)
                {
                    if (asteroidList[i].isAct)
                    {
                        BoundingSphere b =
                        new BoundingSphere(asteroidList[i].position,
                        asteroidRadius *
                        GameConstants.AsteroidBoundingSphereScale);
                        if (b.Intersects(shipSphere))
                        {
                            //blow up ship
                            //soundExplosion3.Play();
                            ship.isAct = false;
                            asteroidList[i].isAct = false;
                            score -= GameConstants.DeathPenalty;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }

    /* paso 4
    NetworkSession networkSession;
    AvailableNetworkSessionCollection availableSessions;
    int selectedSessionIndex;
    PacketReader packetReader = new PacketReader();
    PacketWriter packetWriter = new PacketWriter();

    //Player player = new Player();

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        aspectRatio = (float)GraphicsDeviceManager.DefaultBackBufferWidth /
         (2 * GraphicsDeviceManager.DefaultBackBufferHeight);

        // Add Gamer Services
        Components.Add(new GamerServicesComponent(this));

        // Respond to the SignedInGamer event SignedInGamer.SignedIn +=
        new EventHandler<SignedInEventArgs>(SignedInGamer_SignedIn);
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
        UpdateInput(player);

        player.Update(gameTime);

        networkSession.Update();

        //base.Update(gameTime);
    }
    protected override void Update(GameTime gameTime)
    {
        if (!Guide.IsVisible)
        {
            foreach (SignedInGamer signedInGamer in SignedInGamer.SignedInGamers)
            {
                Player player = signedInGamer.Tag as Player;
                lastState = player.lastState;
                currentState = GamePad.GetState(signedInGamer.PlayerIndex);

                if (networkSession != null)
                {
                    // Handle the lobby input here...
                }
                else if (availableSessions != null)
                {
                    // Handle the available sessions input here..
                }
                else
                {
                    // Handle the title screen input here..
                }
                player.lastState = currentState;
            }
        }
        base.Update(gameTime);
    }
    protected void UpdateInput(Player player)
    {
        //// Get the game pad state.
        //currentState = GamePad.GetState(PlayerIndex.One);
        //lastState = player.lastState;
        ...
        //player.lastState = currentState;
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
        spriteBatch.DrawString(lucidaConsole, message,
            new Vector2(101, 101), Color.Black);
        spriteBatch.DrawString(lucidaConsole, message,
            new Vector2(100, 100), Color.White);
        spriteBatch.End();
    }

    protected override void Draw(GameTime gameTime)
    {
        if (networkSession != null)
        {
        }
        else if (availableSessions != null)
        {
            // Show the available session...
        }
        else
        {
            DrawTitleScreen();
        }

        base.Draw(gameTime);
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
    private void DrawLobby()
    {
        GraphicsDevice.Clear(Color.CornflowerBlue); spriteBatch.Begin();
        float y = 100;

        spriteBatch.DrawString(lucidaConsole, "Lobby (A=ready, B=leave)", new Vector2(101, y + 1), Color.Black);
        spriteBatch.DrawString(lucidaConsole, "Lobby (A=ready, B=leave)", new Vector2(101, y), Color.White);

        y += lucidaConsole.LineSpacing * 2;

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
            spriteBatch.DrawString(lucidaConsole, text, new Vector2(170, y), Color.White);

            y += lucidaConsole.LineSpacing + 64;
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
    protected override void Draw(GameTime gameTime)
    {
        if (networkSession != null)
        {
            //If the session is not null, we're either
            //in the lobby or playing the game...
            // Draw the Lobby
            if (networkSession.SessionState == NetworkSessionState.Lobby) DrawLobby();
        }
        else if (availableSessions != null)
        {
            // Show the available session...
        }
        else
        {
            DrawTitleScreen();
        }

        base.Draw(gameTime);
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
                }
                else if (availableSessions != null)
                {
                    // Handle the available sessions input here...
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
    private void DrawAvailableSessions()
    {
        GraphicsDevice.Clear(Color.CornflowerBlue); spriteBatch.Begin();
        float y = 100;

        spriteBatch.DrawString(lucidaConsole, "Available sessions (A=join, B=back)", new Vector2(101, y + 1), Color.Black);
        spriteBatch.DrawString(lucidaConsole, "Available sessions (A=join, B=back)", new Vector2(100, y), Color.White);

        y += lucidaConsole.LineSpacing * 2; int selectedSessionIndex = 0;
        for (
        int sessionIndex = 0;
        sessionIndex < availableSessions.Count; sessionIndex++)
        {
            Color color = Color.Black;

            if (sessionIndex == selectedSessionIndex) color = Color.Yellow;

            spriteBatch.DrawString(lucidaConsole, availableSessions[sessionIndex].HostGamertag, new Vector2(100, y), color);

            y += lucidaConsole.LineSpacing;
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
            //If the session is not null, we're either
            //in the lobby or playing the game...
            // Draw the Lobby
            if (networkSession.SessionState == NetworkSessionState.Lobby) DrawLobby();
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
*/

    /*paso 5 

    void ReceiveNetworkData(LocalNetworkGamer gamer, GameTime gameTime)
    {
        while (gamer.IsDataAvailable)
        {
            NetworkGamer sender; gamer.ReceiveData(packetReader, out sender);

            if (!sender.IsLocal)
            {
                Player player = sender.Tag as Player; player.ship.isActive = packetReader.ReadBoolean(); player.ship.Position = packetReader.ReadVector3(); player.ship.Rotation = packetReader.ReadSingle(); player.score = packetReader.ReadInt32();
                if (packetReader.ReadBoolean())
                {
                    player.ShootBullet();
                }
                if (packetReader.ReadBoolean())
                {
                    player.ship.isActive = false;
                }
                for (int i = 0; i < GameConstants.NumAsteroids; i++)
                {
                    player.asteroidList[i].isActive = packetReader.ReadBoolean();
                    player.asteroidList[i].position = packetReader.ReadVector3();
                }
                player.Update(gameTime);
            }
        }
    }
    private void HandleGameplayInput(Player player, GameTime gameTime)
    {
      //  ...
        UpdateInput(player, gameTime);
       // ...
}
    private void UpdateInput(Player player, GameTime gameTime)
    {
        bool isFiring = false; bool shipDestroyed = false;

        foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
        {
            ReceiveNetworkData(gamer, gameTime);

            // this code is the same code we have been
            // using to update the player input 
            if (currentState.IsConnected)
            {

                if (player.ship.isActive)
                {
                    player.ship.Update(currentState); PlayEngineSound(currentState);
                }
                // In case you get lost, press B to warp back to the center. 
                if (IsButtonPressed(Buttons.B))
                {
                    player.WarpToCenter();
                    // Make a sound when we warp. 
                    soundHyperspaceActivation.Play();
                }

                //are we shooting?
                if (player.ship.isActive && IsButtonPressed(Buttons.A))
                {
                    player.ShootBullet(); soundWeaponsFire.Play(); isFiring = true;
                }

                if (player.CheckForBulletAsteroidCollision(bulletModel.Meshes[0].BoundingSphere.Radius, asteroidModel.Meshes[0].BoundingSphere.Radius))
                {
                    soundExplosion2.Play();
                }

                shipDestroyed = player.CheckForShipAsteroidCollision(shipModel.Meshes[0].BoundingSphere.Radius, asteroidModel.Meshes[0].BoundingSphere.Radius);

                if (shipDestroyed)
                {
                    soundExplosion3.Play();
                }
            }
            packetWriter.Write(player.ship.isActive);
            packetWriter.Write(player.ship.Position); 
            packetWriter.Write(player.ship.Rotation); 
            packetWriter.Write(player.score); 
            packetWriter.Write(isFiring); 
            packetWriter.Write(shipDestroyed);
            
            for (int i = 0; i < GameConstants.NumAsteroids; i++)
            {
                packetWriter.Write(player.asteroidList[i].isActive); 
                packetWriter.Write(player.asteroidList[i].position);
            }

            gamer.SendData(packetWriter, SendDataOptions.None);
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
*/



}






///nose que es esto
/*
protected void UpdateInput()
{

    }
}
}
        Player player = new Player();

    internal void Update(GameTime gametime)
    {

    }
protected override void Update(GameTime gametime)
{
//ALLOWS THE GAME TO EXIT
if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
    this.Exit();
//GET SOME INPUT
UpdateInput();


if (pplayer.CheckForBulletAsteroidCollision(bulletModel.Meshes[0].BoundingSphere.Radius, asteroidModel.Meshes[0].BoudingSphere.Radius))
{
    soundExplosion2.Play();
}
bool shipDestroyed = player.CheckForShipAsteroidCollision(shipModel.Meshes[0].BoundingSphere.Radius, asteroidModel.Meshes[0].BoundingSphere.Radius);
if (shipDestroyed)
{
    soundExplosion3.Play();
}
base.Update(gametime);
}

*/


