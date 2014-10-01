using System.Text.RegularExpressions;

namespace PointW.WebApi.ResourceModel
{
    public class Link
    {
        // NOTE: the sec. #'s refer to sections of the HAL spec.  These names
        // are pretty generic (except maybe HrefLang) so appropriate to use
        // for a generic resource model of a Link.

        // TODO: should examine other media types to extract other generic names, or otherwise refine this model.

        public string Href { get; set; } // sec. 5.1
        public string Name { get; set;  } // sec 5.5
        public string Title { get; set; } // sec 5.7
        public string Profile { get; set; } // sec 5.6, RFC 6906
        public string Type { get; set; } // sec 5.3

        public string HrefLang { get; set; } // sec 5.8

        public bool IsDeprecated { get; set; } // sec 5.4 "deprecated" property
        public bool IsTemplated // sec 5.2 "templated" property
        {
            get { return !string.IsNullOrEmpty(Href) && IsTemplatedRegex.IsMatch(Href); }
        }        



        private static readonly Regex IsTemplatedRegex = new Regex(@"{.+}", RegexOptions.Compiled);
    }
}