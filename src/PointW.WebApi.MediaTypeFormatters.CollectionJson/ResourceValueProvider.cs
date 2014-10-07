using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;
using PointW.WebApi.ResourceModel;

namespace PointW.WebApi.MediaTypeFormatters.CollectionJson
{
    internal class ResourceValueProvider : IValueProvider
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


            var items = GetItems(target, selfHref, linkCollection);
            rtn.Add("items", items);

            return rtn;
        }



        private static List<Dictionary<string, object>> GetItems(object target, string selfHref, LinkCollection linkCollection)
        {
            var items = new List<Dictionary<string, object>>();

            var item = CollectData(target);


            HandleItemLinks(selfHref, linkCollection, item);

            items.Add(item);
            return items;
        }



        private static void HandleItemLinks(string selfHref, LinkCollection linkCollection, Dictionary<string, object> item)
        {
            if (!string.IsNullOrEmpty(selfHref))
            {
                item.Add("href", selfHref);
            }


            if (linkCollection != null && linkCollection.Any())
            {
                linkCollection.Remove("collection");
                linkCollection.Remove("self");
                item.Add("links", linkCollection);
            }
        }



        private static Dictionary<string, object> CollectData(object target)
        {
            var props = target.GetType()
                .GetProperties().Where(p => p.Name != "Relations" && !p.GetCustomAttributes(typeof(NeverShowAttribute), true).Any());

            var item = new Dictionary<string, object>
            {
                {
                    "data", props.Select(p => new
                    {
                        name = Utilities.ToCamelCase(p.Name),
                        value = p.GetValue(target) ?? (p.GetCustomAttributes(typeof(AlwaysShowAttribute), true).Any() ? null : "~~skip~~")
                    }).ToList().Where(p => p.value as string != "~~skip~~") // TODO: this smells - improve!
                }
            };
            return item;
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
                    var collectionLink = linkCollection.FirstOrDefault(lc => lc.Key == "collection");
                    if (collectionLink.Value != null)
                    {
                        rtn.Add("href", collectionLink.Value.Href);
                    }

                    var selfLink = linkCollection.FirstOrDefault(lc => lc.Key == "self");
                    if (selfLink.Value != null)
                    {
                        selfHref = selfLink.Value.Href;
                    }
                }
            }
            return linkCollection;
        }
    }
}