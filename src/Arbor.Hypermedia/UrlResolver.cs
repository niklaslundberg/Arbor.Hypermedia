using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Arbor.Hypermedia
{
    public class UrlResolver : IUrlResolver
    {
        private readonly IUrlHelper _urlHelper;

        public UrlResolver(IUrlHelper urlHelper) => _urlHelper = urlHelper;

        public Uri GetUrl(EntityMetadata entityMetadata)
        {
            var routeValues = new ExpandoObject();

            IDictionary<string, object?> dictionary = routeValues;

            dictionary.Add(entityMetadata.RouteParameterName, entityMetadata.Entity.Context.Id);

            var context = new UrlRouteContext {RouteName = entityMetadata.RouteName, Values = routeValues};

            string? uriString = _urlHelper.RouteUrl(context);

            if (string.IsNullOrWhiteSpace(uriString))
            {
                throw new InvalidOperationException($"A route url could not be found for entity meta {entityMetadata}");
            }

            var uri = new Uri(uriString, UriKind.RelativeOrAbsolute);

            return uri;
        }
    }
}