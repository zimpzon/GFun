using UnityEngine;

[ExecuteInEditMode]
public class MaterialColorSetter : MonoBehaviour
{
    public Color Color;

    Renderer renderer_;
    MaterialPropertyBlock propBlock_;
    Color prevColor_;

    void Awake()
    {
        propBlock_ = new MaterialPropertyBlock();
        renderer_ = GetComponent<Renderer>();
    }

    void Update()
    {
        if (Color != prevColor_)
        {
            renderer_.GetPropertyBlock(propBlock_);
            propBlock_.SetColor("_Color", Color);
            renderer_.SetPropertyBlock(propBlock_);

            prevColor_ = Color;
        }
    }
}
