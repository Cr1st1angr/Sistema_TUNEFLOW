using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modelos.Tuneflow.Playlists;
using Modelos.Tuneflow.Models;

namespace Modelos.Tuneflow.User.Consumer
{
    public class Client : User
    {
        [ForeignKey(nameof(Country))] public int CountryId { get; set; }
        [ForeignKey(nameof(Subscription))] public int SubscriptionId { get; set; }

        public string UserId { get; set; } 
        public Subscription? Subscription { get; set; }
        public List<Playlist>? Playlists { get; set; } 
        public Country? Country { get; set; }
    }

}
