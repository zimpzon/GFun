using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using HordeEngine;

public class HordeBatchRendererTests
{
    HordeBatchRenderer br;

    void TestCase(Texture2D tex, Material mat, int batchSize, int quadsToAdd, int expectedMeshCount)
    {
        Sprite someSprite = Sprite.Create(tex, Rect.MinMaxRect(0.0f, 0.0f, 1.0f, 1.0f), Vector2.one);

        br = new HordeBatchRenderer(111, tex, mat, 0, batchSize);

        for (int i = 0; i < quadsToAdd; ++i)
            br.AddQuad(Vector3.one, Vector2.one, 45.0f, 1.0f, Color.yellow, someSprite);

        Assert.AreEqual(quadsToAdd, br.QuadCount);
        Assert.AreEqual(br.Meshes.Count, expectedMeshCount);

        br.ApplyChanges();
    }

    [UnityTest]
    public IEnumerator TestHordeBatchRenderer()
    {
        Texture2D tex = new Texture2D(10, 10);
        Material mat = new Material(Shader.Find("Sprites/Diffuse"));

        TestCase(tex, mat, 100, 0, 0); // Empty
        TestCase(tex, mat, 25, 500, 20); // Edge case, exacty filled
        TestCase(tex, mat, 10, 9, 1); // Edge case, 1 below
        TestCase(tex, mat, 10, 11, 2); // Edge case, 1 above

        // Test clear
        // Verify not empty before clear
        Assert.Greater(br.QuadCount, 0);
        Assert.Greater(br.Meshes.Count, 0);
        Assert.Greater(br.Meshes[0].ActiveQuadCount, 0);

        br.Clear();

        Assert.AreEqual(br.QuadCount, 0);
        Assert.Greater(br.Meshes.Count, 0); // Meshes are kept to not generate GC, so not empty after clear
        Assert.AreEqual(br.Meshes[0].ActiveQuadCount, 0);

        yield return null;
    }
}
