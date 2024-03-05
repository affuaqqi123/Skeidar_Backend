using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocalizationController : Controller
    {
        private readonly IStringLocalizer<LocalizationController> _localizer;

        public LocalizationController(IStringLocalizer<LocalizationController> localizer)
        {
            _localizer = localizer;
        }



        //    [HttpGet("{key}")]
        //    public IActionResult GetLocalizedString(string key)
        //    {
        //        CultureInfo.CurrentCulture = new CultureInfo("nb-NO");
        //        CultureInfo.CurrentUICulture = new CultureInfo("nb-NO");
        //        var localizedString = _localizer[key]?.Value; // Retrieve the localized string value
        //        if (localizedString == null)
        //        {
        //            return NotFound(); // Return a 404 Not Found if the localized string is not found
        //        }
        //        return Ok(localizedString);
        //    }
        //}


        //[HttpGet("{culture}")]
        //public IActionResult GetLocalizedString(string culture)
        //{
        //        var localizedString = _localizer[key].Value;    
        //        return Ok(localizedString);
        //    }

        [HttpGet("{culture}")]
        public IActionResult GetLocalizedString(string culture)
        {
            var localizedStrings = _localizer.GetAllStrings(includeParentCultures: true)
                .ToDictionary(x => x.Name, x => x.Value);

            // Filter localized strings for the specified culture
            //var cultureSpecificLocalizedStrings = localizedStrings
            //    .Where(kvp => kvp.Key.EndsWith($":{culture}", StringComparison.OrdinalIgnoreCase))
            //    .ToDictionary(kvp => kvp.Key.Substring(0, kvp.Key.Length - culture.Length - 1), kvp => kvp.Value);

            var cultureSpecificLocalizedStrings = _localizer.GetAllStrings(includeParentCultures: true)
    .Where(x => x.Name.EndsWith($":{culture}", StringComparison.OrdinalIgnoreCase))
    .ToDictionary(x => x.Name.Substring(0, x.Name.Length - culture.Length - 1), x => x.Value);

            return Ok(cultureSpecificLocalizedStrings);
        }
    }
}