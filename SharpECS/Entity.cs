using System;
using System.Collections.Generic;
using System.Linq;

using SharpECS.Exceptions;

namespace SharpECS
{
    /// <summary>
    /// Represents one of your game's Entities.
    /// An Entity can hold many <see cref="IComponent"/>.
    /// </summary>
    public sealed class Entity
    {
        #region Events

        /// <summary>
        /// Called when a <see cref="IComponent"/> is added to this Entity.
        /// </summary>
        public event Action<Entity, IComponent> ComponentAdded;

        /// <summary>
        /// Called when a <see cref="IComponent"/> is removed from this Entity.
        /// </summary>
        public event Action<Entity, IComponent> ComponentRemoved;

        #endregion

        #region Contructors

        /// <summary>
        /// Creates an instance of Entity.
        /// </summary>
        /// <param name="id">String to identify this Entity.</param>
        /// <param name="pool"><see cref="EntityPool"/> that owns this Entity.</param>
        public Entity(string id, EntityPool pool)
        {
            Id = id;

            OwnerPool = pool ?? throw new IndependentEntityException(this);

            Components = new List<IComponent>();
            Children = new List<Entity>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// String that identifies this Entity.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The pool which this Entity resides in.
        /// </summary>
        public EntityPool OwnerPool { get; private set; }

        /// <summary>
        /// A list of this Entity's components.
        /// </summary>
        public List<IComponent> Components { get; private set; }

        /// <summary>
        /// A list of this Entity's children Entities.
        /// </summary>
        public List<Entity> Children { get; private set; }

        /// <summary>
        /// The Entity which this Entity is a child of.
        /// Is null if this Entity is root.
        /// </summary>
        public Entity Parent { get; private set; }

        /// <summary>
        /// Walks up all the parents of this Entity and returns the top one.
        /// </summary>
        public Entity RootEntity
        {
            get
            {
                if (Parent == null)
                    return this;

                Entity parent = Parent;

                while (parent != null)
                {
                    if (parent.Parent == null)
                        return parent;

                    parent = parent.Parent;
                }

                return this;
            }
        }

        public bool Active { get; set; } = true;

        #endregion

        #region Methods

        /// <summary>
        /// AddEntity new component this Entity's list of components.
        /// </summary>
        /// <param name="component">Component to add.</param>
        /// <returns>Reference to the added component.</returns>
        /// <exception cref="ComponentAlreadyExistsException"></exception>
        private IComponent AddComponent(IComponent component)
        {
            if (HasComponent(component.GetType()))
                throw new ComponentAlreadyExistsException(this);

            Components.Add(component);
            ComponentAdded?.Invoke(this, component);
            OwnerPool.ComponentAdded(this);

            return component;
        }

        /// <summary>
        /// AddEntity a group of components to this entity.
        /// </summary>
        /// <param name="components">Group of components to add.</param>
        public void AddComponents(IEnumerable<IComponent> components)
        {
            foreach (var c in components)
                AddComponent(c);
        }

        /// <summary>
        /// AddEntity a group of components to this entity.
        /// </summary>
        /// <param name="components">Group of components to add.</param>
        public void AddComponents(params IComponent[] components)
        {
            foreach (var c in components)
                AddComponent(c);
        }

        /// <summary>
        /// RemoveEntity a component of type T from from this entity.
        /// </summary>
        /// <typeparam name="T">Type of the component to remove.</typeparam>
        /// <exception cref="ComponentNotFoundException"></exception>
        public void RemoveComponent<T>() where T : IComponent
        {
            if (!HasComponent<T>())
                throw new ComponentNotFoundException(this);

            IComponent componentToRemove = GetComponent<T>();
            Components.Remove(componentToRemove);

            ComponentRemoved?.Invoke(this, componentToRemove);
            OwnerPool.ComponentRemoved(this);
        }

        /// <summary>
        /// RemoveEntity a component of a given type from from this entity.
        /// </summary>
        /// <typeparam name="T">Type of the component to remove.</typeparam>
        /// <exception cref="ComponentNotFoundException"></exception>
        public void RemoveComponent(Type type)
        {
            if (!HasComponent(type))
                throw new ComponentNotFoundException(this);

            IComponent componentToRemove = GetComponent(type);
            Components.Remove(componentToRemove);

            ComponentRemoved?.Invoke(this, componentToRemove);
            OwnerPool.ComponentRemoved(this);
        }

        /// <summary>
        /// RemoveEntity a group of components from entity.
        /// </summary>
        /// <param name="types">Group of component types to remove.</param>
        /// <exception cref="ComponentNotFoundException"></exception>
        /// <exception cref="Exception"></exception> TODO: Custom exception
        public void RemoveComponents(params Type[] types)
        {
            foreach (Type type in types)
            {
                if (!type.IsComponent())
                    throw new Exception("One or more of the types you passed were not IComponent children.");

                if (!HasComponent(type)) throw new ComponentNotFoundException(this);

                IComponent componentToRemove = GetComponent(type);
                Components.Remove(componentToRemove);

                ComponentRemoved?.Invoke(this, componentToRemove);
                OwnerPool.ComponentRemoved(this);
            }
        }

        /// <summary>
        /// Get a component from a type T in this entity.
        /// </summary>
        /// <typeparam name="T">Type of the component to get.</typeparam>
        /// <returns>Reference to the component.</returns>
        /// <exception cref="ComponentNotFoundException"></exception>
        public T GetComponent<T>() where T : IComponent
        {
            T match = Components.OfType<T>().FirstOrDefault();

            if (match == null)
                throw new ComponentNotFoundException(this);

            return match;
        }

        /// <summary>
        /// Get a component from a a given type in this entity.
        /// </summary>
        /// <param name="componentType">Type of the component to get.</param>
        /// <returns>Reference to the component.</returns>
        /// <exception cref="ComponentNotFoundException"></exception>
        /// <exception cref="Exception"></exception> TODO: Custom exception
        public IComponent GetComponent(Type componentType)
        {
            if (!componentType.IsComponent())
                throw new Exception("One or more of the types you passed were not IComponent children.");

            IComponent match = Components.FirstOrDefault(c => c.GetType() == componentType);

            if (match == null)
                throw new ComponentNotFoundException(this);

            return match;
        }

        /// <summary>
        /// Check if this entity has a component of type T.
        /// </summary>
        /// <typeparam name="T">Type of component to check.</typeparam>
        /// <returns>True if this entity has the component.</returns>
        /// <returns>False if this entity doesn't have the component.</returns>
        public bool HasComponent<T>() where T : IComponent
        {
            return Components.Any(c => c.GetType() == typeof(T));
        }

        /// <summary>
        /// Check if this entity has a component of a given type.
        /// </summary>
        /// <param name="componentType">Type of component to check.</param>
        /// <returns>True if this entity has the component.</returns>
        /// <returns>False if this entity doesn't have the component.</returns>
        public bool HasComponent(Type componentType)
        {
            if (!componentType.IsComponent())
                throw new Exception("One or more of the types you passed were not IComponent children.");

            return Components.Any(c => c.GetType() == componentType);
        }

        /// <summary>
        /// Check if this entity has an exact group of components.
        /// </summary>
        /// <param name="types">Group of components to check</param>
        /// <returns>True if this entity has the component.</returns>
        /// <returns>False if this entity doesn't have the component.</returns>
        public bool HasComponents(IEnumerable<Type> types)
        {
            return types.All(HasComponent);
        }

        /// <summary>
        /// Check if this entity has an exact group of components.
        /// </summary>
        /// <param name="types">Group of components to check</param>
        /// <returns>True if this entity has the component.</returns>
        /// <returns>False if this entity doesn't have the component.</returns>
        public bool HasComponents(params Type[] types)
        {
            return types.All(HasComponent);
        }

        /// <summary>
        /// RemoveEntity every component in this entity.
        /// </summary>
        public void RemoveAllComponents()
        {
            foreach (IComponent component in Components)
                RemoveComponent(component.GetType());

            Components.Clear();
        }

        /// <summary>
        /// RemoveEntity every component in this entity and in child entities, clear children and owner pool.
        /// </summary>
        public void Reset()
        {
            RemoveAllComponents();

            foreach (Entity child in Children)
                OwnerPool.RemoveEntity(child);

            Children.Clear();

            OwnerPool = null;
        }

        /// <summary>
        /// Move this Entity to another <see cref="EntityPool"/>.
        /// </summary>
        /// <param name="pool">Destination <see cref="EntityPool"/>.</param>
        public void MoveTo(EntityPool pool)
        {
            if (pool == null)
                throw new NullEntityPoolException();

            pool.AddEntity(this);
            OwnerPool.RemoveEntity(this);
            OwnerPool = pool;
        }

        /// <summary>
        /// Creates and returns a new <see cref="Entity"/> as a child of this Entity.
        /// </summary>
        /// <param name="childId">String to identify new entity.</param>
        /// <param name="inheritComponents">If child entity will inherit parent's components.</param>
        /// <returns></returns>
        public Entity CreateChild(string childId, bool inheritComponents = false)
        {
            Entity child = OwnerPool.CreateEntity(childId);

            child.Parent = this;

            if (inheritComponents)
                child.AddComponents(Components);

            Children.Add(child);

            return child;
        }

        /// <summary>
        /// Adds an existing Entity as a child to this Entity.
        /// </summary>
        /// <param name="entity">Entity to add as a child.</param>
        public void AddChild(Entity entity)
        {
            entity.Parent = this;
            Children.Add(entity);
        }

        /// <summary>
        /// Get a child by his Id.
        /// </summary>
        /// <param name="childId">Id of the child to search for.</param>
        /// <returns>Reference to child if it was found or null if it wasn't.</returns>
        public Entity GetChild(string childId)
        {
            return Children.FirstOrDefault(c => c.Id == childId);
        }

        /// <summary>
        /// Returns the whole "family tree" of this Entity (all children, "grandchildren", etc.)
        /// </summary>
        public IEnumerable<Entity> FamilyTree()
        {
            // Thanks @deccer
            var childSelector = new Func<Entity, IEnumerable<Entity>>(ent => ent.Children);

            Stack<Entity> stack = new Stack<Entity>(Children);

            while (stack.Any())
            {
                Entity next = stack.Pop();
                yield return next;
                foreach (Entity child in childSelector(next))
                    stack.Push(child);
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Compare an object to this entity.
        /// </summary>
        /// <param name="obj">Object to compare.</param>
        /// <returns>True if the object is the same as this entity.</returns>
        /// <returns>False if the object is different from this entity.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Entity ent))
                return false;

            return Id.Equals(ent.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion

        #region Operators

        /// <summary>
        /// Add <see cref="IComponent"/> to <see cref="Entity"/>.
        /// </summary>
        /// <param name="entity">Entity to add the component to.</param>
        /// <param name="component">Component to add to the entity.</param>
        /// <returns>Reference to the entity.</returns>
        public static Entity operator +(Entity entity, IComponent component)
        {
            if (entity == null || component == null)
                throw new NullReferenceException();

            entity.AddComponent(component);

            return entity;
        }

        #endregion
    }
}