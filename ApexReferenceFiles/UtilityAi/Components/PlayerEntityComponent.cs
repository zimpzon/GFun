#pragma warning disable 1591

namespace Apex.Examples.AI.Game
{
    using Helpers;
    using UnityEngine;

    public sealed class PlayerEntityComponent : MonoBehaviour, IEntity
    {
        public ParticleSystem bulletParticleSystem;

        public float moveSpeed = 10f;
        public float sprintMoveSpeed = 20f;
        public float rotateSpeed = 40f;
        public float sprintRotateSpeed = 60f;

        public float playerMinimumDamage = 1f;
        public float playerMaximumDamage = 10f;
        public float playerAttackRange = 16f;
        public float fireSphereCastRadius = 0.1f;

        public float playerMaxHealth = 100f;

        public KeyCode customFireKey = KeyCode.Return;

        private ParticleSystem _bulletParticle;

        #region IEntity properties

        public float currentHealth
        {
            get;
            set;
        }

        public float maxHealth
        {
            get { return this.playerMaxHealth; }
        }

        public bool isDead
        {
            get { return this.currentHealth <= 0f; }
        }

        public Vector3 position
        {
            get { return this.transform.position; }
        }

        public EntityType type
        {
            get { return EntityType.Player; }
        }

        public float attackRange
        {
            get { return this.playerAttackRange; }
        }

        public float minDamage
        {
            get { return this.playerMinimumDamage; }
        }

        public float maxDamage
        {
            get { return this.playerMaximumDamage; }
        }

        #endregion IEntity properties

        private void Awake()
        {
            this.currentHealth = this.playerMaxHealth;
            EntityManager.instance.Register(this.gameObject, this);

            _bulletParticle = Instantiate(this.bulletParticleSystem, this.transform.position, this.transform.rotation) as ParticleSystem;
            _bulletParticle.name = string.Concat(this.name, " (Bullet Particle System)");
            _bulletParticle.transform.SetParent(GameObject.Find("ParticlesContainer").transform);
        }

        private void Update()
        {
            if (this.isDead)
            {
                this.gameObject.SetActive(false);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(this.customFireKey))
            {
                Fire();
            }
        }

        private void FixedUpdate()
        {
            Move();
            Rotate();
        }

        private void OnDisable()
        {
            EntityManager.instance.Unregister(this.gameObject);

            if (_bulletParticle != null)
            {
                Destroy(_bulletParticle.gameObject, 0.1f);
            }
        }

        private void Fire()
        {
            // always instantiate the particle system for visual effect
            _bulletParticle.transform.position = this.transform.position;
            _bulletParticle.transform.rotation = this.transform.rotation;
            _bulletParticle.Emit(1);

            RaycastHit hit;
            if (!Physics.SphereCast(this.transform.position + this.transform.forward, this.fireSphereCastRadius, this.transform.forward, out hit, this.attackRange, LayersManagerComponent.instance.unitsLayer))
            {
                // Ray did not hit any units within attack range
                return;
            }

            var target = EntityManager.instance.GetEntity(hit.transform.gameObject);
            if (target == null)
            {
                // no valid registered target hit
                return;
            }

            // hit target: apply damage
            FireAt(target);
        }

        private void Rotate()
        {
            var rotation = Vector3.zero;

            // Rotate right & left
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                rotation = -Vector3.up;
            }
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                rotation = Vector3.up;
            }

            if (rotation.sqrMagnitude > 0f)
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    this.transform.eulerAngles += rotation * this.sprintRotateSpeed * Time.fixedDeltaTime;
                }
                else
                {
                    this.transform.eulerAngles += rotation * this.rotateSpeed * Time.fixedDeltaTime;
                }
            }
        }

        private void Move()
        {
            var speed = Vector3.zero;

            // Forward & Backwards
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                speed = this.transform.forward;
            }
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                speed = -this.transform.forward;
            }

            // Strafe Right & Left
            if (Input.GetKey(KeyCode.Q))
            {
                speed += -this.transform.right;
            }
            else if (Input.GetKey(KeyCode.E))
            {
                speed += this.transform.right;
            }

            if (speed.sqrMagnitude > 0f)
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    this.transform.position += speed * this.sprintMoveSpeed * Time.fixedDeltaTime;
                }
                else
                {
                    this.transform.position += speed * this.moveSpeed * Time.fixedDeltaTime;
                }
            }
        }

        #region IEntity methods

        public void FireAt(IEntity target)
        {
            var damage = UnityEngine.Random.Range(this.minDamage, this.maxDamage);
            target.currentHealth -= damage;
        }

        #endregion IEntity methods
    }
}