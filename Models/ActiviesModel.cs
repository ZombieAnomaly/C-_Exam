using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace C_Exam.Models
{
    public class Activities
    {
        [Key]
        public int ActivityId {get; set;}
        public string Title {get; set;}
        public string Description {get; set;}
        public DateTime Time {get; set;}
        public int Duration {get; set;}

        public int UsersId {get; set;}
        public Users Owner {get; set;}
        public List<Participants> Participants {get; set;}

        public Activities(){
            Participants = new List<Participants>();
        }

    }
}