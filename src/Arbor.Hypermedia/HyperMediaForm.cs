using System;
using System.Collections.Generic;

namespace Arbor.Hypermedia
{
    public class HyperMediaForm : IHyperMediaControl
    {
        public HyperMediaForm(IEnumerable<HyperMediaFormField> formFields, CustomHttpMethod httpMethod, Uri href, LinkRelation linkRelation)
        {
            FormFields = formFields;
            HttpMethod = httpMethod;
            Href = href;
            LinkRelation = linkRelation;
        }

        public IEnumerable<HyperMediaFormField> FormFields { get; }

        public CustomHttpMethod HttpMethod { get; }

        public Uri Href { get; }

        public LinkRelation LinkRelation { get; }
    }
}