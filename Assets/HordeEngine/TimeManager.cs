namespace HordeEngine
{
    // Data that will be updated by GameManager at the beginning of every frame.
    public class TimeManager
    {
        public float Time;
        public float DeltaTime;
        public float SlowableTime;
        public float DeltaSlowableTime;
        public float SlowableTimeScale = 1.0f;

        public float GetDeltaTime(bool slowable)
        {
            return slowable ? DeltaSlowableTime : DeltaTime;
        }

        public void UpdateTime(float delta)
        {
            DeltaTime = delta;
            Time += DeltaTime;

            DeltaSlowableTime = DeltaTime * SlowableTimeScale;
            SlowableTime += DeltaSlowableTime;
        }
    }
}
