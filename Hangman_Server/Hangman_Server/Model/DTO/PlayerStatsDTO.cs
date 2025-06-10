using System.Runtime.Serialization;

[DataContract]
public class PlayerStatsDTO
{
    [DataMember] public string Username { get; set; }
    [DataMember] public int GamesPlayed { get; set; }
    [DataMember] public int Wins { get; set; }
}