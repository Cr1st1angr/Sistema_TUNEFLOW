using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modelos.Tuneflow.Media;
using Modelos.Tuneflow.Playlists;
using Modelos.Tuneflow.Models;
using Modelos.Tuneflow.User.Profiles;

namespace Modelos.Tuneflow.User.Production
{
    public class Artist : User
    {
        public string StageName { get; set; }
        public string MusicGenre { get; set; }
        [ForeignKey(nameof(Country))] public int CountryId { get; set; }
        public bool Verified { get; set; } 
        public string UserId { get; set; } 
        public Country? Country { get; set; }
        public Profile? Profile { get; set; } 
        public List<Song>? Songs { get; set; }
        public List<Album>? Albums { get; set; }
    }

}
