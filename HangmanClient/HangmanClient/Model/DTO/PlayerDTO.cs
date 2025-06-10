using System;
using System.Runtime.Serialization;

namespace HangmanClient.Model.DTO
{
    [DataContract]
    public class PlayerDTO
    {
        [DataMember]
        public int IdPlayer { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Nickname { get; set; }

        [DataMember]
        public string Password { get; set; } 
        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public DateTime? Birthdate { get; set; } 

        [DataMember]
        public string PhoneNumber { get; set; }

        [DataMember]
        public string ImgRoute { get; set; } 

        [DataMember]
        public int Score { get; set; }
    }
}