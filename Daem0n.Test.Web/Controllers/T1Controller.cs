using Daem0n.Test.Web.Models;
using Microsoft.AspNetCore.Http;
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
        IModel model2;
        IEnumerable<IModel> models;
        public T1Controller(IModel model, IServiceProvider serviceProvider)
        {
            this.model = model;
            this.models = models;
            model2 = Task.Run(() => serviceProvider.GetService(typeof(IModel))).Result as IModel;
        }

        [HttpGet]
        [Route("GetInfo")]
        public List<int> GetInfo()
        {
            return new List<int>() { models.Count() };
        }
    }
}
