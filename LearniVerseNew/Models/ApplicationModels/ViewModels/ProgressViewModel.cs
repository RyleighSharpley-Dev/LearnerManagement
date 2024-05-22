using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LearniVerseNew.Models.ApplicationModels.ViewModels
{
    public class ProgressViewModel
    {
        public List<QuizAttempt> QuizAttempts { get; set; }
        public List<Submission> Submissions { get; set; }
        public List<CourseQuizAttempts> CoursesQuizAttempts { get; set; }
        public int HighestMark { get; set; }
        public double AverageMark { get; set; }
        public double? AverageSubmissionMark { get; set; }
    }
}