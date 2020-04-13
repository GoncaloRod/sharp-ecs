using System;

namespace SharpECS
{
    public abstract class DrawableSystem : System
    {
        protected DrawableSystem(EntityPool pool, params Type[] compatibleTypes) : base(pool, compatibleTypes) { }

        /// <summary>
        /// Called every frame when game wants to draw.
        /// </summary>
        /// <param name="dt">Time passed between the current and previous frame.</param>
        public virtual void OnDraw(float dt) { }
    }
}