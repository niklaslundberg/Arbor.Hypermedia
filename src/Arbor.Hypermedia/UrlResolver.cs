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

        public Uri GetUrl(EntityMetadata metadata)
        {
            object? GetRouteValues()
            {
                if (string.IsNullOrWhiteSpace(metadata.RouteParameterName))
                {
                    return null;
                }

                var routeValues = new ExpandoObject();

                IDictionary<string, object?> dictionary = routeValues;
                
                dictionary.Add(metadata.RouteParameterName, metadata.Entity.Context.Id);

                return routeValues;
            }

            var context = new UrlRouteContext {RouteName = metadata.RouteName, Values = GetRouteValues()};

            string? uriString = _urlHelper.RouteUrl(context);

            if (string.IsNullOrWhiteSpace(uriString))
            {
                throw new InvalidOperationException($"A route url could not be found for entity meta {metadata}");
            }

            var uri = new Uri(uriString, UriKind.RelativeOrAbsolute);

            return uri;
        }
    }
}