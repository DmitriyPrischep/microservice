using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Authentication.Models;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.Controllers
{
    [RoutePrefix("oauth")]
    public class AuthController : ApiController
    {
        private AuthenticationContext db = new AuthenticationContext();

        private static string HashPassword(string Message)
        {
            byte[] data = Encoding.UTF8.GetBytes(Message);
            using (HashAlgorithm SHA = new SHA256Managed())
            {
                byte[] encryptedBytes = SHA.TransformFinalBlock(data, 0, data.Length);
                return Convert.ToBase64String(SHA.Hash);
            }
        }

        [Route("getuser")]
        public async Task<IHttpActionResult> GetClientInfo(int clientId, string redirect)
        {
            Owner owner = new Owner();
            try
            {
                owner = await db.Owners.FindAsync(clientId);
            }
            catch
            {
                return InternalServerError();
            }

            if (owner == null)
                return BadRequest();

            if (owner.RedirectURI != redirect)
            {
                return Unauthorized();
            }

            return Ok<string>(owner.Name);
        }

        [Route("checkuser")]
        public async Task<IHttpActionResult> PostCheck([FromBody] AuthModel authModel)
        {
            User user = new User();
            try
            {
                user = await db.Users.FirstOrDefaultAsync(c => c.UserName == authModel.Username);
            }
            catch
            {
                return Unauthorized();
            }

            if (user == null)
                return Unauthorized();

            return Ok<string>(authModel.Username);
        }

        [Route("login")]
        public async Task<IHttpActionResult> PostLogIn([FromBody] AuthModelID authModelID)
        {
            User user = new User();
            try
            {
                user = await db.Users.FirstOrDefaultAsync(c => c.UserName == authModelID.Username);
            }
            catch
            {
                return Unauthorized();
            }

            if (user == null)
                return Unauthorized();

            if (user.UserPassword != HashPassword(authModelID.Password))
                return Unauthorized();

            string source1 = DateTime.Now.ToString() + "|" + user.UserName + "|" + user.UserRole;
            string source2 = DateTime.Now.ToString() + "|" + user.UserName + "|" + user.UserRole;
            string source3 = DateTime.Now.ToString() + "|" + user.UserName + "|" + user.UserRole;

            string Codestr = HashPassword(source1);
            string Tokenstr = HashPassword(source2);
            string Refreshstr = HashPassword(source3);

            Code TempCode = new Code();
            Token TempToken = new Token();

            TempCode.AccessCode = Codestr;
            TempCode.Timeofrelease = DateTime.Now.AddMinutes(10);
            TempCode.OwnerId = authModelID.ClientId;

            db.Codes.Add(TempCode);
            await db.SaveChangesAsync();

            Code ttt = await db.Codes.FirstOrDefaultAsync(c => c.AccessCode == TempCode.AccessCode);

            TempToken.AccessToken = Tokenstr;
            TempToken.RefreshToken = Refreshstr;
            TempToken.TimeOfReleaseAccessToken = DateTime.Now.AddMilliseconds(1);
            TempToken.TimeOfReleaseRefreshToken = DateTime.Now.AddMinutes(100);
            TempToken.UserId = user.Id;
            TempToken.AccessCodeId = ttt.Id;
            db.Tokens.Add(TempToken);
            await db.SaveChangesAsync();

            return Ok(ttt.AccessCode);
        }

        [Route("gettokens")]
        public async Task<IHttpActionResult> PostAccessToken([FromBody] AuthCodeModel codeModel)
        {
            Code code = new Code();
            try
            {
                code = await db.Codes.FirstOrDefaultAsync(x => x.AccessCode == codeModel.Code);
            }
            catch
            {
                return Unauthorized();
            }

            if (code == null) return Unauthorized();

            if (code.Timeofrelease <= DateTime.Now)
            {
                return Unauthorized();
            }

            Owner owner = new Owner();
            try
            {
                owner = await db.Owners.FirstOrDefaultAsync(x => x.RedirectURI == codeModel.RedirectURI);
            }
            catch
            {
                return Unauthorized();
            }

            Token token = new Token();
            try
            {
                token = await db.Tokens.FirstOrDefaultAsync(x => x.AccessCodeId == code.Id);
            }
            catch
            {
                return Unauthorized();
            }
            if (token == null)
                return Unauthorized();

            TokenMessage msg = new TokenMessage();
            msg.AccessToken = msg.AccessToken;
            msg.RefreshToken = msg.RefreshToken;
            msg.TokenType = "Bearer";
            return Ok<TokenMessage>(msg);
        }

        [Route("refresh")]
        public async Task<IHttpActionResult> PostRefresh([FromBody] RefreshToken key)
        {
            Token token = new Token();

            try
            {
                token = await db.Tokens.FirstOrDefaultAsync(x => x.RefreshToken == key.Token);
            }
            catch
            {
                return Unauthorized();
            }

            if (token == null)
                return Unauthorized();

            if (token.TimeOfReleaseRefreshToken <= DateTime.Now)
            {
                return Unauthorized();
            }
            token.TimeOfReleaseAccessToken = token.TimeOfReleaseAccessToken.Value.AddYears(-1);
            token.TimeOfReleaseRefreshToken = token.TimeOfReleaseRefreshToken.Value.AddYears(-1);

            db.Entry(token).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            User TMP = new User();
            try
            {
                TMP = await db.Users.FindAsync(token.UserId);
            }
            catch
            {
                return Unauthorized();
            }

            if (TMP == null)
                return Unauthorized();

            string source1 = DateTime.Now.ToString() + "|" + TMP.UserName + "|" + TMP.UserRole;
            string source2 = DateTime.Now.ToString() + "|" + TMP.UserName + "|" + TMP.UserRole;

            string Tokenstr = HashPassword(source1);
            string Refreshstr = HashPassword(source2);

            Token TempToken = new Token();

            TempToken.AccessToken = Tokenstr;
            TempToken.RefreshToken = Refreshstr;
            TempToken.TimeOfReleaseAccessToken = DateTime.Now.AddMinutes(5);
            TempToken.TimeOfReleaseRefreshToken = DateTime.Now.AddMinutes(10);
            TempToken.UserId = TMP.Id;
            TempToken.AccessCodeId = 2;
            db.Tokens.Add(TempToken);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            TokenMessage msg = new TokenMessage();

            msg.AccessToken = TempToken.AccessToken;
            msg.RefreshToken = TempToken.RefreshToken;
            msg.TokenType = "Bearer";

            return Ok<TokenMessage>(msg);
        }

        [Route("check")]
        public async Task<IHttpActionResult> PostCheck([FromBody] AuthRoleModel roleModel)
        {
            Token token = new Token();

            try
            {
                token = await db.Tokens.FirstOrDefaultAsync(x => x.AccessToken == roleModel.Token);
            }
            catch
            {
                return Unauthorized();
            }

            if (token == null)
                return Unauthorized();

            if (token.TimeOfReleaseAccessToken <= DateTime.Now)
            {
                return Unauthorized();
            }

            token.TimeOfReleaseAccessToken = DateTime.Now.AddMinutes(30);
            token.TimeOfReleaseRefreshToken = DateTime.Now.AddMinutes(30);
            db.Entry(token).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }


            User user = new User();
            try
            {
                user = await db.Users.FindAsync(token.UserId);
            }
            catch
            {
                return Unauthorized();
            }

            if (user == null)
                return Unauthorized();

            if (user.UserRole != roleModel.RequiredRole)
            {
                return Content(HttpStatusCode.Unauthorized, "Sorry. No access rights. Authenticate as admin");
            }

            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TokenExists(int id)
        {
            return db.Tokens.Count(e => e.Id == id) > 0;
        }
    }
}