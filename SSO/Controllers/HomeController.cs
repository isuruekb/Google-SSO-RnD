using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using SSO.Models;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SSO.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IHttpContextAccessor _httpContextAccessor;
        public static HttpContext? Current => new HttpContextAccessor().HttpContext;
        public HomeController(ILogger<HomeController> logger, IHttpContextAccessor context)
        {
            _logger = logger;
            _httpContextAccessor = context;
        }

        public IActionResult Index()
        {

            var remoteIpAddress = _httpContextAccessor.HttpContext.Request.HttpContext.Connection.RemoteIpAddress;
            return View();
        }


        public IActionResult SSORedirect(string code)
        {
            string accesstoke = GetAccessTokenByAuthCodeFlow(code);
            O365UserModel user = GetMeByAccessToken(accesstoke);//me service normally at auth code flow
            return View();
        }


        [HttpPost]
        //public IActionResult EfassPost([FromBody] EfassObj efassObj)
        public IActionResult EfassPost(EfassObj efassObj)
        {
            
            return View();
        }


        [HttpPost]
        public ActionResult GoogleLogin(GooglePostModel googleModel)
        {

            //get user from idtoken
            GoogleUserModel googleUserModel = ReadIdToken(googleModel.id_token);

            ViewBag.name = googleUserModel.name;
            ViewBag.given_name = googleUserModel.given_name;
            ViewBag.family_name = googleUserModel.family_name;
            ViewBag.email = googleUserModel.email;
            ViewBag.picture = googleUserModel.picture;

            return View();
        }


        
      


        private GoogleUserModel ReadIdToken(string id_token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(id_token);

            GoogleUserModel googleUserModel = new GoogleUserModel();

            var name = jwtSecurityToken.Payload.Claims.FirstOrDefault(x => x.Type == "name");
            if (name != null)
            {
                googleUserModel.name = name.Value;
            }

            var given_name = jwtSecurityToken.Payload.Claims.FirstOrDefault(x => x.Type == "given_name");
            if (given_name != null)
            {
                googleUserModel.given_name = given_name.Value;
            }

            var family_name = jwtSecurityToken.Payload.Claims.FirstOrDefault(x => x.Type == "family_name");
            if (family_name != null)
            {
                googleUserModel.family_name = family_name.Value;
            }



            var email = jwtSecurityToken.Payload.Claims.FirstOrDefault(x => x.Type == "email");
            if (email != null)
            {
                googleUserModel.email = email.Value;
            }

            var picture = jwtSecurityToken.Payload.Claims.FirstOrDefault(x => x.Type == "picture");
            if (picture != null)
            {
                googleUserModel.picture = picture.Value;
            }


            return googleUserModel;
        }


        private authRes GetGoogleAccessTokenByAuthCodeFlow(string code)
        {
            string clientId = "REPLACE YOUR CLIENT ID FROM GOOGLE CONSOLE";
            string appKey = "REPLACE YOUR APPKEY FROM GOOGLE CONSOLE";
            string apiEndpoint = "https://oauth2.googleapis.com/token";

            authRes authresponse = new authRes();
            string accessToken = string.Empty;
            string userObj = string.Empty;

            WebRequest accessTokenRequest = WebRequest.Create(apiEndpoint);
            accessTokenRequest.Method = "POST";
            accessTokenRequest.ContentType = "application/x-www-form-urlencoded";
            //openid for id_token, offline_access for refresh_token
            string requestParams = "client_id=" + clientId + "&client_secret=" + appKey + "&code=" + code + "&redirect_uri=http://localhost/SSOTest/Home/GoogleLogin" + "&grant_type=authorization_code";
            byte[] byteArray = Encoding.UTF8.GetBytes(requestParams);
            accessTokenRequest.ContentLength = byteArray.Length;
            Stream dataStream = accessTokenRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            using (WebResponse response = accessTokenRequest.GetResponse())
            {
                string json = string.Empty;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    json = reader.ReadToEnd();
                }
                O365AccessTokenModel accessTokenModel = JsonSerializer.Deserialize<O365AccessTokenModel>(json);
                accessToken = accessTokenModel.access_token;
                userObj = ExtractJwt(accessTokenModel.id_token);
            }

            authresponse.accessToken = accessToken;
            authresponse.userJson = userObj;

            return authresponse;
        }



        public IActionResult Privacy()
        {
            UserModel um = new UserModel();
            um.UserName = null;
            TM(um.UserName, um.Password);

            return View();
        }

        private void TM(string username,string pasw)
        {

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        

        public string ExtractJwt(string jwt)
        {
            // Create a JwtSecurityTokenHandler
            var tokenHandler = new JwtSecurityTokenHandler();

            // Read the JWT token
            var jwtTokenObject = tokenHandler.ReadJwtToken(jwt);
            return jwtTokenObject.ToString();
        }


        private void GetGoogleApiByAccessToken(string accesstoken, string email)
        {
            string apiEndpoint = "https://www.googleapis.com/auth/userinfo.profile?access_token=" + accesstoken + "&client_secret=" + "REPLACE YOUR client_secret FROM GOOGLE CONSOLE" + "&scope=" + "https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/user.phonenumbers.read"; ;
            WebRequest accessTokenRequest = WebRequest.Create(apiEndpoint);
            accessTokenRequest.Method = "GET";
            accessTokenRequest.ContentType = "application/json";
            accessTokenRequest.Headers.Add("Authorization", "Bearer " + accesstoken);

            using (WebResponse response = accessTokenRequest.GetResponse())
            {
                string jsonR = string.Empty;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    jsonR = reader.ReadToEnd();
                }
                //user = JsonConvert.DeserializeObject<O365UserModel>(jsonR);

            }
        }


        private string GetAccessTokenByAuthCodeFlow(string code)
        {
            string clientId = "REPLACE YOUR CLIENT ID FROM GOOGLE CONSOLE";
            string appKey = "REPLACE YOUR APPKEY FROM GOOGLE CONSOLE";
            string tenantId = "IF YOU DONT HAVE TENANT REPLACE WITH A GUID";

            string accessToken = string.Empty;
            string apiEndpoint = "https://login.microsoftonline.com/" + tenantId + "/oauth2/v2.0/token";
            WebRequest accessTokenRequest = WebRequest.Create(apiEndpoint);
            accessTokenRequest.Method = "POST";
            accessTokenRequest.ContentType = "application/x-www-form-urlencoded";
            //openid for id_token, offline_access for refresh_token
            string requestParams = "client_id=" + clientId + "&client_secret=" + appKey + "&scope=https://graph.microsoft.com/User.Read openid offline_access" + "&code=" + code + "&redirect_uri=http://localhost/SSOTest/Home/SSORedirect" + "&grant_type=authorization_code";
            byte[] byteArray = Encoding.UTF8.GetBytes(requestParams);
            accessTokenRequest.ContentLength = byteArray.Length;
            Stream dataStream = accessTokenRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            using (WebResponse response = accessTokenRequest.GetResponse())
            {
                string json = string.Empty;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    json = reader.ReadToEnd();
                }
                O365AccessTokenModel accessTokenModel = JsonSerializer.Deserialize<O365AccessTokenModel>(json);
                accessToken = accessTokenModel.access_token;
                //GetAccessTokenByRefreshToken(accessTokenModel.refresh_token);
            }

            return accessToken;
        }



        private O365UserModel GetMeByAccessToken(string acessToken)
        {
            O365UserModel user = new O365UserModel();
            string apiEndpoint = "https://graph.microsoft.com/v1.0/me";
            WebRequest accessTokenRequest = WebRequest.Create(apiEndpoint);
            accessTokenRequest.Method = "GET";
            accessTokenRequest.ContentType = "application/json";
            accessTokenRequest.Headers.Add("Authorization", "Bearer " + acessToken);

            using (WebResponse response = accessTokenRequest.GetResponse())
            {
                string jsonR = string.Empty;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    jsonR = reader.ReadToEnd();
                }
                user = JsonSerializer.Deserialize<O365UserModel>(jsonR);

            }

            return user;
        }



      
    }
}