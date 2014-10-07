using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;
using PointW.WebApi.ResourceModel;
using PointW.WebApi.ResourceModel.TestResources;

namespace PointW.WebApi.MediaTypeFormatters.CollectionJson
{
    internal class ResourceListValueProvider : IValueProvider
    {
        public void SetValue(object target, object value)
        {
            throw new NotImplementedException();
        }

        public object GetValue(object target)
        {
            var rtn = new Dictionary<string, object>();

            string selfHref;
            var linkCollection = ProcessLinks(target, rtn, out selfHref);

            if (!string.IsNullOrEmpty(selfHref))
            {
                rtn.Add("href", selfHref);
            }

            var items = GetItems(target, selfHref, linkCollection);
            rtn.Add("items", items);

            return rtn;
        }



        private static LinkCollection ProcessLinks(object target, IDictionary<string, object> rtn, out string selfHref)
        {
            var links = target.GetType()
                .GetProperties().FirstOrDefault(p => p.Name == "Relations");

            LinkCollection linkCollection = null;
            selfHref = "";
            if (links != null)
            {
                linkCollection = links.GetValue(target) as LinkCollection;
                if (linkCollection != null)
                {
                    var collectionLink = linkCollection.FirstOrDefault(lc => lc.Key == "collection");  // TODO: review this (copied from Resource to ResourceList)
                    if (collectionLink.Value != null)
                    {
                        rtn.Add("href", collectionLink.Value.Href);
                    }

                    var selfLink = linkCollection.FirstOrDefault(lc => lc.Key == "self");
                    if (selfLink.Value != null)
                    {
                        selfHref = selfLink.Value.Href;
                        linkCollection.Remove("self");
                    }
                }
            }
            return linkCollection;
        }



        private static List<object> GetItems(object target, string selfHref, LinkCollection linkCollection)
        {
            var list = target as IResourceList;

            // ASSERT: list != null

            return list.Items.Select(CollectData).Cast<object>().ToList();
        }


        private static Dictionary<string, object> CollectData(object target)
        {
            var item = new Dictionary<string, object>();

            var selfHref = "";
            var lc = ProcessLinks(target, item, out selfHref);

            if (!string.IsNullOrEmpty(selfHref))
            {
                item.Add("href", selfHref);
            }


            // var props = target.GetType()
            //     .GetProperties().Where(p => p.Name != "Relations");
            // 
            // 
            // item.Add("data", props.Select(p => new
            //     {
            //         name = Utilities.ToCamelCase(p.Name),
            //         value = p.GetValue(target)
            //     }).ToList()
            // );

            // TODO: refactor with dupe logic in ResourceValueProvider
            var props = target.GetType()
                .GetProperties().Where(p => p.Name != "Relations" && !p.GetCustomAttributes(typeof(NeverShowAttribute), true).Any());

            item.Add("data", props.Select(p => new
                {
                    name = Utilities.ToCamelCase(p.Name),
                    value = p.GetValue(target) ?? (p.GetCustomAttributes(typeof(AlwaysShowAttribute), true).Any() ? null : "~~skip~~")
                }).ToList().Where(p => p.value as string != "~~skip~~") // TODO: this smells - improve!
            );


            if (lc.Any())
            {
                item.Add("links", lc);
            }

            return item;
        }





    }
}