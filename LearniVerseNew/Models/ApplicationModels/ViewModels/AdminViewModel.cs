using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LearniVerseNew.Models.ApplicationModels.ViewModels
{
    public class AdminViewModel
    {
        public int StudentCount { get; set; }
        public int CourseCount { get; set; }
        public int TeacherCount { get; set; }
        public decimal Revenue { get; set; }
    }
}