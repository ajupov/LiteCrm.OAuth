using System;

namespace LiteCrm.OAuth.Models
{
    public class UserInfoResponse
    {
        public Guid id { get; set; }

        public string email { get; set; }

        public string phone { get; set; }

        public string surname { get; set; }

        public string name { get; set; }

        public string gender { get; set; }

        public string birth_date { get; set; }

        public string error { get; set; }

        public bool HasError => !string.IsNullOrWhiteSpace(error);
    }
}