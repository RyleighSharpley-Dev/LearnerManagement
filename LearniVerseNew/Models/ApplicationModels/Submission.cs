using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LearniVerseNew.Models.ApplicationModels
{
    public class Submission
    {
        public Guid SubmissionID { get; set; }
        public string AssignmentID { get; set; }
        public virtual Assignment Assignment { get; set; }
        public string StudentID { get; set; }
        public virtual Student Student { get; set; }
        public string FileName { get; set; }
        public string BlobUrl { get; set; }
        public int? Mark { get; set; }
    }
}