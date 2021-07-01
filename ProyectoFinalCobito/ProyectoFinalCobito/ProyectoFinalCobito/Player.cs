using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProyectoFinalCobito
{
    public class Player
    {
        internal GamePadState lastSate;
        internal Ship ship = new Ship();
        internal Asteroid[] asteroidList = new Asteroid[GameConstants.NumAsteroids];
        internal Bullet[bulletList = new Bullet[GameConstants.NumBullets]];
        internal interface score
        {

        }
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
            //BLED OFF VELOVITY OVER TIME
            ship.Velocity *= 0.95f;
            for (int i = 0; i < GameConstants.NumBull; i++)
            {
                asteroidList[i].Update(timeDelta);
            }
            for (int i = 0; i < GameConstants.NumBull; i++)
            {
                if (bulletList[i].isActive)
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
                double angle = random.NextDouble * 2 * Math.PI;
                asteroidList[i].direction.X = -(float)Math.Sin(angle);
                asteroidList[i].direction.Y = (float)Math.Cos(angle);
                asteroidList[i].speed = GameConstants.AsteroidMinSpeed + random.NextDouble * GameConstants.AsteroidMaxSpeed;
                asteroidList[i].isAct = true;
            }
        }
        internal void ShootBullet()
        {
            //ADD ANOTHER BULLET. FIND AN INACTIVE BULLET SLOT AND USE IT
            //IF ALL BULLET SLOTS ARE USED, IGNORE THE USER INPUT
            for (int i = 0; i < GameConstants.NumBullets; i++)
            {
                if (!bulletList[i].isActive)
                {
                    bulletList[i].direction = ship.RotationMatrix.Forward;
                    bulletList[i].speed = GameConstants.BulletSpeedAdjustment;
                    bulletList[i].position = ship.Position + (200 * bulletList[i].direction);
                    bulletList[i].isActive = true;
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
            ship.isActive = true;
            score -= GameConstants.WarpPenalty;
        }
        internal bool CheckForBulletAsteroidCollision(float bulletRadius,
float asteroidRadius)
        {
            for (int i = 0; i < asteroidList.Length; i++)
            {
                if (asteroidList[i].isActive)
                {
                    BoundingSphere asteroidSphere =
                    new BoundingSphere(
                    asteroidList[i].position, asteroidRadius *
                    GameConstants.AsteroidBoundingSphereScale);
                    for (int j = 0; j < bulletList.Length; j++)
                    {
                        if (bulletList[j].isActive)
                        {
                            BoundingSphere bulletSphere =
                            new BoundingSphere(bulletList[j].position,
                            bulletRadius);
                            if (asteroidSphere.Intersects(bulletSphere))
                            {
                                asteroidList[i].isActive = false;
                                bulletList[j].isActive = false;
                                score += GameConstants.KillBonus;
                                return true; //no need to check other bullets
                            }
                        }
                    }
                }
            }
            return false;
        }
        internal bool CheckForShipAsteroidCollision(float shipRadius,
float asteroidRadius)
        {
            //ship-asteroid collision check
            if (ship.isActive)
            {
                BoundingSphere shipSphere =
                new BoundingSphere(ship.Position, shipRadius *
                GameConstants.ShipBoundingSphereScale);
                for (int i = 0; i < asteroidList.Length; i++)
                {
                    if (asteroidList[i].isActive)
                    {
                        BoundingSphere b =
                        new BoundingSphere(asteroidList[i].position,
                        asteroidRadius *
                        GameConstants.AsteroidBoundingSphereScale);
                        if (b.Intersects(shipSphere))
                        {
                            //blow up ship
                            //soundExplosion3.Play();
                            ship.isActive = false;
                            asteroidList[i].isActive = false;
                            score -= GameConstants.DeathPenalty;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }

    protected void UpdateInput()
    {
        currentState = GamePad.GetState(PlayerIndex.One);
        lastState = player.lastState;
        if (currentState.IsConnected)
        {
            if (player.ship.isActive)
            {
                player.ship.Update(currentState);
                PlayEngineSound(currentState);
            }
            // In case you get lost, press B to warp back to the center.
            if (IsButtonPressed(Buttons.B))
            {
                player.WarpToCenter();
                soundHyperspaceActivation.Play();
            }
            //are we shooting?
            if (player.ship.isActive && IsButtonPressed(Buttons.A))
            {
                player.ShootBullet();
                soundWeaponsFire.Play();
                bool isFiring = true;
            }
            player.lastState = currentState;
        }
    }
}
            Player player = new Player();

        internal void Update(GameTime gametime)
        {
            float timeDelta = (float)gametime.ElapsedGameTime.TotalSeconds;
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
                    

        
    
