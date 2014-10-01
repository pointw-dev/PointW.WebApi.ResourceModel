using System.Collections.Generic;

namespace PointW.WebApi.ResourceModel
{
    public class LinkCollection : Dictionary<string, Link>
    {
        private readonly List<Link> _qualifiers;

        public List<Link> Qualifiers
        {
            get { return _qualifiers; }
        }

        public LinkCollection()
        {
            _qualifiers = new List<Link>();
        }

        public void AddQualifier(string name, string href)
        {
            _qualifiers.Add(new Link{ Name = name, Href = href});
        }

        // TODO: remove? other manips?
    }
}