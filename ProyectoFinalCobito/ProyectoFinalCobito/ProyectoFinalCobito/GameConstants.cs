namespace ProyectoFinalCobito
{
    class GameConstants
    {
        //constantes de camara
        public const float CamHeight = 25000.0f;
        public const float PlayfieldSizeX = 16000f;
        public const float PlayfieldSizeY = 12500f;
        
        //Constante asteroides
        public const int NumAsteroids = 10;
        public const float AsteroidMinSpeed = 100.0f;
        public const float AsteroidMaxSpeed = 300.0f;
        public const float AsteroidSpeedAdjusment = 5.0f;
        public const float AsteroidBoundingSphereScale = 0.95f;
        public const float ShipBoundingSphereScale = 0.5f;

        //Constantes balas
        public const int NumBull = 30;
        public const float BullSpeedAdj = 100.0f;

        public const int ShotPenalty = 1;
        public const int DeathPenalty = 100;
        public const int WarpPenalty = 50;
        public const int KillBonus = 25;
    }
}