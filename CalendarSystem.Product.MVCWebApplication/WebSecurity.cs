using CalendarSystem.Model.Membership;
using System;
using System.Security;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;

namespace CalendarSystem.Product.MVCWebApplication
{
    public class WebSecurity
    {
        private const string CookieName = "CalendarSystemClaim";

        public static void SetClaim(Claim claim, DateTimeOffset expiration)
        {
            var serialized = JsonConvert.SerializeObject(claim);
            var ticket = new FormsAuthenticationTicket
                (1, "claim", DateTime.UtcNow, expiration.UtcDateTime, isPersistent: true, userData: serialized);
            var cookieText = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(CookieName, cookieText);
            HttpContext.Current.Response.SetCookie(cookie);
        }

        public static Claim GetClaim()
        {
            var cookie = HttpContext.Current.Request.Cookies[CookieName];
            if (cookie == null) return null;
            var decrypted = FormsAuthentication.Decrypt(cookie.Value);
            if (decrypted == null) return null;
            return JsonConvert.DeserializeObject<Claim>(decrypted.UserData);
        }
    }
}