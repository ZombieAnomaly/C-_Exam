using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace C_Exam.Models
{
    public class Users :DbContext
    {
        [Key]
        public int UserId {get; set;}
        public string FirstName {get; set;}
        public string LastName {get; set;}
        public string Email {get; set;}
        public string Password {get; set;}
        
        public List<Activities> CreatedActivities { get; set; }
        public List<Participants> Joined {get; set;}
        public Users()
        {
            CreatedActivities = new List<Activities>();
            Joined = new List<Participants>();
        }

    }
}