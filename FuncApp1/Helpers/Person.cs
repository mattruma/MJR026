using Newtonsoft.Json;
using System;

namespace FuncApp1
{
    public class Person
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("person_id")]
        public Guid PersonId => this.Id;

        [JsonProperty("birth_date")]
        public DateTime? BirthDate { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("full_name")]
        public string FullName => $"{this.FirstName} {this.LastName}";

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }
    }
}
