using Daem0n.Test.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daem0n.Test.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class T1Controller : ControllerBase
    {
        IModel model;
        public T1Controller(IModel model)
        {
            this.model = model;
        }

        [HttpGet]
        [Route("GetInfo")]
        public List<int> GetInfo()
        {
            return new List<int>() { numbers.GetHashCode(), names.GetHashCode() };
        }
    }
}
