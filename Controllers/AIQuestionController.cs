using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InterviewAI.Data;
using InterviewAI.Models;

namespace InterviewAI.Controllers
{
    public class AIQuestionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AIQuestionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || role.ToLower() != "admin")
            {
                return RedirectToAction("Dashboard", "Account");
            }

            ViewBag.Domains = new SelectList(new[] { "C#", "AI", "DBMS", "JavaScript", "Python", "Java" });
            ViewBag.Difficulties = new SelectList(new[] { "Easy", "Medium", "Hard" });
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Generate(string domain, string difficulty, string? keywords)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || role.ToLower() != "admin")
            {
                return RedirectToAction("Dashboard", "Account");
            }

            var questions = GenerateQuestionsFromAI(domain, difficulty, keywords);
            
            foreach (var q in questions)
            {
                q.Id = 0;
                _context.Questions.Add(q);
            }
            await _context.SaveChangesAsync();

            return View("Results", questions);
        }

        public async Task<IActionResult> History()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || role.ToLower() != "admin")
            {
                return RedirectToAction("Dashboard", "Account");
            }

            var questions = await _context.Questions
                .OrderByDescending(q => q.Id)
                .Take(50)
                .ToListAsync();
            return View(questions);
        }

        private List<Question> GenerateQuestionsFromAI(string domain, string difficulty, string? keywords)
        {
            var questions = new List<Question>();
            var questionBank = GetQuestionBank(domain, difficulty, keywords);
            
            var random = new Random();
            var selected = questionBank.OrderBy(x => random.Next()).Take(10).ToList();

            foreach (var q in selected)
            {
                questions.Add(new Question
                {
                    QuestionText = q.QuestionText,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD,
                    CorrectAnswer = q.CorrectAnswer
                });
            }

            return questions;
        }

        private List<Question> GetQuestionBank(string domain, string difficulty, string? keywords)
        {
            var allQuestions = new List<Question>();

            switch (domain.ToUpper())
            {
                case "C#":
                    allQuestions = GetCSharpQuestions(difficulty, keywords);
                    break;
                case "AI":
                    allQuestions = GetAIQuestions(difficulty, keywords);
                    break;
                case "DBMS":
                    allQuestions = GetDBMSQuestions(difficulty, keywords);
                    break;
                case "JAVASCRIPT":
                    allQuestions = GetJavaScriptQuestions(difficulty, keywords);
                    break;
                case "PYTHON":
                    allQuestions = GetPythonQuestions(difficulty, keywords);
                    break;
                case "JAVA":
                    allQuestions = GetJavaQuestions(difficulty, keywords);
                    break;
            }

            return allQuestions;
        }

        private List<Question> GetCSharpQuestions(string difficulty, string? keywords)
        {
            return new List<Question>
            {
                new() { QuestionText = "What is the difference between 'struct' and 'class' in C#?", OptionA = "Struct is value type, Class is reference type", OptionB = "Struct is reference type, Class is value type", OptionC = "They are identical", OptionD = "Struct cannot have methods", CorrectAnswer = "A" },
                new() { QuestionText = "Which keyword is used to inherit a class in C#?", OptionA = "inherits", OptionB = "extends", OptionC = ":", OptionD = "base", CorrectAnswer = "C" },
                new() { QuestionText = "What is LINQ in C#?", OptionA = "Language Integrated Query", OptionB = "Linked Interface Query", OptionC = "Linear Integration Query", OptionD = "Language Interface Query", CorrectAnswer = "A" },
                new() { QuestionText = "What is async/await used for?", OptionA = "Synchronous programming", OptionB = "Asynchronous programming", OptionC = "Multi-threading only", OptionD = "Database connection", CorrectAnswer = "B" },
                new() { QuestionText = "What is a delegate in C#?", OptionA = "A class", OptionB = "A type-safe function pointer", OptionC = "An interface", OptionD = "A variable", CorrectAnswer = "B" },
                new() { QuestionText = "What is the purpose of 'using' statement?", OptionA = "Import namespaces", OptionB = "Resource management/disposal", OptionC = "Namespace alias", OptionD = "All of the above", CorrectAnswer = "D" },
                new() { QuestionText = "What is Entity Framework?", OptionA = "ORM framework", OptionB = "Web framework", OptionC = "Testing framework", OptionD = "Logging framework", CorrectAnswer = "A" },
                new() { QuestionText = "What is difference between IEnumerable and IQueryable?", OptionA = "Same functionality", OptionB = "IQueryable for remote data, IEnumerable for local", OptionC = "IQueryable is faster", OptionD = "IEnumerable is for databases", CorrectAnswer = "B" },
                new() { QuestionText = "What is dependency injection?", OptionA = "Design pattern for loose coupling", OptionB = "Way to create objects", OptionC = "Testing methodology", OptionD = "Data structure", CorrectAnswer = "A" },
                new() { QuestionText = "What is Middleware in ASP.NET Core?", OptionA = "Software that processes requests/responses", OptionB = "Database connector", OptionC = "UI component", OptionD = "Security feature", CorrectAnswer = "A" }
            };
        }

        private List<Question> GetAIQuestions(string difficulty, string? keywords)
        {
            return new List<Question>
            {
                new() { QuestionText = "What is Machine Learning?", OptionA = "Rule-based programming", OptionB = "Learning from data without explicit programming", OptionC = "Artificial Intelligence only", OptionD = "Database management", CorrectAnswer = "B" },
                new() { QuestionText = "What is overfitting?", OptionA = "Model performs well on training data", OptionB = "Model performs poorly on training data", OptionC = "Model memorizes training data", OptionD = "Model is too simple", CorrectAnswer = "C" },
                new() { QuestionText = "What is the activation function in neural networks?", OptionA = "Network connector", OptionB = "Function that determines neuron output", OptionC = "Data preprocessing", OptionD = "Weight initializer", CorrectAnswer = "B" },
                new() { QuestionText = "What is supervised learning?", OptionA = "Learning without labels", OptionB = "Learning with labeled data", OptionC = "Reinforcement learning", OptionD = "Unsupervised clustering", CorrectAnswer = "B" },
                new() { QuestionText = "What is a CNN used for?", OptionA = "Text processing", OptionB = "Image recognition", OptionC = "Audio processing", OptionD = "Time series", CorrectAnswer = "B" },
                new() { QuestionText = "What is gradient descent?", OptionA = "Weight update algorithm", OptionB = "Data augmentation", OptionC = "Model evaluation", OptionD = "Feature selection", CorrectAnswer = "A" },
                new() { QuestionText = "What is NLP?", OptionA = "New Programming Language", OptionB = "Natural Language Processing", OptionC = "Network Learning Protocol", OptionD = "Node Link Protocol", CorrectAnswer = "B" },
                new() { QuestionText = "What is transfer learning?", OptionA = "Moving data between servers", OptionB = "Using pre-trained model knowledge", OptionC = "Data transfer protocol", OptionD = "Learning rate transfer", CorrectAnswer = "B" },
                new() { QuestionText = "What is a loss function?", OptionA = "Measures model error", OptionB = "Network architecture", OptionC = "Data loader", OptionD = "Optimizer", CorrectAnswer = "A" },
                new() { QuestionText = "What is backpropagation?", OptionA = "Forward data flow", OptionB = "Gradient calculation for weights", OptionC = "Model deployment", OptionD = "Data preprocessing", CorrectAnswer = "B" }
            };
        }

        private List<Question> GetDBMSQuestions(string difficulty, string? keywords)
        {
            return new List<Question>
            {
                new() { QuestionText = "What is a primary key?", OptionA = "First column in table", OptionB = "Unique identifier for records", OptionC = "Foreign key reference", OptionD = "Index only", CorrectAnswer = "B" },
                new() { QuestionText = "What is ACID property?", OptionA = "Atomicity, Consistency, Isolation, Durability", OptionB = "Action, Command, Index, Data", OptionC = "Access, Control, Integration, Deployment", OptionD = "Auto, Create, Insert, Delete", CorrectAnswer = "A" },
                new() { QuestionText = "What is normalization?", OptionA = "Data encryption", OptionB = "Organizing data to reduce redundancy", OptionC = "Data compression", OptionD = "Data backup", CorrectAnswer = "B" },
                new() { QuestionText = "What is a foreign key?", OptionA = "Key outside the database", OptionB = "Reference to another table's primary key", OptionC = "Backup key", OptionD = "Encryption key", CorrectAnswer = "B" },
                new() { QuestionText = "What is SQL JOIN?", OptionA = "Database connection", OptionB = "Combines rows from two tables", OptionC = "Table deletion", OptionD = "Index creation", CorrectAnswer = "B" },
                new() { QuestionText = "What is an index in database?", OptionA = "Table copy", OptionB = "Data structure for fast retrieval", OptionC = "Backup mechanism", OptionD = "User permission", CorrectAnswer = "B" },
                new() { QuestionText = "What is a transaction?", OptionA = "User session", OptionB = "Unit of work with ACID properties", OptionC = "Database backup", OptionD = "Query result", CorrectAnswer = "B" },
                new() { QuestionText = "What is NoSQL?", OptionA = "Non-relational databases", OptionB = "No SQL at all", OptionC = "New SQL version", OptionD = "SQL optimization", CorrectAnswer = "A" },
                new() { QuestionText = "What is database sharding?", OptionA = "Data encryption", OptionB = "Horizontal partitioning of data", OptionC = "Data compression", OptionD = "Index optimization", CorrectAnswer = "B" },
                new() { QuestionText = "What is CAP theorem?", OptionA = "Consistency, Availability, Partition tolerance", OptionB = "Create, Update, Process", OptionC = "Cache, Access, Protocol", OptionD = "Connect, Apply, Perform", CorrectAnswer = "A" }
            };
        }

        private List<Question> GetJavaScriptQuestions(string difficulty, string? keywords)
        {
            return new List<Question>
            {
                new() { QuestionText = "What is closures in JavaScript?", OptionA = "Loop structure", OptionB = "Function with access to outer scope", OptionC = "Error handling", OptionD = "Data type", CorrectAnswer = "B" },
                new() { QuestionText = "What is Promise in JavaScript?", OptionA = "Data structure", OptionB = "Async operation handler", OptionC = "Variable type", OptionD = "Loop construct", CorrectAnswer = "B" },
                new() { QuestionText = "What is the difference between let and var?", OptionA = "Same functionality", OptionB = "let has block scope, var has function scope", OptionC = "var is faster", OptionD = "let is deprecated", CorrectAnswer = "B" },
                new() { QuestionText = "What is hoisting?", OptionA = "CSS property", OptionB = "Moving declarations to top of scope", OptionC = "Error type", OptionD = "Data structure", CorrectAnswer = "B" },
                new() { QuestionText = "What is prototype chain?", OptionA = "Network structure", OptionB = "Object inheritance mechanism", OptionC = "Memory management", OptionD = "Event handling", CorrectAnswer = "B" },
                new() { QuestionText = "What is event delegation?", OptionA = "Event removal", OptionB = "Handling events at parent element", OptionC = "Event creation", OptionD = "Event blocking", CorrectAnswer = "B" },
                new() { QuestionText = "What is async/await?", OptionA = "Loop construct", OptionB = "Syntax for async operations", OptionC = "Data type", OptionD = "Error handler", CorrectAnswer = "B" },
                new() { QuestionText = "What is destructuring?", OptionA = "Object creation", OptionB = "Unpacking values from arrays/objects", OptionC = "Code compression", OptionD = "Module loading", CorrectAnswer = "B" },
                new() { QuestionText = "What is spread operator?", OptionA = "Mathematical operation", OptionB = "... syntax for expanding iterables", OptionC = "Error type", OptionD = "Loop construct", CorrectAnswer = "B" },
                new() { QuestionText = "What is arrow function?", OptionA = "Array method", OptionB = "Concise function syntax with lexical this", OptionC = "Data structure", OptionD = "Event type", CorrectAnswer = "B" }
            };
        }

        private List<Question> GetPythonQuestions(string difficulty, string? keywords)
        {
            return new List<Question>
            {
                new() { QuestionText = "What is GIL in Python?", OptionA = "Global Interpreter Lock", OptionB = "General Input Language", OptionC = "Garbage Collection", OptionD = "Module type", CorrectAnswer = "A" },
                new() { QuestionText = "What is list comprehension?", OptionA = "List creation syntax", OptionB = "Data structure", OptionC = "Memory optimization", OptionD = "Type conversion", CorrectAnswer = "A" },
                new() { QuestionText = "What is pip?", OptionA = "Package installer", OptionB = "Python interpreter", OptionC = "IDE", OptionD = "Data structure", CorrectAnswer = "A" },
                new() { QuestionText = "What is virtual environment?", OptionA = "OS virtualization", OptionB = "Isolated Python environment", OptionC = "Cloud service", OptionD = "IDE feature", CorrectAnswer = "B" },
                new() { QuestionText = "What is decorator?", OptionA = "Design pattern", OptionB = "Modifies function behavior", OptionC = "Data structure", OptionD = "Import statement", CorrectAnswer = "B" },
                new() { QuestionText = "What is lambda?", OptionA = "Cloud service", OptionB = "Anonymous function", OptionC = "Data structure", OptionD = "Package manager", CorrectAnswer = "B" },
                new() { QuestionText = "What is Django?", OptionA = "Database", OptionB = "Python web framework", OptionC = "IDE", OptionD = "Data science library", CorrectAnswer = "B" },
                new() { QuestionText = "What is pandas used for?", OptionA = "Web development", OptionB = "Data manipulation/analysis", OptionC = "Game development", OptionD = "Networking", CorrectAnswer = "B" },
                new() { QuestionText = "What is self in Python?", OptionA = "Global variable", OptionB = "Reference to instance", OptionC = "Module name", OptionD = "Function name", CorrectAnswer = "B" },
                new() { QuestionText = "What is __init__?", OptionA = "Destructor", OptionB = "Constructor method", OptionC = "Main function", OptionD = "Import statement", CorrectAnswer = "B" }
            };
        }

        private List<Question> GetJavaQuestions(string difficulty, string? keywords)
        {
            return new List<Question>
            {
                new() { QuestionText = "What is JVM?", OptionA = "Java Virtual Machine", OptionB = "Java Variable Manager", OptionC = "Java Version Manager", OptionD = "Java Visual Module", CorrectAnswer = "A" },
                new() { QuestionText = "What is the difference between JDK and JRE?", OptionA = "Same thing", OptionB = "JDK includes JRE plus development tools", OptionC = "JRE is larger", OptionD = "JDK is for runtime only", CorrectAnswer = "B" },
                new() { QuestionText = "What is polymorphism?", OptionA = "Data type", OptionB = "Multiple forms/methods", OptionC = "Package type", OptionD = "Access modifier", CorrectAnswer = "B" },
                new() { QuestionText = "What is interface in Java?", OptionA = "UI component", OptionB = "Contract with abstract methods", OptionC = "Data structure", OptionD = "Class type", CorrectAnswer = "B" },
                new() { QuestionText = "What is exception handling?", OptionA = "Error prevention", OptionB = "Handling runtime errors", OptionC = "Compilation process", OptionD = "Memory management", CorrectAnswer = "B" },
                new() { QuestionText = "What is Spring Framework?", OptionA = "Hardware component", OptionB = "Java enterprise framework", OptionC = "Database", OptionD = "IDE", CorrectAnswer = "B" },
                new() { QuestionText = "What is Maven?", OptionA = "Build tool/dependency manager", OptionB = "Java compiler", OptionC = "IDE", OptionD = "Database", CorrectAnswer = "A" },
                new() { QuestionText = "What is the difference between ArrayList and LinkedList?", OptionA = "Same performance", OptionB = "ArrayList is index-based, LinkedList is node-based", OptionC = "LinkedList is faster always", OptionD = "ArrayList uses less memory", CorrectAnswer = "B" },
                new() { QuestionText = "What is garbage collection?", OptionA = "Manual memory management", OptionB = "Automatic memory cleanup", OptionC = "Database operation", OptionD = "Code compilation", CorrectAnswer = "B" },
                new() { QuestionText = "What is multithreading?", OptionA = "Single task execution", OptionB = "Concurrent task execution", OptionC = "Database queries", OptionD = "Network communication", CorrectAnswer = "B" }
            };
        }
    }
}
