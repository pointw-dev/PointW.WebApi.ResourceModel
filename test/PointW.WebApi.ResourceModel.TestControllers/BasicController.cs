using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using PointW.WebApi.ResourceModel.TestResources;

namespace PointW.WebApi.ResourceModel.TestControllers
{
    [RoutePrefix("api/basic")]
    public class BasicController : ApiController
    {
        private SimpleResourceList<ResourceWithHiddenId> _fakeDatabase;

        public BasicController()
        {
            _fakeDatabase = new SimpleResourceList<ResourceWithHiddenId>
            {
                Items = new List<ResourceWithHiddenId>
                {
                    new ResourceWithHiddenId {Id = 1, Name = "alpha"},
                    new ResourceWithHiddenId {Id = 2, Name = "beta"},
                    new ResourceWithHiddenId {Id = 3, Name = "gamma"}
                }
            };
        }

        [Route("", Name = "GetAll")]
        public IHttpActionResult GetAllProducts()
        {
            _fakeDatabase.Relations.Add("self", new Link { Href = Url.Link("GetAll", null) });

            return Ok(_fakeDatabase);
        }



        [Route("{productId:int}", Name = "GetById")]
        public IHttpActionResult GetProduct(int id)
        {
            var rtn = _fakeDatabase.Items.FirstOrDefault(i => i.Id == id);


            if (rtn == null)
            {
                return NotFound();
            }

            var href = Url.Link("GetById", new { productId = id });
            rtn.Relations.Add("self", new Link { Href = href });

            return Ok(rtn);
        }



        [Route("", Name = "Post")]
        public IHttpActionResult Post([FromBody] ProductResource product)
        {
            var newId = _fakeDatabase.Items.Max(i => i.Id) + 1;

            var r = new ResourceWithHiddenId
            {
                Id = newId,
                Name = product.Model
            };

            _fakeDatabase.Items.Add(r);

            return Created(Url.Link("GetById", new {productId = newId}), r);
        }        
    }
}
