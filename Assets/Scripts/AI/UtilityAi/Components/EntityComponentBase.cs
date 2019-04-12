#pragma warning disable 1591

namespace Apex.Examples.AI.Game
{
    using System.Collections.Generic;
    using Apex.AI;
    using Apex.AI.Components;
    using Memory;
    using UnityEngine;
#if UNITY_2017 || UNITY_5_5 || UNITY_5_6
    using UnityEngine.AI;
#endif

    public abstract class EntityComponentBase : MonoBehaviour, IAIEntity, IContextProvider
    {
        public ParticleSystem bulletParticleSystem;

        public float defaultAttackRange = 15f;
        public float defaultScanRange = 30f;

        public float defaultMinimumDamage = 1f;
        public float defaultMaximumDamage = 10f;

        public int defaultMaximumAmmunition = 10;

        public float defaultMaxHealth = 100f;

        public bool defaultCanCommunicate = true;

        public Transform[] defaultPatrolPoints;

        private AIContext _context;
        private UnityEngine.AI.NavMeshAgent _navMeshAgent;

        private int _currentAmmo;
        private ParticleSystem _bulletParticle;
        private List<Vector3> _patrolPoints;

        #region IEntity properties

        public float currentHealth
        {
            get;
            set;
        }

        public float minDamage
        {
            get { return this.defaultMinimumDamage; }
        }

        public float maxDamage
        {
            get { return this.defaultMaximumDamage; }
        }

        public int currentAmmo
        {
            get { return _currentAmmo; }
        }

        public int maxAmmo
        {
            get { return this.defaultMaximumAmmunition; }
        }

        public float maxHealth
        {
            get { return this.defaultMaxHealth; }
        }

        public bool isDead
        {
            get { return this.currentHealth <= 0f; }
        }

        public IList<Vector3> patrolPoints
        {
            get { return _patrolPoints; }
        }

        public bool isPatrolling
        {
            get;
            set;
        }

        public int currentPatrolIndex
        {
            get;
            set;
        }

        public UnityEngine.AI.NavMeshAgent navMeshAgent
        {
            get { return _navMeshAgent; }
        }

        public Vector3 position
        {
            get { return this.transform.position; }
        }

        public float attackRange
        {
            get { return this.defaultAttackRange; }
        }

        public float scanRange
        {
            get { return this.defaultScanRange; }
        }

        public bool canCommunicate
        {
            get { return this.defaultCanCommunicate; }
        }

        public abstract EntityType type
        {
            get;
        }

        public Vector3? moveTarget
        {
            get;
            set;
        }

        public IEntity attackTarget
        {
            get;
            set;
        }

        #endregion IEntity properties

        private void Awake()
        {
            _patrolPoints = new List<Vector3>(3);
            _context = new AIContext(this);
            _navMeshAgent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
        }

        protected void Start()
        {
            this.name = string.Concat(this.type, " ", this.transform.parent != null ? this.transform.parent.childCount - 1 : 0);
            this.currentHealth = this.maxHealth;
            _currentAmmo = this.maxAmmo;

            if (this.defaultPatrolPoints.Length > 0)
            {
                this.currentPatrolIndex = 0;
                for (int i = 0; i < this.defaultPatrolPoints.Length; i++)
                {
                    _patrolPoints.Add(this.defaultPatrolPoints[i].position);
                }
            }
            // Register this game object and entity so that others can identify it as part of the scanning
            EntityManager.instance.Register(this.gameObject, this);
        }

        private void Update()
        {
            if (this.isDead)
            {
                this.gameObject.SetActive(false);
            }
        }

        protected void OnDisable()
        {
            // Unregister this game object and entity so that it will no longer be scanned by other entities
            EntityManager.instance.Unregister(this.gameObject);

            if (_bulletParticle != null)
            {
                Destroy(_bulletParticle.gameObject, 0.1f);
            }
        }

        public void MoveTo(Vector3 destination)
        {
            if ((destination - this.transform.position).sqrMagnitude < 1f)
            {
                // destination is not far enough away to warrant a move
                return;
            }

            UnityEngine.AI.NavMeshHit hit;
            int mask = _navMeshAgent.areaMask;

            if (UnityEngine.AI.NavMesh.SamplePosition(destination, out hit, 1f, mask))
            {
                _navMeshAgent.SetDestination(hit.position);
            }
        }

        public void Reload()
        {
            _currentAmmo = this.maxAmmo;
        }

        public void FireAt(IEntity target)
        {
            if (target == null || target.isDead)
            {
                return;
            }

            if (--_currentAmmo < 0)
            {
                return;
            }

            var damage = Random.Range(this.minDamage, this.maxDamage);
            target.currentHealth -= damage;

            _bulletParticle.transform.position = this.transform.position;
            _bulletParticle.transform.rotation = this.transform.rotation;
            _bulletParticle.Emit(1);
        }

        public void ReceiveCommunicatedMemory(IList<Observation> observations)
        {
            var count = observations.Count;
            for (int i = 0; i < count; i++)
            {
                if (object.ReferenceEquals(observations[i].entity, this))
                {
                    // don't store observation of "self"
                    continue;
                }

                // make new observation so that isVisible can be set to false (because this entity is not actaully seeing the other one)
                var newObs = new Observation(observations[i], false);
                _context.memory.AddOrUpdateObservation(newObs, true);
            }
        }

        public IAIContext GetContext(System.Guid id)
        {
            return _context;
        }
    }
}