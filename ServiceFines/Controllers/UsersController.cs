using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using ServiceFines.Models;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ServiceFines.Controllers
{
    public class UsersController : Controller
    {
        // GET: /Users/
        public ActionResult Authenticate(string redirect_uri, int client_id = 1)
        {
            ViewBag.ReturnUrl = redirect_uri;
            ViewBag.Code = client_id;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(AuthenticationModRedirect authModel)
        {
            AuthModel auth = new AuthModel();

            auth.Username = authModel.Username;
            auth.Password = authModel.Password;

            string name = "";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.PostAsJsonAsync(new Uri("http://localhost:1524/oauth/checkuser"), auth);

                    if (res.IsSuccessStatusCode)
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        name = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(Response);
                    }
                    else
                    {
                        ViewBag.ReturnUrl = authModel.Redirect;
                        ViewBag.Code = authModel.ClientId;
                        return View("Authenticate");
                    }
                }
            }
            catch
            {
                return View("Error");
            }

            string clientname = "";
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:1524/oauth/getuser?ClientId=" + authModel.ClientId.ToString() + "&redirect=" + authModel.Redirect));

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        clientname = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                    }
                    else
                    {
                        return View("Error");
                    }
                }
            }
            catch
            {
                return View("Error");
            }

            ViewBag.ReturnUrl = authModel.Redirect;
            ViewBag.Code = authModel.ClientId;
            ViewBag.Username = authModel.Username;
            ViewBag.Password = authModel.Password;
            ViewBag.Clientname = clientname;
            return View("Access");
        }

        public async Task<ActionResult> Acept(AuthenticationModRedirect authModel)
        {
            AuthModelID modelID = new AuthModelID();

            modelID.Username = authModel.Username;
            modelID.Password = authModel.Password;
            modelID.ClientId = authModel.ClientId;

            string code = "";

            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.PostAsJsonAsync(new Uri("http://localhost:1524/oauth/login"), modelID);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        code = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                    }
                    else
                    {
                        return View("Error");
                    }
                }
            }
            catch
            {
                return View("Error");
            }

            if (authModel.Redirect != null)
            {
                return Redirect(String.Format(authModel.Redirect + "?code={0}&state=", HttpUtility.UrlEncode(code)));
            }
            return RedirectToAction("Index", "Home");
        }



    }
}