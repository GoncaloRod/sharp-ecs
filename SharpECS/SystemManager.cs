using System.Collections.Generic;

namespace SharpECS
{
    public class SystemManager
    {
        private List<System> _systems;

        public SystemManager(IEnumerable<System> systems)
        {
            _systems.AddRange(systems);
        }
    }
}