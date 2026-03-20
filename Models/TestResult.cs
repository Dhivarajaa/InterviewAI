using System;
using System.ComponentModel.DataAnnotations;

namespace InterviewAI.Models
{
    public class TestResult
    {
        public int Id { get; set; }

        public string UserEmail { get; set; } = string.Empty;

        public int Score { get; set; }

        public int TotalQuestions { get; set; }

        public double Percentage { get; set; }

        public DateTime DateTaken { get; set; }
    }
}