using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpECS
{
    /// <summary>
    /// Represents a system and is responsible to add behavior to an <see cref="Entity"/>.
    /// </summary>
    public abstract class System
    {
        #region Properties

        /// <summary>
        /// <see cref="EntityPool"/> where for this system.
        /// </summary>
        public EntityPool Pool { get; private set; }

        /// <summary>
        /// Compatible <see cref="Entity"/> with this system.
        /// </summary>
        public List<Entity> CompatibleEntities { get; private set; }

        /// <summary>
        /// Compatible types of <see cref="IComponent"/> with this system.
        /// </summary>
        protected List<Type> CompatibleTypes { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create an instance of entity system.
        /// </summary>
        /// <param name="pool"><see cref="EntityPool"/> where this system belongs.</param>
        /// <param name="compatibleTypes">Compatible types of <see cref="IComponent"/> for this system.</param>
        protected System(EntityPool pool, params Type[] compatibleTypes)
        {
            if (compatibleTypes.Any(t => !t.IsComponent()))
                throw new Exception("Type passed into EntitySystem is not an IComponent!");

            CompatibleTypes = new List<Type>();
            CompatibleTypes.AddRange(compatibleTypes);

            Pool = pool;

            CompatibleEntities = GetCompatibleInPool();
            
            Pool.OnEntityComponentAdded += OnEntityPoolChanged;
            Pool.OnEntityComponentRemoved += OnEntityPoolChanged;

            Pool.OnEntityAdded += OnEntityPoolChanged;
            Pool.OnEntityRemoved += OnEntityPoolChanged;

            OnCreate();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add a compatible type of <see cref="IComponent"/> to this system.
        /// </summary>
        /// <param name="type">Type of component.</param>
        public void AddCompatibleType(Type type)
        {
            if (!type.IsComponent())
                throw new Exception("Type passed into AddCompatibleType is not an IComponent!");

            CompatibleTypes.Add(type);
            CompatibleEntities = GetCompatibleInPool();
        }

        /// <summary>
        /// Get every compatible <see cref="Entity"/> with this system.
        /// </summary>
        /// <returns>List with compatible entities.</returns>
        protected List<Entity> GetCompatibleInPool()
        {
            return Pool.Entities.Where(ent => ent.HasComponents(CompatibleTypes)).ToList();
        }

        /// <summary>
        /// Called when <see cref="Pool"/> changes to update <see cref="CompatibleEntities"/> list.
        /// </summary>
        private void OnEntityPoolChanged(EntityPool pool, Entity entity)
        {
            Pool = pool;
            CompatibleEntities = GetCompatibleInPool();
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Called when system is created.
        /// </summary>
        public virtual void OnCreate() { }

        /// <summary>
        /// Called every frame when game wants to update.
        /// </summary>
        /// <param name="dt">Time passed between the current and previous frame.</param>
        public virtual void OnUpdate(float dt) { }


        #endregion
    }
}
