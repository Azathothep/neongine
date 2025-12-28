using System;
using neon;

namespace neongine {
    public class ASystem : IGameUpdateSystem
    {
        public void Update(TimeSpan span) {}
    }

    [Order(OrderType.After, typeof(ASystem))][Order(OrderType.After, typeof(ESystem))]
    public class BSystem : IGameUpdateSystem
    {
        public void Update(TimeSpan span) {}
    }

    [Order(OrderType.After, typeof(BSystem))]
    public class CSystem : IGameUpdateSystem
    {
        public void Update(TimeSpan span) {}
    }

    [Order(OrderType.After, typeof(CSystem))]
    public class DSystem : IGameUpdateSystem
    {
        public void Update(TimeSpan span) {}
    }

    [Order(OrderType.After, typeof(ASystem))]
    public class ESystem : IGameUpdateSystem
    {
        public void Update(TimeSpan span) {}
    }
}

