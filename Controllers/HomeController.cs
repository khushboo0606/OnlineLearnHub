using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OnlineLearnHub.Models;

namespace OnlineLearnHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        // GET: Contact
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        // POST: Contact/Submit
        [HttpPost]
        public IActionResult Submit(string name, string email, string message)
        {
            // Check if any of the fields are empty
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message))
            {
                ViewBag.ErrorMessage = "Please fill in all fields.";
                return View("Contact"); // Stay on the Contact view to show the error message
            }

            // If successful, show a success message
            ViewBag.SuccessMessage = "Thank you for your message. We'll get back to you soon!";
            return View("Contact"); // Redirect back to the Contact view to show the success message
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
