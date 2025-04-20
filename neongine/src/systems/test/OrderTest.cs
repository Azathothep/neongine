using System;
using neon;

namespace neongine {
    public class ASystem : IUpdateSystem
    {
        public void Update(TimeSpan span) {}
    }

    [Order(OrderType.After, typeof(ASystem))][Order(OrderType.After, typeof(ESystem))]
    public class BSystem : IUpdateSystem
    {
        public void Update(TimeSpan span) {}
    }

    [Order(OrderType.After, typeof(BSystem))]
    public class CSystem : IUpdateSystem
    {
        public void Update(TimeSpan span) {}
    }

    [Order(OrderType.After, typeof(CSystem))]
    public class DSystem : IUpdateSystem
    {
        public void Update(TimeSpan span) {}
    }

    [Order(OrderType.After, typeof(ASystem))]
    public class ESystem : IUpdateSystem
    {
        public void Update(TimeSpan span) {}
    }
}

