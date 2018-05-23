using System;
using System.Linq;
using System.Threading.Tasks;
using Boa.Sample.Data;
using Boa.Sample.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Options;

namespace Boa.Sample.Controllers
{
    public class HomeController : Controller
    {
        private readonly BoaIntegrationDbContext _context;
        private readonly BoaOptions _boaOptions;

        public HomeController(BoaIntegrationDbContext context,
            IOptions<BoaOptions> boaOptions)
        {
            _context = context;
            _boaOptions = boaOptions.Value;
        }

        public IActionResult ChangeLanguage(BaseViewModel vm)
        {

            var user = _context.Users.FirstOrDefault(x => x.UserName == User.Identity.Name);
            if (user != null)
            {
                user.LanguageCode = vm.LanguageCode;
                _context.SaveChanges();
            }
            else
            {
                Response.Cookies.Append("lang", vm.LanguageCode);
            }

            return Redirect("/");
        }

        public IActionResult Index()
        {
            var user = _context.Users.FirstOrDefault(x => x.UserName == User.Identity.Name);
            return base.View("BoaIntegration", new BoaIntegrationModel
            {
                BoaUrl = _boaOptions.BoaUrl,
                User = user,
                LanguageCode = GetLanguage(user)
            });
        }

        private string GetLanguage(User user)
        {
            return user?.LanguageCode ?? Request.Cookies["lang"] ?? BoaController.EN;
        }

        public IActionResult BoaIntegration()
        {
            var user = _context.Users.FirstOrDefault(x => x.UserName == User.Identity.Name);
            return View(new BoaIntegrationModel
            {
                BoaUrl = _boaOptions.BoaUrl,
                User = user,
                LanguageCode = GetLanguage(user)
            });
        }
    }

    public class BoaIntegrationModel : BaseViewModel
    {
        public User User { get; set; }
        public string BoaUrl { get; set; }
    }
}