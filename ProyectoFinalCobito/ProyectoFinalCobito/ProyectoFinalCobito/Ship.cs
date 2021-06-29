using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProyectoFinalCobito
{
    class Nave
    {
        public Model Modelo;
        public Matrix[] Transforms;

        //Posiciona modelo en el espacio
        public Vector3 Position = Vector3.Zero;

        //Velocidad de control
        private const float VelocityScale = 5.0f;

        public bool isAct = true;

        //Velocidad del modelo
        public Vector3 Velocity = Vector3.Zero;

        public Matrix RotationMatrix = Matrix.CreateRotationX(MathHelper.PiOver2);
        private float rotation;
        public float Rotation
        {
            get { return rotation; }
            set
            {
                float newVal = value;
                while (newVal >= MathHelper.TwoPi)
                {
                    newVal -= MathHelper.TwoPi;
                }
                while (newVal < 0)
                {
                    newVal += MathHelper.TwoPi;
                }

                if (rotation != newVal)
                {
                    rotation = newVal;
                    RotationMatrix = Matrix.CreateRotationX(MathHelper.PiOver2) * 
                        Matrix.CreateRotationZ(rotation);
                }
            }
        }
        public void Update(GamePadState controlState)
        {
            //Rota nave con palanca izquierda
            Rotation -= controlState.ThumbSticks.Left.X * 0.10f;

            //Agrega vector a velocidad
            Velocity += RotationMatrix.Forward * VelocityScale * controlState.Triggers.Right;
        }
    }
}