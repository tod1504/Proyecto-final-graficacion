using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ProyectoFinalCobito
{
    struct Bullet
    {
        public Vector3 position;
        public Vector3 direction;
        public float speed;

        public bool isAct;

        public void Update(float delta)
        {
            position += direction * speed * GameConstants.BullSpeedAdj * delta;
            if (position.X > GameConstants.PlayfieldSizeX ||
                position.X < -GameConstants.PlayfieldSizeX ||
                position.Y > GameConstants.PlayfieldSizeY ||
                position.Y < -GameConstants.PlayfieldSizeY)
                isAct = false;
        }
    }
}