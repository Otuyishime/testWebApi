﻿using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace testWebAPI.Models
{
    /*
     *  The Ion Specification required links to be presented like this:
        {
            "href": "https:// absolute-uri-to/route",
            "methode": "GET",
            "rel": ["collection"]
        }
     */
    public class Link
    {
        public const string GetMethod = "GET";
        public const string PostMethod = "POST";
        public const string DeleteMethod = "DELETE";

        [JsonProperty(Order = -4)]
        public string Href { get; set; }

        [JsonProperty(Order = -3, NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(GetMethod)]
        public string Method { get; set; }

        [JsonProperty(Order = -2, PropertyName = "rel", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Relations { get; set; }

        // Store the route properties before being rewritten
        [JsonIgnore]
        public string RouteName { get; set; }

        [JsonIgnore]
        public object RouteValues { get; set; }

        public static Link To(string routeName, object routeValues = null)
            => new Link { 
                RouteName = routeName,
                RouteValues = routeValues,
                Method = GetMethod,
                Relations = null
            };

        public static Link ToCollection(string routeName, object routeValues = null)
            => new Link
            {
                RouteName = routeName,
                RouteValues = routeValues,
                Method = GetMethod,
                Relations = new string[] { "collection" }
            };

        public static Link ToForm(
            string routeName,
            object routeValues = null,
            string method = PostMethod,
            params string[] relations)
            => new Link
            {
                RouteName = routeName,
                RouteValues = routeValues,
                Method = method,
                Relations = relations
            };
    }
}
