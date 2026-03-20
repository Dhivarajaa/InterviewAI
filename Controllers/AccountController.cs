using Microsoft.AspNetCore.Mvc;
using InterviewAI.Data;
using InterviewAI.Models;
using System.Linq;

namespace InterviewAI.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // SHOW REGISTER PAGE
        public IActionResult Register()
        {
            return View();
        }

        // HANDLE REGISTER POST
       [HttpPost]
public IActionResult Register(User user)
{
    // Prevent duplicate email
    if (_context.Users.Any(u => u.Email == user.Email))
    {
        ViewBag.Error = "Email already exists!";
        return View(user);
    }

    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

    // If no users exist → first user becomes Admin
    if (!_context.Users.Any())
        user.Role = "Admin";
    else
        user.Role = "Student";

    _context.Users.Add(user);
    _context.SaveChanges();

    return RedirectToAction("Login");
}
        // SHOW LOGIN PAGE
        public IActionResult Login()
        {
            return View();
        }

        // HANDLE LOGIN POST
        [HttpPost]
        public IActionResult Login(User user)
        {
            ViewBag.Debug = $"Email: '{user.Email}', Password length: {user.PasswordHash?.Length ?? 0}";
            
            try
            {
                if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.PasswordHash))
                {
                    ViewBag.Error = "Email and Password are required";
                    return View(user);
                }

                var usersWithEmail = _context.Users
                    .Where(u => u.Email.ToLower() == user.Email.ToLower())
                    .ToList();

                ViewBag.Debug += $", Users found: {usersWithEmail.Count}";

                if (usersWithEmail.Count == 0)
                {
                    ViewBag.Error = "Invalid Email or Password";
                    return View(user);
                }

                User? existingUser = null;
                foreach (var u in usersWithEmail)
                {
                    if (BCrypt.Net.BCrypt.Verify(user.PasswordHash, u.PasswordHash))
                    {
                        existingUser = u;
                        break;
                    }
                }

                if (existingUser != null)
                {
                    if (usersWithEmail.Count > 1)
                    {
                        var duplicatesToRemove = usersWithEmail.Where(u => u.Id != existingUser.Id).ToList();
                        _context.Users.RemoveRange(duplicatesToRemove);
                        _context.SaveChanges();
                    }

                    HttpContext.Session.SetString("UserEmail", existingUser.Email);
                    HttpContext.Session.SetString("UserRole", existingUser.Role);
                    HttpContext.Session.SetString("UserId", existingUser.Id.ToString());
                    return RedirectToAction("Dashboard");
                }

                ViewBag.Error = "Invalid Email or Password";
                return View(user);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Login error: " + ex.Message;
                return View(user);
            }
        }


        // DASHBOARD
 public IActionResult Dashboard()
{
    var email = HttpContext.Session.GetString("UserEmail");

    if (email == null)
    {
        return RedirectToAction("Login");
    }

    var user = _context.Users
        .FirstOrDefault(u => u.Email == email);

    return View(user);
}
public IActionResult Logout()
{
    HttpContext.Session.Clear();
    return RedirectToAction("Login");
}
// PROMOTE USER TO ADMIN
public IActionResult MakeAdmin(string email)
{
    var role = HttpContext.Session.GetString("UserRole");

    if (role != "Admin")
    {
        return Content("Access Denied");
    }

    var user = _context.Users
        .FirstOrDefault(u => u.Email == email);

    if (user != null)
    {
        user.Role = "Admin";
        _context.SaveChanges();
        return Content("User promoted to Admin successfully!");
    }

    return Content("User not found.");
}

// REMOVE DUPLICATE USERS BY EMAIL
public IActionResult CleanDuplicateUsers(string email)
{
    var users = _context.Users
        .Where(u => u.Email == email)
        .OrderBy(u => u.Id)
        .ToList();

    if (users.Count > 1)
    {
        var usersToDelete = users.Skip(1).ToList();
        _context.Users.RemoveRange(usersToDelete);
        _context.SaveChanges();

        return Content("Duplicate users removed successfully!");
    }

    return Content("No duplicates found.");
}
public IActionResult Users()
{
    var role = HttpContext.Session.GetString("UserRole");

    if (role != "Admin")
    {
        return RedirectToAction("Dashboard");
    }

    var users = _context.Users.ToList();

    return View(users);
}
public IActionResult FixAdmin()
{
    var users = _context.Users
        .Where(u => u.Email == "dhivaraja6066@gmail.com")
        .ToList();

    foreach (var user in users)
    {
        user.Role = "Admin";
    }

    _context.SaveChanges();

    return Content("Admin role fixed.");
}

public IActionResult ResetAdminPassword(string password)
{
    var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == "dhivaraja6066@gmail.com");
    if (user != null)
    {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        _context.SaveChanges();
        return Content("Admin password reset to: " + password);
    }
    return Content("Admin user not found");
}

public IActionResult DeleteAdminDuplicates()
{
    var users = _context.Users
        .Where(u => u.Email == "dhivaraja6066@gmail.com")
        .ToList();

    _context.Users.RemoveRange(users);
    _context.SaveChanges();

    return Content("All duplicates removed.");
}

public IActionResult TestLogin(string email, string password)
{
    var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
    if (user == null)
        return Content($"User not found for email: {email}");
    
    bool passwordMatch = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    return Content($"User found: {user.Email}, Role: {user.Role}, Password match: {passwordMatch}");
}

public IActionResult DirectLogin(string email, string password)
{
    var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
    if (user == null)
        return Content("User not found");
    
    if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        return Content("Invalid password");
    
    HttpContext.Session.SetString("UserEmail", user.Email);
    HttpContext.Session.SetString("UserRole", user.Role);
    return RedirectToAction("Dashboard");
}
}
}