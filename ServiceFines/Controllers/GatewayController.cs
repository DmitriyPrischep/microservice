using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using PagedList.Mvc;
using ServiceFines.Models;
using System.Messaging;
using PagedList;
using NLog;

namespace ServiceFines.Controllers
{
    [RoutePrefix("api/Gateway")]
    public class GatewayController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static string[] tokens = new string[3];

        public GatewayController()
        {
            Task.Run(() => backwork());
        }

        public async void backwork()
        {
            MessageQueue queue;
            if(MessageQueue.Exists(@".\private$\PrivateQueue"))
            {
                queue = new MessageQueue(@".\private$\PrivateQueue");
            }
            else
            {
                queue = MessageQueue.Create(".\\private$\\PrivateQueue");
            }
            using (queue)
            {
                queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                while(queue.CanRead)
                {
                    Message msg = queue.Receive();
                    int res = await DeleteEntyti(msg.Body.ToString());
                    if(res < 0)
                    {
                        queue.Send(msg);
                    }
                }
            }
        }

        private async Task<int> DeleteEntyti(string source)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    string msg = source.Substring(0, 23);
                    switch(msg)
                    {
                        case "http://localhost:50078/":
                            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[0]);
                            break;
                        case "http://localhost:50133/":
                            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[1]);
                            break;
                        case "http://localhost:50178/":
                            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[2]);
                            break;
                        default:
                            break;
                    }

                    HttpResponseMessage res = await client.DeleteAsync(source);
                    if (res.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        GetToken();
                        return -1;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request DELETE http://localhost:50178/api/Fines/ with filters. Error message: {0}", ex.Message);
                return -1;
            }
            return 0;
        }

        private async Task<int> DeleteAsyncFine(FineModel fine)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    bool flag = false;
                    while (flag)
                    {
                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[2]);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage res = await client.DeleteAsync("http://localhost:50178/api/Fines/" + fine.Id);
                        if (res.StatusCode == HttpStatusCode.Unauthorized) GetToken();
                        else flag = true;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request DELETE http://localhost:50178/api/Fines/ with filters. Error message: {0}", ex.Message);
                return -1;
            }
            return 0;
        }

        private async Task<int> DeleteAsyncUser(UserModel user)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    bool flag = false;
                    while (flag)
                    {
                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[1]);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage res = await client.DeleteAsync("http://localhost:50133/api/Users/" + user.Id);
                        if (res.StatusCode == HttpStatusCode.Unauthorized) GetToken();
                        else flag = true;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request DELETE http://localhost:50133/api/Users/ with filters. Error message: {0}", ex.Message);
                return -1;
            }
            return 0;
        }


        private int DeleteFine(FineModel fine)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    Task<HttpResponseMessage> test = client.DeleteAsync("http://localhost:50178/api/Fines/" + fine.Id);
                    var k = test.Result;
                }
            }
            catch(HttpRequestException ex)
            {
                logger.Error("Error with request DELETE http://localhost:50178/api/Fines/ with filters. Error message: {0}", ex.Message);
                return -1;
            }
            return 0;
        }

        private int DeleteUser(UserModel user)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    Task<HttpResponseMessage> test = client.DeleteAsync("http://localhost:50133/api/Users/" + user.Id);
                    var k = test.Result;
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request DELETE http://localhost:50133/api/Users/ with filters. Error message: {0}", ex.Message);

                return -1;
            }
            return 0;
        }


        private int GetToken()
        {
            string[] adress = new string[3];
            adress[0] = "http://localhost:50078/";
            adress[1] = "http://localhost:50133/";
            adress[2] = "http://localhost:50178/";
            int i = 0, j = 0;
            try
            {
                for(i = j; i < 3; i++)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var form = new Dictionary<string, string>
                        {
                           {"grant_type", "password"},
                           {"username", "Offender3" + (1+i).ToString()},
                           {"password", "1234"},
                        };
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var loginContent = new FormUrlEncodedContent(form);
                        HttpResponseMessage res = client.PostAsync(adress[i], loginContent).Result;
                        if (res.IsSuccessStatusCode)
                        {
                            var Response = res.Content.ReadAsStringAsync().Result;
                            Response = Response.Remove(0, 17);
                            Response = Response.Remove(Response.IndexOf(",") - 1, Response.Length - Response.IndexOf(",") + 1);
                            tokens[i] = Response;
                            j = i;
                        }
                        else
                        {
                            j = i;
                            return -1;
                        }

                    }
                }
            }
            catch (HttpRequestException ex)
            {
                GetToken();
            }
            return 0;
        }

        // GET: api/gateway/users
        [Route("inf/{service:maxlength(32)}")]
        public async Task<IHttpActionResult> Get([FromUri] string service)
        {
#if (DEBUG == true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif      
            logger.Info("Request from {1} with parametr 'service'= {0}", service, ip);
            switch (service)
            {
                case "users":
                    List<UserModel> Users = new List<UserModel>();
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            HttpResponseMessage res = await client.GetAsync(new Uri("http://localhost:50133/api/Users"));

                            if (res.IsSuccessStatusCode)
                            { 
                                var EmpResponse = res.Content.ReadAsStringAsync().Result;
                                Users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<UserModel>>(EmpResponse);
                            }
                            logger.Info("Request http://localhost:50133/api/Users. Answer status = {0} and Reason = {1}", res.StatusCode, res.ReasonPhrase);
                        }
                    }
                    catch (HttpException ex)
                    {
                        logger.Error("Error with request http://localhost:50133/api/Users. Answer status = {0} and Reason = {1}", ex.WebEventCode, ex.Message);
                        //return StatusCode(HttpStatusCode.BadGateway);
                        Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
                    }
                    logger.Info("Succsess request from {1} with parametr 'service'= {0}", service, ip);
                    return Ok(Users);

                case "machines":
                    List<MachineModel> Machines = new List<MachineModel>();
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            HttpResponseMessage res = await client.GetAsync(new Uri("http://localhost:50078/api/Machines"));

                            if (res.IsSuccessStatusCode)
                            {
                                var EmpResponse = res.Content.ReadAsStringAsync().Result;
                                Machines = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MachineModel>>(EmpResponse);
                            }
                            logger.Info("Request http://localhost:50078/api/Machines. Answer status = {0} and Reason = {1}", res.StatusCode, res.ReasonPhrase);
                        }
                    }
                    catch (HttpException ex)
                    {
                        logger.Error("Error with request http://localhost:50078/api/Machines. Answer status = {0} and Reason = {1}", ex.WebEventCode, ex.Message);
                        //return StatusCode(HttpStatusCode.BadGateway);
                        Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
                    }
                    logger.Info("Succsess request from {1} with parametr 'service'= {0}", service, ip);
                    return Ok(Machines);

                case "fines":
                    List<FineModel> Fines = new List<FineModel>();
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            HttpResponseMessage res = await client.GetAsync(new Uri("http://localhost:50178/api/Fines"));

                            if (res.IsSuccessStatusCode)
                            {
                                var EmpResponse = res.Content.ReadAsStringAsync().Result;
                                Fines = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FineModel>>(EmpResponse);
                            }
                            logger.Info("Request http://localhost:50178/api/Fines. Answer status = {0} and Reason = {1}", res.StatusCode, res.ReasonPhrase);
                        }
                    }
                    catch (HttpException ex)
                    {
                        logger.Error("Error with request http://localhost:50178/api/Fines. Answer status = {0} and Reason = {1}", ex.WebEventCode, ex.Message);
                        //return StatusCode(HttpStatusCode.BadGateway);
                        Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
                    }
                    logger.Info("Succsess request from {1} with parametr 'service'= {0}", service, ip);
                    return Ok(Fines);
            }
            return StatusCode(HttpStatusCode.NotImplemented);
        }


        // GET: api/gateway/users
        //http://localhost:49939/api/gateway/users/all/page/1/2
        [Route("users/all/page/{page}/{pageSize:int=3}")]
        public async Task<IPagedList<UserModel>> Get(int? page, int pageSize)
        {
#if (DEBUG == true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            logger.Info("Request from {2} with parametr 'page'= {0} 'pageSize'= {1}", page, pageSize, ip);
            int pageNumber = (page ?? 1);
            List<UserModel> UserInfo = new List<UserModel>();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync(new Uri("http://localhost:50133/api/Users"));

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        UserInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<UserModel>>(EmpResponse);
                    }
                }
            }
            catch(HttpException ex)
            {
                logger.Error("Error with request http://localhost:50133/api/Users. Answer status = {0} and Reason = {1}", ex.WebEventCode, ex.Message);
                //throw new HttpException(400, "Bad Request");
                return UserInfo.ToPagedList(pageNumber, pageSize);
            }
            logger.Info("Succsess request from {2} with parametr 'page'= {0} 'pageSize'= {1}", page, pageSize, ip);
            return UserInfo.ToPagedList(pageNumber, pageSize);
        }

        // GET: api/gateway/machines
        [Route("machines/all/page/{page}/{pageSize:int=3}")]
        public async Task<IPagedList<MachineModel>> GetAllUsers(int? page, int pageSize)
        {
#if (DEBUG == true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            logger.Info("Request from {2} with parametr 'page'= {0} 'pageSize'= {1}", page, pageSize, ip);
            int pageNumber = (page ?? 1);
            List<MachineModel> UserInfo = new List<MachineModel>();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync(new Uri("http://localhost:50078/api/Machines"));

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        UserInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MachineModel>>(EmpResponse);
                    }
                }
            }
            catch (HttpException ex)
            {
                logger.Error("Error with request http://localhost:50078/api/Machines. Answer status = {0} and Reason = {1}", ex.WebEventCode, ex.Message);
                //throw new HttpException(400, "Bad Request");
                return UserInfo.ToPagedList(pageNumber, pageSize);
            }
            logger.Info("Succsess request from {2} with parametr 'page'= {0} 'pageSize'= {1}", page, pageSize, ip);
            return UserInfo.ToPagedList(pageNumber, pageSize);
        }




        //users/{FIO}/{Phone:maxlength(11)=_}/{Adress:maxlength(32)=_}
        [Route("inf/{service:maxlength(32)}/{id:int}")]
        public async Task<IHttpActionResult> Get([FromUri]string service, [FromUri]int id)
        {
#if (DEBUG == true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            logger.Info("Request from {3} with parametr 'service'= {0} 'id'={1}", service, id, ip);
            switch(service)
            {
                case "users":
                    UserModel UserInfo = new UserModel();
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            HttpResponseMessage res = await client.GetAsync(new Uri("http://localhost:50133/api/Users/" + id.ToString()));

                            if (res.IsSuccessStatusCode)
                            {
                                var EmpResponse = res.Content.ReadAsStringAsync().Result;
                                UserInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(EmpResponse);
                            }
                        }
                    }
                    catch (HttpException ex)
                    {
                        logger.Error("Error with request http://localhost:50133/api/Users/. Answer status = {0} and Reason = {1}", ex.WebEventCode, ex.Message);
                        //return StatusCode(HttpStatusCode.BadGateway);
                        return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
                    }
                    logger.Info("Complete request from {3} with parametr 'service'= {0} 'id'={1}", service, id, ip);
                    return Ok(UserInfo);

                case "machines":
                    MachineModel MachineInfo = new MachineModel();
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            HttpResponseMessage res = await client.GetAsync(new Uri("http://localhost:50078/api/Machines/" + id.ToString()));

                            if (res.IsSuccessStatusCode)
                            {
                                var EmpResponse = res.Content.ReadAsStringAsync().Result;
                                MachineInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<MachineModel>(EmpResponse);
                            }
                        }
                    }
                    catch (HttpException ex)
                    {
                        logger.Error("Error with request http://localhost:50078/api/Machines. Answer status = {0} and Reason = {1}", ex.WebEventCode, ex.Message);
                        //return StatusCode(HttpStatusCode.BadGateway);
                        return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
                    }
                    logger.Info("Complete request from {3} with parametr 'service'= {0} 'id'={1}", service, id, ip);
                    return Ok(MachineInfo);


                case "fines":
                    FineModel FineInfo = new FineModel();
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            HttpResponseMessage res = await client.GetAsync(new Uri("http://localhost:50178/api/Fines/" + id.ToString()));

                            if (res.IsSuccessStatusCode)
                            {
                                var EmpResponse = res.Content.ReadAsStringAsync().Result;
                                FineInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<FineModel>(EmpResponse);
                            }
                        }
                    }
                    catch (HttpException ex)
                    {
                        logger.Error("Error with request http://localhost:50178/api/Fines. Answer status = {0} and Reason = {1}", ex.WebEventCode, ex.Message);
                        //return StatusCode(HttpStatusCode.BadGateway);
                        return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
                    }
                    logger.Info("Complete request from {3} with parametr 'service'= {0} 'id'={1}", service, id, ip);
                    return Ok(FineInfo);
            }
            throw new HttpException(500, "Not Implemented");
        }

        [Route("~users/{FIO}/{Phone:maxlength(11)=_}/{Adress:maxlength(32)=_}")]
        public async Task<IHttpActionResult> Get([FromUri] UserModel user)
        {
#if (DEBUG == true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            logger.Info("Request GET from {3} with parametrs 'FIO'= {0} 'Phone'= {1} 'Adress'= {2}", user.FIO, user.Phone, user.Adress, ip);
            string requeststr = "";
            if(user.FIO != null)
            {
                requeststr += "?$filter=FIO eq '" + user.FIO + "'";
            }

            if(user.Phone != "_")
            {
                requeststr += " and Phone eq '" + user.Phone + "'";
            }

            if (user.Adress != "_")
            {
                requeststr += " and Adress eq '" + user.Adress + "'";
            }
            UserModel UserInfo = new UserModel();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync("http://localhost:50133/odata/UserInf" + requeststr);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                        UserInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(EmpResponse);
                    }
                }
            }
            catch (HttpException ex)
            {
                logger.Warn("Error GET http://localhost:50133/odata/UserInf + {0} Error message: {1}", requeststr, ex.Message);
                //return StatusCode(HttpStatusCode.BadGateway);
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }
            logger.Info("Success complete request GET from {3} with parametrs  'FIO'= {0} 'Phone'= {1} 'Adress'= {2}", user.FIO, user.Phone, user.Adress, ip);
            if (UserInfo == null)
            {
                return Ok();
            }
            return Ok(UserInfo);
        }


        [Route("~machines/{Mark:maxlength(14)=_}/{Model:maxlength(12)=_}/{Year:int=0}")]
        public async Task<IHttpActionResult> Get([FromUri] MachineModel machine)
        {
#if (DEBUG == true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            logger.Info("Request GET from {4} with parametrs 'Type'= {0} 'Mark'= {1} 'Model'= {2} 'Year'= {3}", machine.Type, machine.Mark, machine.Model, machine.Year, ip);
            string requeststr = "";
            if (machine.Mark != null)
            {
                requeststr += "?$filter=Mark eq '" + machine.Mark + "'";
            }

            if (machine.Model != "_")
            {
                requeststr += " and Model eq '" + machine.Model + "'";
            }
            if(machine.Year != 0)
            {
                requeststr += " and Year eq " + machine.Year;
            }

            MachineModel MachineInfo = new MachineModel();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync("http://localhost:50078/odata/MachineInf/" + requeststr);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                        MachineInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<MachineModel>(EmpResponse);
                    }
                }
            }
            catch (HttpException ex)
            {
                logger.Warn("Error GET http://localhost:50078/odata/MachineInf + {0} Error message: {1}", requeststr, ex.Message);
                //return StatusCode(HttpStatusCode.BadGateway);
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }
            logger.Info("Success complete request GET from {4} with parametrs 'Type'= {0} 'Mark'= {1} 'Model'= {2} 'Year'= {3}", machine.Type, machine.Mark, machine.Model, machine.Year, ip);
            if (MachineInfo == null)
            {
                return Ok();
            }
            return Ok(MachineInfo);
        }

        
       
        [Route("~getuser/{UserFIO:maxlength(36)=_}")]
        public async Task<IHttpActionResult> GetAll([FromUri]string UserFIO)
        {
#if (DEBUG == true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif

            logger.Info("Request GET from {1} with parametr 'UserFIO'= {0}", UserFIO, ip);
            string requeststr = "";
            if(!(UserFIO == "_"))
            {
                requeststr += "?$filter=FIO eq '" + UserFIO + "'";
            }
            else
            {
                logger.Warn("Request GET from {1} with parametr 'UserFIO'= {0} aborted by parameters", UserFIO, ip);
                //return StatusCode(HttpStatusCode.BadRequest);
                return Content(HttpStatusCode.BadRequest, "Bad DATA. Many users. But need one user.");
            }

            List<FineModel> FineInfo = new List<FineModel>();
            UserModel UserInfo = new UserModel();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync("http://localhost:50133/odata/UserInf/" + requeststr);

                    if(res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                        UserInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(EmpResponse);
                    }
                }
            }
            catch (HttpException ex)
            {
                logger.Warn("Error GET http://localhost:50133/odata/UserInf + {0} Error message: {1}", requeststr, ex.Message);
                //return StatusCode(HttpStatusCode.BadGateway);
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            if (UserInfo == null)
            {
                return Content(HttpStatusCode.BadRequest, "Users is not found.");
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:50178/");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync("odata/FineInf?$filter=Id eq " + UserInfo.IdFines.ToString());

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('['));
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']') + 1, 1);
                        FineInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FineModel>>(EmpResponse);
                    }
                }
            }
            catch (HttpException ex)
            {
                logger.Warn("Error GET http://localhost:50178/odata/UserInf?$filter=Id eq {0} Error message: {1}", UserInfo.Id, ex.Message);

                DetailFineModel temp = new DetailFineModel();
                temp.FIO = UserInfo.FIO;
                temp.Adress = UserInfo.Adress;
                temp.Phone = UserInfo.Phone;
                temp.NameFine = string.Empty;
                temp.AmountFine = 0;
                logger.Info("DEGRADATE complete request GET from {1} with parametr 'CompanyName'= {0}", UserFIO, ip);
                return Ok(temp);
            }

            List<DetailFineModel> DUserInfo = new List<DetailFineModel>();

            foreach (var t in FineInfo)
            {
                DetailFineModel temp = new DetailFineModel();

                temp.FIO = UserInfo.FIO;
                temp.Adress = UserInfo.Adress;
                temp.Phone = UserInfo.Phone;
                temp.NameFine = t.NameFine;
                temp.AmountFine = t.AmountFine;

                DUserInfo.Add(temp);
            }
            logger.Info("Success complete request GET from {1} with parametr 'UserFIO'= {0}", UserFIO, ip);
            return Ok(DUserInfo);
        }



        [Route("~fines/{fine:maxlength(32)=_}")]
        public async Task<IHttpActionResult> GetAllFines([FromUri] string fine)
        {
#if(DEBUG==true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            logger.Info("Request GET from {1} with parametr 'fine'= {0}", fine, ip);
            string requeststr = "";
            if (!(fine == "_"))
            {
                requeststr = requeststr + "?$filter=NameFine eq '" + fine + "'";
            }
            else
            {
                FineModel Fine = new FineModel();
                return Ok(Fine);
            }

            FineModel FineInfo = new FineModel();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync("http://localhost:50178/odata/FineInf" + requeststr);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                        FineInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<FineModel>(EmpResponse);
                    }
                }
            }
            catch (HttpException ex)
            {
                logger.Warn("Error GET http://localhost:50178/odata/FineInfwith + {0} Error message: {1}", requeststr, ex.Message);
                //return StatusCode(HttpStatusCode.BadGateway);
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }
            logger.Info("Success compliete request GET from {1} with parametr 'fine'= {0}", fine, ip);
            return Ok(FineInfo);
        }



        [Route("~fines/{fine:maxlength(32)=_}/page/{page}/{pageSize:int=3}")]
        public async Task<IPagedList<FineModel>> GetAllFines([FromUri] string fine, int? page, int pageSize)
        {
            int pageNumber = (page ?? 1);
#if(DEBUG==true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            logger.Info("Request GET from {1} with parametr 'FineName'= {0}", fine, ip);
            string requeststr = "";
            if (!(fine == "_"))
            {
                requeststr += "?$filter=NameFine eq '" + fine + "'";
            }
            List<FineModel> FineInfo = new List<FineModel>();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync("http://localhost:50178/odata/FineInf" + requeststr);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('['));
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']') + 1, 1);
                        FineInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FineModel>>(EmpResponse);
                    }
                }
            }
            catch (HttpException ex)
            {
                logger.Warn("Error GET http://localhost:50178/odata/FineInfwith + {0} Error message: {1}", requeststr, ex.Message);
                return FineInfo.ToPagedList(pageNumber, pageSize);
            }
            logger.Info("Success compliete request GET from {1} with parametr 'fine'= {0}", fine, ip);
            return FineInfo.ToPagedList(pageNumber, pageSize);
        }



        

        // POST: api/Gateway
        [Route("~users/add")]
        public async Task<IHttpActionResult> Post([FromBody]DetailUserModel value)
        {
#if (DEBUG == true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            logger.Info("Request POST from {8} with parametrs 'FIO'= {0}, 'Adress'= {1}, 'Phone'= {2}, 'Mark'= {3}, 'Model'= {4}, 'StateNumber'= {5}, 'NameFine'= {6}, 'AmountFine'= {7}", 
                value.FIO, value.Adress, value.Phone, value.Mark, value.Model, value.StateNumber, value.NameFine, value.AmountFine, ip);

            if ((value.FIO == null) || (value.Adress == null) || (value.Phone == null) || 
                (value.Mark == null) || (value.Model == null) || (value.StateNumber == null) || 
                (value.NameFine == null) || (value.AmountFine == 0))
            {
                logger.Warn("ABORTED POST from {8} with parametrs 'FIO'= {0}, 'Adress'= {1}, 'Phone'= {2}, 'Mark'= {3}, 'Model'= {4}, 'StateNumber'= {5}, 'NameFine'= {6}, 'AmountFine'= {7}",
                value.FIO, value.Adress, value.Phone, value.Mark, value.Model, value.StateNumber, value.NameFine, value.AmountFine, ip);
                return Content(HttpStatusCode.BadRequest, "Bad input data.");
            }

            UserModel user = new UserModel();
            UserModel bufUser = new UserModel();
            MachineModel machine = new MachineModel();
            MachineModel bufMachine = new MachineModel();
            FineModel fine = new FineModel();
            FineModel bufFine = new FineModel();
            bool flag = true;

            bufUser.FIO = value.FIO;
            bufUser.Adress = value.Adress;
            bufUser.Phone = value.Phone;

            bufMachine.Mark = value.Mark;
            bufMachine.Model = value.Model;
            bufMachine.StateNumber = value.StateNumber;

            bufFine.NameFine = value.NameFine;
            bufFine.AmountFine = value.AmountFine;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync("http://localhost:50178/odata/FineInf?$filter=NameFine eq '" + bufFine.NameFine + "'");

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                        fine = Newtonsoft.Json.JsonConvert.DeserializeObject<FineModel>(EmpResponse);
                    }
                }
            }
            catch (HttpException ex)
            {
                logger.Error("Error with request GET http://localhost:50178/odata/FineInf?$filter=NameFines eq '{0}' . Error message: {1}", bufFine.NameFine, ex.Message);
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            try
            {
                if (fine == null)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage res = await client.PostAsJsonAsync("http://localhost:50178/api/Fines", bufFine);

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            bufFine = Newtonsoft.Json.JsonConvert.DeserializeObject<FineModel>(EmpResponse);
                        }
                        else
                        {
                            return Content(res.StatusCode, "Service Fines returned not succes status code.");
                        }
                    }
                }
                else
                {
                    bufFine = fine;
                    flag = false;
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request POST http://localhost:50178/api/Fines. Error message: {0}", ex.Message);
                //return StatusCode(HttpStatusCode.BadGateway);
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            bufUser.IdFines = bufFine.Id;

            try
            {
                using (HttpClient client = new HttpClient())
                {

                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync("http://localhost:50133/odata/UserInf?$filter=IdFines eq " + bufUser.IdFines
                        + " and " + "FIO eq '" + bufUser.FIO + "' and Phone eq '" + bufUser.Phone + "'");

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                        user = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(EmpResponse);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request GET http://localhost:50133/odata/UserInf with filters. Error message: {0}", ex.Message);
                //return StatusCode(HttpStatusCode.BadGateway);
                if(flag)
                    if (DeleteFine(bufFine) < 0)
                    {
                        return Content((HttpStatusCode)418, "Global system error. Sorry.");
                    }
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            try
            {
                if (user == null)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage res = await client.PostAsJsonAsync("http://localhost:50133/api/Users", bufUser);

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            bufUser = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(EmpResponse);
                        }
                        else
                        {
                            if (flag)
                                if (DeleteFine(bufFine) < 0)
                                {
                                    return Content((HttpStatusCode)418, "Global system error. Sorry.");
                                }
                            return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
                        }
                    }
                }
                else
                {
                    bufUser = user; 
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request POST http://localhost:50133/api/Users. Error message: {0}", ex.Message);
                if (flag)
                    if (DeleteFine(bufFine) < 0)
                    {
                        return Content((HttpStatusCode)418, "Global system error. Sorry.");
                    }
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            bufMachine.IdUsers = bufUser.Id;

            try
            {
                using (HttpClient client = new HttpClient())
                {

                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync("http://localhost:50078/odata/MachineInf?$filter=Mark eq '" + bufMachine.Mark + "' and " +
                        "Model eq '" + bufMachine.Model + "' and StateNumber eq '" + bufMachine.StateNumber + "'");

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                        machine = Newtonsoft.Json.JsonConvert.DeserializeObject<MachineModel>(EmpResponse);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request GET http://localhost:50078/odata/MachineInf with filters. Error message: {1}", bufFine.NameFine, ex.Message);
                if (flag)
                    if (DeleteFine(bufFine) < 0)
                    {
                        return Content((HttpStatusCode)418, "Global system error. Sorry.");
                    }
                if (DeleteUser(bufUser) < 0)
                {
                    return Content((HttpStatusCode)418, "Global system error. Sorry.");
                }
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            try
            {
                
                if (machine == null)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage res = await client.PostAsJsonAsync("http://localhost:50078/api/Machines", bufMachine);

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            bufMachine = Newtonsoft.Json.JsonConvert.DeserializeObject<MachineModel>(EmpResponse);
                        }
                        else
                        {
                            if (flag)
                                if (DeleteFine(bufFine) < 0)
                                {
                                    return Content((HttpStatusCode)418, "Global system error. Sorry.");
                                }
                            if (DeleteUser(bufUser) < 0)
                            {
                                return Content((HttpStatusCode)418, "Global system error. Sorry.");
                            }
                            return Content(res.StatusCode, "Error. Sorry.");
                        }
                    }
                }
                else
                {
                    bufMachine = machine;
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request POST http://localhost:50078/api/Machines . Error message: {0}", ex.Message);
                
                if (flag)
                {
                    int k = await DeleteAsyncFine(bufFine);
                    if(k < 0)
                    {
                        return Content((HttpStatusCode)418, "Global system error. Sorry.");
                    }
                }
                int res = await DeleteAsyncUser(bufUser);
                if(res < 0)
                {
                    return Content((HttpStatusCode)418, "Global system error. Sorry.");
                }
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            logger.Info("Success compliete request POST from {8} with parametrs 'FIO'= {0}, 'Adress'= {1}, 'Phone'= {2}, 'Mark'= {3}, 'Model'= {4}, 'StateNumber'= {5}, 'NameFine'= {6}, 'AmountFine'= {7}",
                value.FIO, value.Adress, value.Phone, value.Mark, value.Model, value.StateNumber, value.NameFine, value.AmountFine, ip);
            return StatusCode(HttpStatusCode.NoContent);
        }


        // PUT: api/Gateway/5
        [Route("~users/edit/{id:int=1}")]
        public async Task<IHttpActionResult> Put([FromUri] int id, [FromBody] UserModel user)
        {
#if (DEBUG == true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            logger.Info("Request PUT from {4} with parametrs 'ID'= {0}, 'FIO'= {1}, 'Phone'= {2}, 'Adress'= {3}", id, user.FIO, user.Phone, user.Adress, ip);
            if ((user.FIO == null) || (user.Phone == null) || (user.Adress == null))
            {
                logger.Warn("ABORTED PUT from {4} with parametrs 'ID'= {0}, 'FIO'= {1}, 'Phone'= {2}, 'Adress'= {3}", id, user.FIO, user.Phone, user.Adress, ip);
                return Content(HttpStatusCode.BadRequest, "Bad input data.");
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    await client.PutAsJsonAsync("http://localhost:50133/api/Users/" + id.ToString(), user);
                }
            }
            catch (HttpException ex)
            {
                logger.Error("Error with request PUT http://localhost:50133/api/Users + {4}. Parametrs 'ID'= {0}, 'FIO'= {1}, 'Phone'= {2}, 'Adress'= {3}", id, user.FIO, user.Phone, id.ToString(), ex.Message);
                return StatusCode(HttpStatusCode.BadGateway);
            }

            logger.Info("Success compliete PUT from {4}. Parametrs 'ID'= {0}, 'FIO'= {1}, 'Phone'= {2}, 'Adress'= {3}", id, user.FIO, user.Phone, user.Adress, ip);
            return StatusCode(HttpStatusCode.NoContent);

        }

        // DELETE: api/Gate/5
        [Route("~users/delete/{fio:maxlength(36)=_}")]
        public async Task<IHttpActionResult> Delete([FromUri] string fio)
        {
#if (DEBUG == true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            logger.Info("Request DELETE from {1} with parametr 'FIO'= {0}", fio, ip);

            var queue = new MessageQueue(@".\private$\PrivateQueue");

            string requeststr = "";
            if (!(fio == "_"))
            {
                requeststr = requeststr + "?$filter=FIO eq '" + fio + "'";
            }
            else
            {
                logger.Info("Request DELETE from {1} with parametr 'FIO'= {0} CANCELED by fitler. Bad parametr value.", fio, ip);
                return Content(HttpStatusCode.BadRequest, "Bad input data.");
            }

            List<FineModel> Fines = new List<FineModel>();
            UserModel User = new UserModel();
            
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync("http://localhost:50133/odata/UserInf" + requeststr);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                        User = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(EmpResponse);
                    }
                }
            }
            catch (HttpException ex)
            {
                logger.Error("Error with request GET http://localhost:50133/odata/UserInf + {0}. Error message: {1}", requeststr, ex.Message);
                //return StatusCode(HttpStatusCode.BadGateway);
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            if (User == null)
            {
                logger.Warn("Aborted DELETE from {1} with parametr 'FIO'= {0}. No such company.", fio, ip);
                //return StatusCode(HttpStatusCode.Conflict);
                return Content(HttpStatusCode.Conflict, "User is invalid");
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:50133/");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    await client.DeleteAsync("api/Users/" + User.Id);
                }
            }
            catch (HttpException ex)
            {
                logger.Error("Error with request DELETE http://localhost:50133/api/Users/ . Error message: {1}", requeststr, ex.Message);
                queue.Send("http://localhost:50133/api/Users/" + User.Id);
                //return StatusCode(HttpStatusCode.BadGateway);
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.GetAsync("http://localhost:50178/odata/FineInf?$filter=Id eq " + User.IdFines.ToString());

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('['));
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']') + 1, 1);
                        Fines = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FineModel>>(EmpResponse);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request GET http://localhost:50178/odata/UserInf?$filter=Id eq  + {0}. Error message: {1}", User.IdFines, ex.Message);
                //return StatusCode(HttpStatusCode.BadGateway);
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            try
            {
                foreach (var t in Fines)
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        await client.DeleteAsync("http://localhost:50178/api/Fines/" + t.Id);
                    }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request DELETE http://localhost:50178/api/Machine/. Error message: {0}", ex.Message);
                foreach (var t in Fines)
                    queue.Send("http://localhost:50178/api/Fines/" + t.Id.ToString());
                return StatusCode(HttpStatusCode.OK);
            }

            logger.Info("Success compliete DELETE from {1} with parametr 'FIO'= {0}", fio, ip);
            return Ok(User);
        }

        // POST api/Gateway/Code
        [Route("Code")]
        public async Task<IHttpActionResult> Postcode([FromBody] AuthCodeModel CodeModel)
        {
            TokenMessage msg = new TokenMessage();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.PostAsJsonAsync(new Uri("http://localhost:1524/oauth/gettokens"), CodeModel);

                    if (res.IsSuccessStatusCode)
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        msg = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenMessage>(Response);
                    }
                    else return Unauthorized();
                }
            }
            catch
            {
                return InternalServerError();
            }
            return Ok<TokenMessage>(msg);
        }

        // POST api/Gateway/Refresh
        [Route("refresh")]
        public async Task<IHttpActionResult> Postrefresh([FromBody] RefreshToken refresh)
        {
            TokenMessage msg = new TokenMessage();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await client.PostAsJsonAsync(new Uri("http://localhost:1524/oauth/refresh"), refresh);

                    if (res.IsSuccessStatusCode)
                    {
                        var Response = res.Content.ReadAsStringAsync().Result;
                        msg = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenMessage>(Response);
                    }
                    else return Unauthorized();
                }
            }
            catch
            {
                return InternalServerError();
            }
            return Ok<TokenMessage>(msg);
        }
    }
}
