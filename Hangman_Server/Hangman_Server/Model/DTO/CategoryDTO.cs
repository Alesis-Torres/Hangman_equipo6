using System.Runtime.Serialization;

namespace Hangman_Server.Model.DTO
{
    [DataContract]
    public class CategoryDTO
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public byte[] ImageBytes { get; set; }
    }
}