using Modelos.Tuneflow.User.Administration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modelos.Tuneflow.User.Consumer;
using Modelos.Tuneflow.User.Production;

namespace Modelos.Tuneflow.User.Profiles
{
    public class Profile
    {
        [Key] public int Id { get; set; } 

        [ForeignKey(nameof(Client))] public int? ClientId { get; set; } 

        [ForeignKey(nameof(Artist))] public int? ArtistId { get; set; } 
        public Client? Client { get; set; } 
        public Artist? Artist { get; set; } 
        public string ProfileImage { get; set; }
        public string Biography { get; set; } 
        public DateTime CreationDate { get; set; } 
    }

}
