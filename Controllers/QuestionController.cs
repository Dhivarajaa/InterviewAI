using Microsoft.AspNetCore.Mvc;
using InterviewAI.Data;
using InterviewAI.Models;

namespace InterviewAI.Controllers
{
    public class QuestionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuestionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // SHOW ADD QUESTION PAGE
        public IActionResult Add()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || role.ToLower() != "admin")
            {
                return RedirectToAction("Dashboard", "Account");
            }
            return View();
        }

        // HANDLE POST - ADD QUESTION
        [HttpPost]
        public IActionResult Add(Question question)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || role.ToLower() != "admin")
            {
                return RedirectToAction("Dashboard", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(question);
            }

            _context.Questions.Add(question);
            _context.SaveChanges();

            TempData["Success"] = "Question added successfully!";
            return RedirectToAction("Manage");
        }

        // MANAGE QUESTIONS (LIST)
        public IActionResult Manage()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || role.ToLower() != "admin")
            {
                return RedirectToAction("Dashboard", "Account");
            }

            var questions = _context.Questions.OrderByDescending(q => q.Id).ToList();
            return View(questions);
        }

        // EDIT QUESTION - GET
        public IActionResult Edit(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || role.ToLower() != "admin")
            {
                return RedirectToAction("Dashboard", "Account");
            }

            var question = _context.Questions.Find(id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // EDIT QUESTION - POST
        [HttpPost]
        public IActionResult Edit(Question question)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || role.ToLower() != "admin")
            {
                return RedirectToAction("Dashboard", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(question);
            }

            var existingQuestion = _context.Questions.Find(question.Id);
            if (existingQuestion == null)
            {
                return NotFound();
            }

            existingQuestion.QuestionText = question.QuestionText;
            existingQuestion.OptionA = question.OptionA;
            existingQuestion.OptionB = question.OptionB;
            existingQuestion.OptionC = question.OptionC;
            existingQuestion.OptionD = question.OptionD;
            existingQuestion.CorrectAnswer = question.CorrectAnswer;

            _context.SaveChanges();

            TempData["Success"] = "Question updated successfully!";
            return RedirectToAction("Manage");
        }

        // DELETE QUESTION - GET (Confirm)
        public IActionResult Delete(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || role.ToLower() != "admin")
            {
                return RedirectToAction("Dashboard", "Account");
            }

            var question = _context.Questions.Find(id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // DELETE QUESTION - POST
        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || role.ToLower() != "admin")
            {
                return RedirectToAction("Dashboard", "Account");
            }

            var question = _context.Questions.Find(id);
            if (question != null)
            {
                _context.Questions.Remove(question);
                _context.SaveChanges();
                TempData["Success"] = "Question deleted successfully!";
            }

            return RedirectToAction("Manage");
        }

        // TAKE TEST - GET
        public IActionResult TakeTest()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }

            var questions = _context.Questions.ToList();
            
            if (!questions.Any())
            {
                ViewBag.NoQuestions = true;
                return View(new List<Question>());
            }

            // Shuffle questions for variety
            Random random = new Random();
            questions = questions.OrderBy(x => random.Next()).ToList();

            // Store question IDs in session for validation
            HttpContext.Session.SetString("TestQuestionIds", string.Join(",", questions.Select(q => q.Id)));

            return View(questions);
        }

        // TAKE TEST - POST
        [HttpPost]
        public IActionResult TakeTest(List<string> answers, List<int> questionIds)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var questions = _context.Questions.ToList();
            int score = 0;
            int total = questionIds?.Count ?? 0;

            for (int i = 0; i < total; i++)
            {
                if (questionIds != null && i < questionIds.Count && answers != null && i < answers.Count)
                {
                    var question = questions.FirstOrDefault(q => q.Id == questionIds[i]);
                    if (question != null && answers[i] == question.CorrectAnswer)
                    {
                        score++;
                    }
                }
            }

            double percentage = total > 0 ? (double)score / total * 100 : 0;

            var result = new TestResult
            {
                UserEmail = userEmail,
                Score = score,
                TotalQuestions = total,
                Percentage = percentage,
                DateTaken = DateTime.Now
            };

            _context.TestResults.Add(result);
            _context.SaveChanges();

            // Clear session
            HttpContext.Session.Remove("TestQuestionIds");

            ViewBag.Score = score;
            ViewBag.Total = total;
            ViewBag.Percentage = percentage;
            ViewBag.ResultMessage = percentage >= 50 ? "PASS" : "FAIL";

            return View("Result");
        }

        // HISTORY
        public IActionResult History()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var results = _context.TestResults
                .Where(r => r.UserEmail == userEmail)
                .OrderByDescending(r => r.DateTaken)
                .ToList();

            return View(results);
        }

        // LEADERBOARD
        public IActionResult Leaderboard()
        {
            var results = _context.TestResults
                .OrderByDescending(r => r.Percentage)
                .ThenByDescending(r => r.Score)
                .ToList();

            return View(results);
        }
    }
}
