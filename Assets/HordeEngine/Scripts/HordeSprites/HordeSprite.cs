using UnityEngine;

namespace HordeEngine
{
    public class HordeSprite : MonoBehaviour, IComponentUpdate
    {
        public Sprite Sprite;
        public Material Material;
        public bool FlipX;
        [SerializeField, Layer] public LayerMask Layer;
        public Color32 Color = UnityEngine.Color.white;
        public Vector2 Scale = Vector2.one;
        public Vector3 Offset = Vector3.zero;
        public float Rotation;

        int layer_;

        Transform trans_;

        /// <summary>
        /// Cache the conversion from LayerMask to (int)layer to avoid engine calls per frame.
        /// </summary>
        void UpdateLayer()
        {
            layer_ = (int)Layer;
        }

        void OnValidate()
        {
            UpdateLayer();
        }
        
        void OnEnable()
        {
            trans_ = transform;
            UpdateLayer();
            Horde.ComponentUpdater.RegisterForUpdate(this, ComponentUpdatePass.Late);
        }

        void OnDisable()
        {
            Horde.ComponentUpdater.UnregisterForUpdate(this, ComponentUpdatePass.Late);
        }

        // TODO
        // Enable whole GO: nothing drawn.
        // Click any component in inspector: drawn
        // (NO its not.. wth) Hey hey! Light sprite can be toggled np. Sooo. Its a camera thing.
        public void ComponentUpdate(ComponentUpdatePass pass)
        {
            var scale = Scale;
            scale.x *= FlipX ? -1 : 1;
            if (Sprite != null && Material != null)
                Horde.Sprites.AddQuad(trans_.position + Offset, scale, Rotation, Scale.y, Color, Sprite, Material, layer_);
        }
    }
}
