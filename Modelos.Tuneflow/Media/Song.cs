using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modelos.Tuneflow.Playlists;
using Modelos.Tuneflow.User.Production;

namespace Modelos.Tuneflow.Media
{
    public class Song
    {
        [Key] public int Id { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public string Genre { get; set; }
        [ForeignKey(nameof(Artist))] public int ArtistId { get; set; }
        [ForeignKey(nameof(Album))] public int? AlbumId { get; set; }
        public string FilePath { get; set; }
        public bool ExplicitContent { get; set; }
        public string ImagePath { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool Available { get; set; } 
        public Artist? Artist { get; set; }
        public Album? Album { get; set; }

        public string TimeInMinutes(int time)
        {
            int minutes = time / 60;
            int seconds = time % 60;
            return $"{minutes}:{seconds:D2}";
        }
    }
}

