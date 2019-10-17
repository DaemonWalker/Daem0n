using Daem0n.StKIoc;
using Daem0n.Test.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daem0n.Test.WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private IFoo foo;
        private IServiceProvider serviceProvider;
        public TestController(IFoo foo, IServiceProvider serviceProvider)
        {
            this.foo = foo;
            this.serviceProvider = serviceProvider;
        }
        [HttpGet("[action]")]
        public List<string> GetValue()
        {
            var tempFoo = serviceProvider.GetService(typeof(IFoo)) as IFoo;
            return new List<string>() { this.foo.ID, tempFoo.ID };
        }

        [HttpGet]
        [Route("[action]")]
        public List<ObjectInfo> MemoryMonitor()
        {
            return (serviceProvider.GetService(typeof(IStKProviderMonitor)) as IStKProviderMonitor).GetObjectInfos();
        }
    }
}
