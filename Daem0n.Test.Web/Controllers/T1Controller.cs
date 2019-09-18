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
        private IConfiguration configuration { get; }
        private IList<int> numbers;
        private IList<string> names;
        public T1Controller(IConfiguration configuration, IList<int> numbers, IList<string> names)
        {
            this.configuration = configuration;
            this.numbers = numbers;
            this.names = names;
        }

        [HttpGet]
        [Route("GetInfo")]
        public List<int> GetInfo()
        {
            return new List<int>() { numbers.GetHashCode(), names.GetHashCode() };
        }
    }
}
