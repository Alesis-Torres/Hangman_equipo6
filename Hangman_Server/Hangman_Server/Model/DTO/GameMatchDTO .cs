using System.Runtime.Serialization;

namespace HangmanClient.Model.DTO
{
    [DataContract]
    public class GameMatchDTO
    {
        [DataMember]
        public int IdPlayerChallenger { get; set; }

        [DataMember]
        public int IdPlayerGuesser { get; set; }

        [DataMember]
        public int IdWord { get; set; }

        [DataMember]
        public int IdGameMatchStatus { get; set; }
    }
}