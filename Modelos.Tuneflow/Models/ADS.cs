using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modelos.Tuneflow.Models
{
    public class ADS
    {
        [Key]public int Id { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; } 
        public string FilePath { get; set; } 
        public string ImagePath { get; set; } 

        public string TimeInMinutes(int time)
        {
            int minutes = time / 60;
            int seconds = time % 60;
            return $"{minutes}:{seconds:D2}";
        }
    }
}
