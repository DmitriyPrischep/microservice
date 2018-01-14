﻿using Frontend.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;

namespace Frontend.Controllers
{
    public class WelcomeController : Controller
    {
        string token;
        public ActionResult Index()
        {
            return View("Index");
        }

        public ActionResult Description()
        {
            return View("Description");
        }

        public ActionResult Machines()
        {
            return View("Machines");
        }

        // POST: /Account/Login
        public ActionResult Login()
        {
            return Redirect(String.Format("http://localhost:49939/Users/Authenticate?redirect_uri={0}&client_id={1}", "http://localhost:53722/Hello/lootcodes", 1));
        }

        public async Task<ActionResult> lootcodes(string code, string state)
        {
            TokenMessage msg = new TokenMessage();
            AuthCodeModel codeModel = new AuthCodeModel();

            codeModel.Code = code;
            codeModel.RedirectURI = "http://localhost:53722/";
            codeModel.GrantType = "code";
            codeModel.ClientId = 1;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.PostAsJsonAsync(new Uri("http://localhost:56454/api/gate/code"), codeModel);

                    if (res.IsSuccessStatusCode)
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        msg = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenMessage>(Response);
                        HttpContext.Response.Cookies["access_token"].Value = msg.AccessToken;
                        HttpContext.Response.Cookies["refresh_token"].Value = msg.RefreshToken;
                    }
                    else
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        var str = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(Response);
                        return View("SorryPage", (object)str);
                    }
                }
            }
            catch
            {
                string str = "Now system is unavailable";
                return View("SorryPage", (object)str);
            }
            return View("Description");
        }


        public async Task<ActionResult> getMachines()
        {
            List<Frontend.Models.MachineModel> EmpInfo = new List<Models.MachineModel>();
            try
            {
                using(HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync(new Uri("http://localhost:49939/api/gateway/inf/machines"));

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.MachineModel>>(EmpResponse);
                    }
                    else
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        var str = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                        return View("SorryPage", (object)str);
                    }
                }
            }
            catch
            {
                string str = "Now system is unavailable";
                return View("SorryPage", (object)str);
            }
            return View(EmpInfo);
        }

        public async Task<ActionResult> getMachineByID(int machine_number) {
            Frontend.Models.MachineModel MachineInfo = new Models.MachineModel();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync(new Uri("http://localhost:49939/api/gateway/inf/machines/" + machine_number.ToString()));

                    if (res.IsSuccessStatusCode)
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        MachineInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.MachineModel>(Response);
                    }
                    else
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        var str = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(Response);
                        return View("SorryPage", (object)str);
                    }
                }
            }
            catch
            {
                string str = "Now system is unavailable";
                return View("SorryPage", (object)str);
            }
            return View(MachineInfo);
        }


        public async Task<ActionResult> getUserByFIO(string user_name)
        {
            Frontend.Models.UserModel UserInfo = new Models.UserModel();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync("http://localhost:49939/api/gateway/~users/" + user_name);

                    if (res.IsSuccessStatusCode)
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        UserInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.UserModel>(Response);
                    }
                    else
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        var str = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(Response);
                        return View("SorryPage", (object)str);
                    }
                }
            }
            catch
            {
                string str = "Now system is unavailable";
                return View("SorryPage", (object)str);
            }

            return View(UserInfo);
        }

        public async Task<ActionResult> getUsersPage(int? page, int pageSize = 4)
        {
            if (pageSize <= 0)
                pageSize = 4;
            List<UserModel> EmpInfo = new List<UserModel>();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:49939/api/gateway/users/all/page/" + page.ToString() + "/" + pageSize.ToString()));
                    if (res.IsSuccessStatusCode)
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        EmpInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<UserModel>>(Response);
                    }
                    else
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        var str = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(Response);
                        return View("SorryPage", (object)str);
                    }
                }
            }
            catch
            {
                string str = "Now system is unavailable";
                return View("SorryPage", (object)str);
            }

            int pageNumber = 1;
            return View(EmpInfo.ToPagedList(pageNumber, pageSize));
        }

        public async Task<ActionResult> getMachinesPage(int? page, int pageSize = 4)
        {
            if (pageSize <= 0)
                pageSize = 4;
            List<MachineModel> EmpInfo = new List<MachineModel>();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:49939/api/gateway/machines/all/page/" + page.ToString() + "/" + pageSize.ToString()));

                    if (res.IsSuccessStatusCode)
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        EmpInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MachineModel>>(Response);
                    }
                    else
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        var str = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(Response);
                        return View("SorryPage", (object)str);
                    }
                }
            }
            catch
            {
                string str = "Now system is unavailable";
                return View("SorryPage", (object)str);
            }

            int pageNumber = 1;
            return View(EmpInfo.ToPagedList(pageNumber, pageSize));
        }

        public async Task<ActionResult> getUserFilter(string user_fio)
        {
            UserModel UserFine = new UserModel();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync("http://localhost:49939/api/gateway/~users/" + user_fio);

                    if (res.IsSuccessStatusCode)
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        UserFine = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(Response);
                    }
                    else
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        var str = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                        return View("SorryPage", (object)str);
                    }
                }
            }
            catch
            {
                string str = "Now system is unavailable";
                return View("SorryPage", (object)str);
            }

            return View(UserFine);
        }

        public async Task<ActionResult> getUser(string user_fio, string Phone, string Adress)
        {
            string request = "";
            if (Phone != "")
            {
                request += "/" + Phone;
            }
            else
            {
                request += "/_";
            }
            if (Adress != "")
            {
                request += "/" + Adress;
            }
            else
            {
                request += "/_";
            }
            UserModel User = new UserModel();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync("http://localhost:49939/api/gateway/~users/" + user_fio + request);

                    if (res.IsSuccessStatusCode)
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        User = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(Response);
                    }
                    else
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        var str = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(Response);
                        return View("SorryPage", (object)str);
                    }
                }
            }
            catch
            {
                string str = "Now system is unavailable";
                return View("SorryPage", (object)str);
            }

            return View(User);
        }

        public async Task<ActionResult> getMachine(string Mark, string Model, int Year, string StateNumber)
        {
            string request = "";
            if (Model != "")
            {
                request += "/" + Model;
            }
            else
            {
                request += "/_";
            }
            if (Year != 0)
            {
                request += "/" + Year.ToString();
            }
            else
            {
                request += "/_";
            }
            MachineModel Machine = new MachineModel();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync("http://localhost:49939/api/gateway/~machines/" + Mark + request);

                    if (res.IsSuccessStatusCode)
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        Machine = Newtonsoft.Json.JsonConvert.DeserializeObject<MachineModel>(Response);
                    }
                    else
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        var str = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(Response);
                        return View("SorryPage", (object)str);
                    }
                }
            }
            catch
            {
                string str = "Now system is unavailable";
                return View("SorryPage", (object)str);
            }

            return View(Machine);
        }

        public async Task<ActionResult> addUser(DetailUserModel userModel)
        {
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.PostAsJsonAsync("http://localhost:49939/api/gateway/~users/add", userModel);

                    if (res.IsSuccessStatusCode)
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        var str = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(Response);
                        return View("SorryPage", (object)str);
                    }
                }
            }
            catch
            {
                string str = "Now system is unavailable";
                return View("SorryPage", (object)str);
            }

            return View();
        }

        public async Task<ActionResult> deleteUser(string user_fio)
        {
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.DeleteAsync("http://localhost:49939/api/gateway/~users/delete/" + user_fio);

                    if (res.IsSuccessStatusCode)
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        var str = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(Response);
                        return View("SorryPage", (object)str);
                    }
                }
            }
            catch
            {
                string str = "Now system is unavailable";
                return View("SorryPage", (object)str);
            }


            return View();
        }

        public async Task<ActionResult> editUser(int userID, string FIO, string Phone, string Adress, int fineID)
        {
            UserModel user = new UserModel();
            user.Id = userID;
            user.FIO = FIO;
            user.Phone = Phone;
            user.Adress = Adress;
            user.IdFines = fineID;

            if (!ModelState.IsValid)
            {
                return View();
            }

            try 
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.PutAsJsonAsync("http://localhost:49939/api/gateway/~users/edit/" + userID.ToString(), user);

                    if (res.IsSuccessStatusCode)
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        var str = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(Response);
                        return View("SorryPage", (object)str);
                    }
                }
            }
            catch
            {
                string str = "Now system is unavailable";
                return View("SorryPage", (object)str);
            }
            return View();
        }

    }
}
