//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Hangman_Server.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class gamematch
    {
        public int id_gamematch { get; set; }
        public string code { get; set; }
        public Nullable<int> id_player_challenger { get; set; }
        public Nullable<int> id_player_guesser { get; set; }
        public Nullable<int> id_word { get; set; }
        public Nullable<int> id_playerinfo { get; set; }
        public Nullable<int> id_gamematch_status { get; set; }
        public Nullable<System.DateTime> date_finished { get; set; }
    
        public virtual gamematch_status gamematch_status { get; set; }
        public virtual player player { get; set; }
        public virtual player player1 { get; set; }
        public virtual player player2 { get; set; }
        public virtual word word { get; set; }
    }
}
