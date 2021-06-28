using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SampleMvcApp.Controllers
{
    public class StatusCodeController : Controller
    {
        public IActionResult Index(string code)
        {
            var originalUrl = GetOriginalUrl();

            if (!int.TryParse(code, out int statusCode)) return View(nameof(NotFound), originalUrl);

            switch ((HttpStatusCode)statusCode)
            {
                case HttpStatusCode.Forbidden:
                    return View(nameof(AccessDenied), originalUrl);
                case HttpStatusCode.NotFound:
                default:
                    return View(nameof(NotFound), originalUrl);
            }
        }

        public IActionResult NotFound(string url)
        {
            return View(url);
        }

        public IActionResult AccessDenied(string url)
        {
            return View(url);
        }

        private string GetOriginalUrl()
        {
            var originalUrl = string.Empty;
            var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            if (statusCodeReExecuteFeature != null)
            {
                originalUrl =
                    statusCodeReExecuteFeature.OriginalPathBase
                    + statusCodeReExecuteFeature.OriginalPath
                    + statusCodeReExecuteFeature.OriginalQueryString;
            }

            return originalUrl;
        }
    }
}
