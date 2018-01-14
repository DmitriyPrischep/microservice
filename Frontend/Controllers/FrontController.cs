using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;
using Frontend.Models;
using System.Web;

namespace Frontend.Controllers
{
    public class FrontController : Controller
    {
        string token;
        //
        // GET: /Front/
        public ActionResult Index()
        {
            return View("Index");
        }

        public ActionResult Description()
        {

            return View("Description");
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
            codeModel.RedirectURI = "http://localhost:55490";
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
                        return View("sorry", (object)str);
                    }
                }
            }
            catch
            {
                string myString = "System is unavalieable. lol.";
                return View("sorry", (object)myString);
            }
            return View("lab4");
        }

        public async Task<ActionResult> getUsers()
        {
            List<UserModel> EmpUser = new List<UserModel>();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync(new Uri("http://localhost:50133/api/DB"));

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpUser = Newtonsoft.Json.JsonConvert.DeserializeObject<List<UserModel>>(EmpResponse);
                    }
                }
            }
            catch (HttpException e)
            {
                throw new HttpException(400, "Bad Request", e);
            }
            return View(EmpUser);
        }


        public async Task<ActionResult> getUsersPage(int? page)
        {
            List<UserModel> UserInfo = new List<UserModel>();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:50133/api/DB"));

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        UserInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<UserModel>>(EmpResponse);
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            int pageSize = 3;
            int pageNumber = (page ?? 1);
            return View(UserInfo.ToPagedList(pageNumber, pageSize));
        }


        public async Task<ActionResult> getSomeUsers()
        {
            List<FineModel> FineInfo = new List<FineModel>();
            UserModel UserInfo = new UserModel();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:50133/");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync("odata/UserInf?$filter=FIO eq 'Ivanov'");

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                        UserInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(EmpResponse);
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:50178/");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync("odata/UserInf?$filter=id eq " + UserInfo.IdFines.ToString());

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('['));
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']') + 1, 1);
                        FineInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FineModel>>(EmpResponse);
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            List<DetailFineModel> DFineInfo = new List<DetailFineModel>();

            foreach (var t in FineInfo)
            {
                DetailFineModel temp = new DetailFineModel();

                temp.FIO = UserInfo.FIO;
                temp.Adress = UserInfo.Adress;
                temp.Phone = UserInfo.Phone;
                temp.NameFine = t.NameFine;
                temp.AmountFine = t.AmountFine;

                DFineInfo.Add(temp);
            }

            return View(DFineInfo);
        }





        public async Task<ActionResult> deleteUser()
        {
            List<FineModel> FineInfo = new List<FineModel>();
            UserModel UserInfo = new UserModel();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:50133/");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync("odata/CompInf?$filter=Name eq 'Microsort'");

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                        UserInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(EmpResponse);
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:50133/");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    await client.DeleteAsync("api/DB/" + UserInfo.Id);
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.BaseAddress = new Uri("http://localhost:50178/");
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync("odata/UserInf?$filter=Id eq " + UserInfo.IdFines.ToString());

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('['));
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']') + 1, 1);
                        FineInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FineModel>>(EmpResponse);
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            try
            {
                foreach (var t in FineInfo)
                    using (HttpClient test = new HttpClient())
                    {
                        test.BaseAddress = new Uri("http://localhost:50178/");
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        await test.DeleteAsync("api/DB/" + t.Id);
                    }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }
            return View();
        }




        public async Task<ActionResult> addUserFine()
        {
            FineModel fineId = new FineModel();
            FineModel fineBuf = new FineModel();
            fineId.NameFine = "Пересечение сплошной полосы";
            fineId.AmountFine = 5000;

            UserModel userId = new UserModel();
            UserModel userBuf = new UserModel();
            userId.FIO = "Petrov Ivan";
            userId.Adress = "Moscow Lenina 23";
            userId.Phone = "89991234567";
            userId.IdFines = 0;

            MachineModel autoId = new MachineModel();
            MachineModel autoBuf = new MachineModel();
            autoId.Mark = "Audi";
            autoId.Model = "Q3";
            autoId.Type = "Automobile";
            autoId.Year = 2011;
            autoId.StateNumber = "o444oo77";
            autoId.VIN = "WWW11S1234567890";             
            autoId.IdUsers = 0;

           
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:50178/");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync("odata/UserInf?$filter=NameFines eq '" + fineId.NameFine + "'");

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                        fineBuf = Newtonsoft.Json.JsonConvert.DeserializeObject<FineModel>(EmpResponse);
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            try
            {
                if (fineBuf == null)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri("http://localhost:50178/");
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage res = await client.PostAsJsonAsync("api/DB", fineId);

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            fineId = Newtonsoft.Json.JsonConvert.DeserializeObject<FineModel>(EmpResponse);
                        }
                    }
                }
                else
                {
                    fineId = fineBuf;
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            userId.IdFines = fineId.Id;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:50133/");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync("odata/UserInf?$filter=IdFines eq " + userId.IdFines + " and " +
                        "FIO eq '" + userId.FIO + "' and Phone eq '" + userId.Phone + "'");

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                        userBuf = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(EmpResponse);
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            try
            {
                if (userBuf == null)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri("http://localhost:50133/");
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage res = await client.PostAsJsonAsync("api/DB", userId);

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            userId = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(EmpResponse);
                        }
                    }
                }
                else
                {
                    userId = userBuf;
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            autoId.IdUsers = userId.Id;

            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.BaseAddress = new Uri("http://localhost:50078/");
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync("odata/UserInf?$filter=Mark eq '" + autoId.Mark + "' and " +
                        "Model eq " + autoId.Model + " and Year eq " + autoId.Year + " and StateNumber eq " + autoId.StateNumber);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                        autoBuf = Newtonsoft.Json.JsonConvert.DeserializeObject<MachineModel>(EmpResponse);
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            try
            {
                if (autoBuf == null)
                {
                    using (HttpClient test = new HttpClient())
                    {
                        test.BaseAddress = new Uri("http://localhost:50078/");
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage res = await test.PostAsJsonAsync("api/DB", autoId);

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            autoId = Newtonsoft.Json.JsonConvert.DeserializeObject<MachineModel>(EmpResponse);
                        }
                    }
                }
                else
                {
                    autoId = autoBuf;
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }
            return View();
        }

    }
}
