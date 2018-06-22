using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace C_Exam.Models
{
    public class Participants
    {
        [Key]
        public int ParticipantsId {get; set;}
        public int UserId {get; set;}
        public Users User {get; set;}

        public int ActivityId {get; set;}
        public Activities Activity {get; set;}

    }
}