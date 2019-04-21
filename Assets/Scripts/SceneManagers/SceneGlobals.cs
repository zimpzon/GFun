using GFun;
using System;
using UnityEngine;

public class SceneGlobals : MonoBehaviour
{
    public static SceneGlobals Instance;

    public CameraPositioner CameraPositioner;
    public CameraShake CameraShake;
    public MapScript MapScript;
    public IMapAccess MapAccess;
    public AudioManager AudioManager;
    public ParticleScript ParticleScript;
    public DebugLinesScript DebugLinesScript;
    public LightingImageEffect LightingImageEffect;
    public LightingCamera LightingCamera;
    public MiniMapCamera MapCamera;
    public LayerMask MapLayer;
    public LayerMask PlayerLayer;
    public LayerMask PlayerDamageLayer;
    public LayerMask PlayerInteractionLayer;
    public LayerMask EnemyLayer;
    public LayerMask EnemyDamageLayer;
    public LayerMask DeadEnemyLayer;
    public GameObjectPool PlainBulletPool;
    public GameObjectPool ElongatedBulletPool;
    public GameObjectPool EnemyBullet1Pool;
    public PlayableCharacters PlayableCharacters;
    public int OnTheFloorSortingValue = 15;

    void Awake()
    {
        MapLayer = LayerMask.NameToLayer("Map");
        PlayerLayer = LayerMask.NameToLayer("Player");
        PlayerDamageLayer = LayerMask.NameToLayer("PlayerDamage");
        PlayerInteractionLayer = LayerMask.NameToLayer("PlayerInteraction");
        EnemyLayer = LayerMask.NameToLayer("Enemy");
        EnemyDamageLayer = LayerMask.NameToLayer("EnemyDamage");
        DeadEnemyLayer = LayerMask.NameToLayer("DeadEnemy");

        CameraPositioner = FindObjectOfType<CameraPositioner>();
        CameraShake = FindObjectOfType<CameraShake>();
        AudioManager = FindObjectOfType<AudioManager>();
        MapScript = FindObjectOfType<MapScript>();
        MapAccess = (IMapAccess)MapScript;
        ParticleScript = FindObjectOfType<ParticleScript>();
        DebugLinesScript = FindObjectOfType<DebugLinesScript>();
        LightingImageEffect = FindObjectOfType<LightingImageEffect>();
        LightingCamera = FindObjectOfType<LightingCamera>();
        MapCamera = FindObjectOfType<MiniMapCamera>();
        PlayableCharacters = FindObjectOfType<PlayableCharacters>();

        NullCheck(MapLayer);
        NullCheck(PlayerLayer);
        NullCheck(PlayerDamageLayer);
        NullCheck(PlayerInteractionLayer);
        NullCheck(EnemyLayer);
        NullCheck(EnemyDamageLayer);
        NullCheck(DeadEnemyLayer);

        Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }

    public static void NullCheck(object o, string message = "")
    {
        if (o == null)
        {
            Instance.DebugLinesScript.SetLine($"Null check failed ({message}), see console", Time.time);
            throw new ArgumentNullException();
        }
    }
}
