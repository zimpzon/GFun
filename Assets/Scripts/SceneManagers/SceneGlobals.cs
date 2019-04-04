using UnityEngine;

public class SceneGlobals : MonoBehaviour
{
    public static SceneGlobals Instance;

    public PlayerScript PlayerScript;
    public CameraPositioner CameraPositioner;
    public CameraShake CameraShake;
    public MapScript MapScript;
    public SoundManager SoundManager;
    public ParticleScript ParticleScript;
    public DebugLinesScript DebugLinesScript;
    public LightingImageEffect LightingImageEffect;
    public LightingCamera LightingCamera;
    public MapCamera MapCamera;
    public LayerMask MapLayer;
    public LayerMask PlayerLayer;
    public LayerMask PlayerDamageLayer;
    public LayerMask PlayerInteractionLayer;
    public LayerMask EnemyLayer;
    public LayerMask EnemyDamageLayer;
    public AiBlackboard AiBlackboard;

    void Awake()
    {
        MapLayer = LayerMask.NameToLayer("Map");
        PlayerLayer = LayerMask.NameToLayer("Player");
        PlayerDamageLayer = LayerMask.NameToLayer("PlayerDamage");
        PlayerInteractionLayer = LayerMask.NameToLayer("PlayerInteraction");
        EnemyLayer = LayerMask.NameToLayer("Enemy");
        EnemyDamageLayer = LayerMask.NameToLayer("EnemyDamage");

        PlayerScript = FindObjectOfType<PlayerScript>();
        CameraPositioner = FindObjectOfType<CameraPositioner>();
        CameraShake = FindObjectOfType<CameraShake>();
        SoundManager = FindObjectOfType<SoundManager>();
        MapScript = FindObjectOfType<MapScript>();
        ParticleScript = FindObjectOfType<ParticleScript>();
        DebugLinesScript = FindObjectOfType<DebugLinesScript>();
        LightingImageEffect = FindObjectOfType<LightingImageEffect>();
        LightingCamera = FindObjectOfType<LightingCamera>();
        MapCamera = FindObjectOfType<MapCamera>();
        AiBlackboard = FindObjectOfType<AiBlackboard>();

        Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }
}
