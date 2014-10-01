using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using PointW.WebApi.MediaTypeFormatters.Hal.Tests.TestResources;
using PointW.WebApi.ResourceModel;

namespace PointW.WebApi.MediaTypeFormatters.Hal.Tests.TestControllers
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

            // var href = Url.Link("GetById", new { productId = "{productId}" });
            // var href = Url.Link("GetById", new Dictionary<string, object> {{"productId", "{productId}"}});
            // _fakeDatabase.Relations.Add("product", new Link { Href = href} );

            return Ok(_fakeDatabase);
        }


        
        [Route("{productId:int}", Name="GetById")]
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
    }
}