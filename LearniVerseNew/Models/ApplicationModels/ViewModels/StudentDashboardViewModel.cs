using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LearniVerseNew.Models.ApplicationModels.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string UserName { get; set; }
        public string Faculty { get; set; }
        public string Qualification { get; set; }
        public List<Enrollment> Enrollments { get; set; }
        public List<Payment> Payments { get; set; }
        
    }
}