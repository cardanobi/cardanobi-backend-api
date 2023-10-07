using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OData.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiCore.Controllers
{
    public class CustomEnableQueryAttribute : EnableQueryAttribute
    {
        public string AdditionalQuery { get; set; }

        public CustomEnableQueryAttribute(string additionalQuery)
        {
            AdditionalQuery = additionalQuery;
        }

        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var request = actionContext.HttpContext.Request;

            var additionalQueryParams = ParseQueryString(AdditionalQuery);
            var userQueryParams = ParseQueryString(request.QueryString.Value);

            // Merge the dictionaries, with user-provided values taking precedence.
            var mergedQueryParams = new Dictionary<string, string>(additionalQueryParams);
            foreach (var kvp in userQueryParams)
            {
                mergedQueryParams[kvp.Key] = kvp.Value;
            }

            // Construct the new query string.
            var newQueryString = "?" + string.Join("&", mergedQueryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            actionContext.HttpContext.Request.QueryString = new QueryString(newQueryString);

            base.OnActionExecuting(actionContext);
        }

        private Dictionary<string, string> ParseQueryString(string queryString)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(queryString))
            {
                return result;
            }

            foreach (var param in queryString.TrimStart('?').Split('&'))
            {
                var parts = param.Split('=');
                if (parts.Length == 2)
                {
                    result[parts[0]] = parts[1];
                }
            }

            return result;
        }
    }
}
