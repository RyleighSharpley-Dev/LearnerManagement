using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LearniVerseNew.Models.ApplicationModels.ViewModels
{
    public class ClassroomViewModel
    {
        public Course Course { get; set; }
        public List<Resource> Resources { get; set; }
    }
}