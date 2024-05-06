using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LearniVerseNew.Models.ApplicationModels
{
    public class QuizAttempt
    {
        public Guid QuizAttemptID { get; set; }
        public DateTime AttemptDate { get; set; }
        public int MarkObtained { get; set; }

        public Guid QuizID { get; set; }
        public virtual Quiz Quiz { get; set; }

        public Guid StudentID { get; set; }
        public virtual Student Student { get; set; }
    }
}