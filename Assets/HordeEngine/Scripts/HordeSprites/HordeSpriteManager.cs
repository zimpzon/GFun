using System;
using UnityEngine;

namespace HordeEngine
{
    public class HordeSpriteManager : MonoBehaviour, IComponentUpdate
    {
        public Vector3 Offset = Vector3.zero;
        [NonSerialized] public int QuadsPerBatchMesh = 256;
        [NonSerialized] public int SpritesRendered;
        [NonSerialized] public int MeshesRendered;

        const int InitialRendererCapacity = 128;
        HordeBatchRenderer[] batches_;
        UInt64[] keys_;
        int rendererCount_;
        Camera[] cameras_ = new Camera[0];

        public void AddQuad(Vector3 center, Vector2 size, float rotationDegrees, float zSkew, Color32 color, Sprite sprite, Material material, int layer)
        {
            var batch = GetBatchRenderer(sprite, material, layer);
            batch.AddQuad(center, size, rotationDegrees, zSkew, color, sprite);
        }

        public HordeBatchRenderer GetBatchRenderer(Sprite sprite, Material material, int layer)
        {
            UInt64 key = ((UInt64)sprite.texture.GetInstanceID() << 29) + ((UInt64)material.GetInstanceID() << 6) + (UInt64)layer;
            int idx = -1;
            for (int i = 0; i < rendererCount_; ++i)
            {
                if (keys_[i] == key)
                {
                    idx = i;
                    break;
                }
            }

            if (idx < 0)
            {
                idx = CreateBatchRenderer(key, sprite.texture, material, layer);
                Debug.LogFormat("HordeBatchRenderer created, idx = {0}, key = {1}", idx, key);
                Debug.LogWarning("FIX ME. HordeBatchMesh have to call RecalculateBounds or everything is culled."); // Maybe manually render to camera on PreRender.
            }

            return batches_[idx];
        }

        private int CreateBatchRenderer(UInt64 key, Texture sprite, Material material, int layer)
        {
            if (rendererCount_ >= batches_.Length)
            {
                Array.Resize(ref batches_, batches_.Length + batches_.Length / 2);
                Array.Resize(ref keys_, batches_.Length);
            }

            var batch = new HordeBatchRenderer(key, sprite, material, layer, QuadsPerBatchMesh);
            batches_[rendererCount_] = batch;
            keys_[rendererCount_++] = key;
            return rendererCount_ - 1;
        }

        void OnEnable()
        {
            Horde.Sprites = this;
            batches_ = new HordeBatchRenderer[InitialRendererCapacity];
            keys_ = new UInt64[InitialRendererCapacity];
            rendererCount_ = 0;

            Array.Resize(ref cameras_, Camera.allCamerasCount);
            Camera.GetAllCameras(cameras_);
            Horde.ComponentUpdater.RegisterForUpdate(this, ComponentUpdatePass.Internal_DrawMeshes);
        }

        void OnDisable()
        {
            Horde.ComponentUpdater.UnregisterForUpdate(this, ComponentUpdatePass.Internal_DrawMeshes);
        }

        public void ComponentUpdate(ComponentUpdatePass pass)
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            matrix.SetTRS(Offset, Quaternion.identity, Vector3.one);

            SpritesRendered = 0;
            MeshesRendered = 0;
            for (int i = 0; i < rendererCount_; ++i)
            {
                var batch = batches_[i];
                batch.ApplyChanges();
                int activeMeshes = batch.GetActiveMeshCount();
                for (int j = 0; j < activeMeshes; ++j)
                {
                    // TODO: DrawMesh does not always show in the editor. Seems to lag exactly one update behind.
                    Graphics.DrawMesh(batch.Meshes[j].Mesh, matrix, batch.Material, batch.Layer);
                    SpritesRendered += batch.Meshes[j].ActiveQuadCount;
                    MeshesRendered++;
                }
                batch.Clear();
            }
        }
    }
}
