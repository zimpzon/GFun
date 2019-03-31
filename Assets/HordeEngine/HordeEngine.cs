using UnityEngine;

// https://0x72.itch.io/pixeldudesmaker

namespace HordeEngine
{
    /// <summary>
    /// Shortcut class
    /// </summary>
    public static class Horde
    {
        public static HordeEngine Engine;
        public static ComponentUpdater ComponentUpdater = new ComponentUpdater();
        public static TimeManager Time = new TimeManager();
        public static HordeSpriteManager Sprites;
        public static AiBlackboard AiBlackboard = new AiBlackboard();
    }

    /// <summary>
    /// HordeEngine root
    /// </summary>
    public class HordeEngine : MonoBehaviour
    {
        void Awake()
        {
            Horde.Engine = this;
        }

        void Update()
        {
            // This is the first Update() to be called in every frame
            Horde.Time.UpdateTime(Time.deltaTime);

            Horde.ComponentUpdater.DoUpdate();
        }

        void LateUpdate()
        {
            // This is the first LateUpdate() to be called in every frame
            Horde.ComponentUpdater.DoLateUpdate();
        }
    }
}
