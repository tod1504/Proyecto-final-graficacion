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
}
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
        
    
