using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Messages;
using Nybus;

namespace WebProducer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBus _bus;

        public HomeController(IBus bus)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            _bus = bus;
        }

        [HttpGet]
        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("post-value")]
        public async Task<ActionResult> PostValue(string value)
        {
            await _bus.InvokeCommand(new ReverseStringCommand
            {
                Value = value
            });

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("ajax-post-value")]
        public async Task<ActionResult> AjaxPostValue(string value)
        {
            await _bus.InvokeCommand(new ReverseStringCommand
            {
                Value = value
            });

            return Content("OK");
        }
    }
}