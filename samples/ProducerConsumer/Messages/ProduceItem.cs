using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nybus;

namespace Messages
{
    public class ProduceItem : ICommand
    {
        public Guid ItemId { get; set; }

        public float Quantity { get; set; }


    }

    public class ItemProduced : IEvent
    {
        public Guid ItemId { get; set; }

        public float Quantity { get; set; }
    }
}
