using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.FSharp.Core;
using CalendarSystem.Model;
using CalendarSystem.Model.Membership;
using CalendarSystem.Domain.Membership;
using CalendarSystem.Product.MVCWebApplication.ViewModels;
using Utilities;

namespace CalendarSystem.Product.MVCWebApplication.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var claim = WebSecurity.GetClaim();
            if (claim == null)
                return RedirectToAction(nameof(Login));
            var now = DateTimeOffset.UtcNow;
            var startOfMonth = new DateTimeOffset
                (new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc));
            var endOfMonth = new DateTimeOffset
                (new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 0, 0, 0, DateTimeKind.Utc));
            var duration = InclusiveRange<DateTimeOffset>.Of(startOfMonth, endOfMonth).ResultValue;
            var plan = Domain.Calendar.Implementation.Calendar.CalendarEvents.GetEvents(claim, duration);
            var events = await Standup.Standup.RunPlan(plan);
            return View(events);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel viewModel)
        {
            var email = EmailAddress.OfString(viewModel.Email).ResultValue;
            var password = InputPassword.OfString(viewModel.Password).ResultValue;
            var plan = Domain.Membership.Implementation.Membership.Authentication.Login(email, password);
            var result = await Standup.Standup.RunPlan(plan);
            if (result == null)
            {
                return RedirectToAction(nameof(Login));
            }
            else
            {
                WebSecurity.SetClaim(result.Value.Item1, result.Value.Item2);
                return RedirectToAction(nameof(Index));
            }
        }
    }
}