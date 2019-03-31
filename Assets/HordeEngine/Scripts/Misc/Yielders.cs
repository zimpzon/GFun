using System.Collections;

namespace HordeEngine
{
    public class Yielders
    {
        public static IEnumerator WaitUntil(float endTime, bool slowableTime = true)
        {
            if (slowableTime)
            {
                while (Horde.Time.SlowableTime < endTime)
                    yield return null;
            }
            else
            {
                while (Horde.Time.Time < endTime)
                    yield return null;
            }
        }

        public static IEnumerator WaitForSeconds(float ms, bool slowableTime = true)
        {
            float endTime = slowableTime ? Horde.Time.SlowableTime + ms : Horde.Time.Time + ms;
            yield return WaitUntil(endTime, slowableTime);
        }
    }
}
