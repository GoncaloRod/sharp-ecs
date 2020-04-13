using System;
using System.Collections.Generic;
using System.Linq;
using SharpECS.Exceptions;

namespace SharpECS
{
    /// <summary>
    /// The object to manage all your game <see cref="Entity"/>.
    /// </summary>
    public class EntityPool
    {
        #region Events

        /// <summary>
        /// Called when an <see cref="Entity"/> is added to this pool.
        /// </summary>
        public event Action<EntityPool, Entity> OnEntityAdded;

        /// <summary>
        /// Called when an <see cref="Entity"/> is removed from this pool.
        /// </summary>
        public event Action<EntityPool, Entity> OnEntityRemoved;

        /// <summary>
        /// Called when a <see cref="IComponent"/> is added to an <see cref="Entity"/> from this pool.
        /// </summary>
        public event Action<EntityPool, Entity> OnEntityComponentAdded;

        /// <summary>
        /// Called when a <see cref="IComponent"/> is removed from an <see cref="Entity"/> from this pool.
        /// </summary>
        public event Action<EntityPool, Entity> OnEntityComponentRemoved;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of EntityPool.
        /// </summary>
        /// <param name="id">String to identify this entity pool.</param>
        public EntityPool(string id)
        {
            Id = id ?? throw new ArgumentNullException();

            Entities = new List<Entity>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// String that identifies this Entity Pool.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Entities stored in this Entity Pool.
        /// </summary>
        public List<Entity> Entities { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Create a new <see cref="Entity"/> and add it to this entity pool.
        /// </summary>
        /// <param name="id">String to identify new entity.</param>
        /// <returns>Reference to the created entity.</returns>
        public Entity CreateEntity(string id)
        {
            Entity entity = new Entity(id, this);

            Entities.Add(entity);
            OnEntityAdded?.Invoke(this, entity);

            return entity;
        }

        /// <summary>
        /// Add an existing <see cref="Entity"/> to this pool.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        public void AddEntity(Entity entity)
        {
            Entities.Add(entity);

            OnEntityAdded?.Invoke(this, entity);
        }

        /// <summary>
        /// Add a group of <see cref="Entity"/> to this pool.
        /// </summary>
        /// <param name="entities">Group of entities to add.</param>
        public void AddEntities(params Entity[] entities)
        {
            foreach (Entity entity in entities)
            {
                Entities.Add(entity);

                OnEntityAdded?.Invoke(this, entity);
            }
        }

        /// <summary>
        /// Add a group of <see cref="Entity"/> to this pool.
        /// </summary>
        /// <param name="entities">Group of entities to add.</param>
        public void AddEntities(IEnumerable<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                Entities.Add(entity);

                OnEntityAdded?.Invoke(this, entity);
            }
        }

        /// <summary>
        /// Check if an <see cref="Entity"/> with "id" is registered in this entity pool.
        /// </summary>
        /// <param name="id">Id of the entity to find.</param>
        /// <returns>True if entity is registered in this entity pool.</returns>
        /// <returns>False if entity is not registered in this entity pool.</returns>
        public bool DoesEntityExist(string id)
        {
            return !string.IsNullOrEmpty(id) && Entities.Any(ent => ent.Id == id);
        }

        /// <summary>
        /// Check if an <see cref="Entity"/> is registered in this entity pool.
        /// </summary>
        /// <param name="entity">Reference to the entity to find.</param>
        /// <returns>True if entity is registered in this entity pool.</returns>
        /// <returns>False if entity is not registered in this entity pool.</returns>
        public bool DoesEntityExist(in Entity entity)
        {
            return entity != null && !string.IsNullOrEmpty(entity.Id) && Entities.Contains(entity);
        }

        /// <summary>
        /// Get an <see cref="Entity"/> from entities list with a given "id".
        /// </summary>
        /// <param name="id">Id to search for in entities list.</param>
        /// <returns>Instance of entity if it was found.</returns>
        /// <returns>null if it was not found.</returns>
        public Entity GetEntity(string id)
        {
            return Entities.FirstOrDefault(ent => ent.Id == id);
        }

        /// <summary>
        /// Remove an <see cref="Entity"/> from this pool.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        public void RemoveEntity(Entity entity)
        {
            if (!Entities.Contains(entity))
                throw new EntityNotFoundException(this);

            Entities.Remove(entity);
            OnEntityRemoved?.Invoke(this, entity);
        }

        /// <summary>
        /// Remove a group of <see cref="Entity"/> from this pool.
        /// </summary>
        /// <param name="entities">Group of entities to remove.</param>
        public void RemoveEntities(params Entity[] entities)
        {
            foreach (Entity entity in entities)
            {
                if (!Entities.Contains(entity))
                    throw new EntityNotFoundException(this);

                Entities.Remove(entity);
                OnEntityRemoved?.Invoke(this, entity);
            }
        }

        /// <summary>
        /// Remove a group of <see cref="Entity"/> from this pool.
        /// </summary>
        /// <param name="entities">Group of entities to remove.</param>
        public void RemoveEntities(IEnumerable<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                if (!Entities.Contains(entity))
                    throw new EntityNotFoundException(this);

                Entities.Remove(entity);
                OnEntityRemoved?.Invoke(this, entity);
            }
        }

        /// <summary>
        /// Clear active entities list.
        /// </summary>
        public void WipeEntities()
        {
            Entities.Clear();
        }

        /// <summary>
        /// Trigger event when a component is added to an entity in this pool.
        /// </summary>
        /// <param name="entity">Entity that triggered the event.</param>
        internal void ComponentAdded(Entity entity)
        {
            OnEntityComponentAdded?.Invoke(this, entity);
        }

        /// <summary>
        /// Trigger event when a component is removed from an entity in this pool.
        /// </summary>
        /// <param name="entity">Entity that triggered the event.</param>
        internal void ComponentRemoved(Entity entity)
        {
            OnEntityComponentRemoved?.Invoke(this, entity);
        }

        #endregion

        #region Operators

        /// <summary>
        /// AddEntity an <see cref="Entity"/> to an <see cref="EntityPool"/>.
        /// </summary>
        /// <param name="pool">Entity pool to add the entity.</param>
        /// <param name="entity">Entity to add.</param>
        public static EntityPool operator +(EntityPool pool, in Entity entity)
        {
            pool.AddEntity(entity);

            return pool;
        }

        /// <summary>
        /// RemoveEntity an <see cref="Entity"/> from an <see cref="EntityPool"/>.
        /// </summary>
        /// <param name="pool">Entity pool to remove the entity.</param>
        /// <param name="entity">Entity to remove.</param>
        public static EntityPool operator -(EntityPool pool, Entity entity)
        {
            if (entity == null || !pool.DoesEntityExist(entity.Id))
                throw new EntityNotFoundException(pool);

            pool.RemoveEntity(entity);

            return pool;
        }

        #endregion
    }
}