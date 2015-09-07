using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace WebProducer.Hubs
{
    [HubName("notificationHub")]
    public class NotificationHub : Hub
    {

    }
}