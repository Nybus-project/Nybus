using System;
using Nybus;

namespace Messages
{
    public class ItemProduced : IEvent
    {
        public Guid ItemId { get; set; }

        public float Quantity { get; set; }
    }
}