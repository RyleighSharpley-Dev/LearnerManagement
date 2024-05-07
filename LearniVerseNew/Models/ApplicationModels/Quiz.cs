using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Windows.Forms;

namespace LearniVerseNew.Models.ApplicationModels
{
    public class Quiz
    {
        public Guid QuizID { get; set; }
        public string QuizDescription { get; set; }
        public int QuizMaxMark { get; set; }
        public DateTime QuizDate { get; set; }
        public TimeSpan QuizStart { get; set; }
        public TimeSpan QuizEnd { get; set; }
        public DateTime DateCreated { get; set; }
        public int MaxAttempts { get; set; }
        public string Status { get; set; }

        public virtual ICollection<Question> Questions { get; set; }

        public string TeacherID { get; set; }
        public virtual Teacher Teacher { get; set; }

        public string CourseID { get; set; } 
        public virtual Course Course { get; set; } 

        public enum QuizStatus
        {
            Draft,
            Published
        }


        public int MarkTest(Quiz quiz, Dictionary<Guid, string> selectedAnswers)
        {
            int totalScore = 0;

            // Iterate over each question in the quiz
            foreach (var question in quiz.Questions)
            {
                // Check if the question ID exists in the selected answers dictionary
                if (selectedAnswers.ContainsKey(question.QuestionID))
                {
                    string selectedAnswer = selectedAnswers[question.QuestionID];

                    // Check if the selected answer matches the correct answer
                    if (selectedAnswer == question.CorrectAnswer)
                    {
                        // Add the weighting of the question to the total score
                        totalScore += question.Weighting;
                    }
                }
            }

            return totalScore;
        }
    }
}