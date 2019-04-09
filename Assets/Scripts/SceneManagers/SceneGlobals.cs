using GFun;
using UnityEngine;

public class SceneGlobals : MonoBehaviour
{
    public static SceneGlobals Instance;

    public PlayableCharacterScript PlayerScript;
    public CameraPositioner CameraPositioner;
    public CameraShake CameraShake;
    public MapScript MapScript;
    public IMapAccess MapAccess;
    public AudioManager AudioManager;
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
    public WeaponPrefabs WeaponPrefabs;
    public GameObjectPool PlainBulletPool;
    public GameObjectPool ElongatedBulletPool;

    void Awake()
    {
        MapLayer = LayerMask.NameToLayer("Map");
        PlayerLayer = LayerMask.NameToLayer("Player");
        PlayerDamageLayer = LayerMask.NameToLayer("PlayerDamage");
        PlayerInteractionLayer = LayerMask.NameToLayer("PlayerInteraction");
        EnemyLayer = LayerMask.NameToLayer("Enemy");
        EnemyDamageLayer = LayerMask.NameToLayer("EnemyDamage");

        PlayerScript = FindObjectOfType<PlayableCharacterScript>();
        CameraPositioner = FindObjectOfType<CameraPositioner>();
        CameraShake = FindObjectOfType<CameraShake>();
        AudioManager = FindObjectOfType<AudioManager>();
        MapScript = FindObjectOfType<MapScript>();
        MapAccess = (IMapAccess)MapScript;
        ParticleScript = FindObjectOfType<ParticleScript>();
        DebugLinesScript = FindObjectOfType<DebugLinesScript>();
        LightingImageEffect = FindObjectOfType<LightingImageEffect>();
        LightingCamera = FindObjectOfType<LightingCamera>();
        MapCamera = FindObjectOfType<MapCamera>();
        AiBlackboard = FindObjectOfType<AiBlackboard>();
        WeaponPrefabs = FindObjectOfType<WeaponPrefabs>();

        Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }
}
