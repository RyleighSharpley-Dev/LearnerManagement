using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LearniVerseNew.Models.ApplicationModels
{
    public class Student
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Student ID")]
        public string StudentID { get; set; }

        [Display(Name = "First Name")]
        public string StudentFirstName { get; set; }

        [Display(Name = "Last Name")]
        public string StudentLastName { get; set; }

        [Display(Name = "Email")]
        public string StudentEmail { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Display(Name = "Date of Birth")]
        public DateTime DOB { get; set; }

        public string FacultyID { get; set; }
        public string QualificationID { get; set; }

        public virtual Faculty Faculty { get; set; }
        public virtual Qualification Qualification { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }

        public virtual ICollection<QuizAttempt> QuizAttempts { get; set; }
    }
}