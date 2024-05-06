using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LearniVerseNew.Models.ApplicationModels
{
    public class Question
    {
        public Guid QuestionID { get; set; }
        public string QuestionText { get; set; }
        public string AnswerA { get; set; } 
        public string AnswerB { get; set; }
        public string AnswerC { get; set; }
        public string AnswerD { get; set; }
        public char CorrectAnswer { get; set; }
        public Guid QuizID { get; set; }

        public virtual Quiz Quiz { get; set; }
    }
}