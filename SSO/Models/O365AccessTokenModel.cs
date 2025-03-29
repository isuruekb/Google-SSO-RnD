namespace SSO.Models
{
    public class O365AccessTokenModel
    {
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public int ext_expires_in { get; set; }
        public string access_token { get; set; }

        public string id_token { get; set; }

        public string refresh_token { get; set; }

        public string scope { get; set; }
    }



    public class O365UserModel
    {
        public string id { get; set; }
        public string userPrincipalName { get; set; }
        public string surname { get; set; }
        public string preferredLanguage { get; set; }
        public string officeLocation { get; set; }
        public string mobilePhone { get; set; }
        public string mail { get; set; }
        public string jobTitle { get; set; }
        public string givenName { get; set; }
        public string displayName { get; set; }
        public List<string> businessPhones { get; set; }
    }

    public class authRes
    {
        public string accessToken { get; set; }
        public string userJson { get; set;}
    }


    public class GooglePostModel
    {
        public string code { get; set; }

        public string id_token { get; set; }
        public string state { get; set; }
    }


    public class GoogleUserModel
    {
        public string name { get; set; }

        public string given_name { get; set; }

        public string family_name { get; set; }

        public string email { get; set; }

        public string picture { get; set; }
    }
}
