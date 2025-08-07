using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modelos.Tuneflow.Media;
using Modelos.Tuneflow.User.Production;

namespace Modelos.Tuneflow.Playlists
{
    public class Album
    {
        [Key] public int Id { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Genre { get; set; } 
        public DateTime CreationDate { get; set; } 
        public string Description { get; set; } 
        public string CoverPath { get; set; } 
        public List<Song>? Songs { get; set; } 
        public int ArtistId { get; set; } 
        public Artist? Artist { get; set; } 
    }

}
