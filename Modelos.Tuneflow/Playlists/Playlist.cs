using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modelos.Tuneflow.User.Consumer;
using Modelos.Tuneflow.Media;

namespace Modelos.Tuneflow.Playlists
{
    public class Playlist
    {
        [Key] public int Id { get; set; }
        public string Title { get; set; } 
        public string Description { get; set; } 
        public DateTime CreationDate { get; set; } 
        [ForeignKey(nameof(Client))] public int ClientId { get; set; }
        public string PlaylistCover { get; set; }
        public Client? Client { get; set; }
        public List<Song>? Songs { get; set; } 
    }

}
