#pragma warning disable 1591

namespace Apex.Examples.AI.Helpers
{
    using UnityEngine;

    public sealed class LayersManagerComponent : MonoBehaviour
    {
        public static LayersManagerComponent instance;

        public LayerMask unitsLayer;
        public LayerMask obstacleLayer;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                return;
            }

            Debug.LogWarning(this.ToString() + " multiple " + this.ToString() + " found in the scene");
            Destroy(this.gameObject, 0.01f);
        }
    }
}