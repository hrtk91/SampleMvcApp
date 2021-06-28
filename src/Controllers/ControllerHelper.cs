using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace SampleMvcApp.Controllers
{
    public class ControllerHelper
    {
        public static string NameOf<T>() where T : Controller
        {
            var name = typeof(T).Name;
            return name.EndsWith("Controller") ? Regex.Replace(name, "Controller$", "") : name;
        }
    }
}
