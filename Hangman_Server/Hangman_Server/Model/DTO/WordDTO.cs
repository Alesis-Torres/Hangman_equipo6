﻿using System.Runtime.Serialization;

namespace Hangman_Server.Model.DTO
{
    [DataContract]
    public class WordDTO
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Hint { get; set; }

        [DataMember]
        public byte[] ImageBytes { get; set; }
    }
}