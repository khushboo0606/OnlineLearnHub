using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearnHub.Data;
using OnlineLearnHub.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineLearnHub.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (await _userManager.IsInRoleAsync(user, "Student"))
            {
                return await StudentDashboard(user.Id);
            }
            else if (await _userManager.IsInRoleAsync(user, "Instructor"))
            {
                return await InstructorDashboard(user.Id);
            }
            else if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return await AdminDashboard();
            }

            return NotFound();
        }

        private async Task<IActionResult> StudentDashboard(string userId)
        {
            var enrolledCourses = await _context.Enrollments
        .Where(e => e.StudentId == userId && e.Course != null)
        .Select(e => e.Course)
        .Select(c => new
        {
            Id = c!.Id,  // Use null-forgiving operator
            Title = c.Title ?? "Untitled Course",
            Description = c.Description ?? "No description available"
        })
        .ToListAsync();

    ViewBag.EnrolledCourses = enrolledCourses;

    var enrolledCourseIds = enrolledCourses.Select(c => c.Id).ToList();

    var recommendedCourses = await _context.Courses
        .Where(c => !enrolledCourseIds.Contains(c.Id))
        .Select(c => new
        {
            Id = c.Id,
            Title = c.Title ?? "Untitled Course",
            Description = c.Description ?? "No description available"
        })
        .Take(2)
        .ToListAsync();

    ViewBag.RecommendedCourses = recommendedCourses;

    return View("StudentDashboard");
        }

        private async Task<IActionResult> InstructorDashboard(string userId)
        {
            var taughtCourses = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .ToListAsync();

            return View("InstructorDashboard", taughtCourses);
        }

        private async Task<IActionResult> AdminDashboard()
        {
            var allCourses = await _context.Courses.Include(c => c.Instructor).ToListAsync();
            return View("AdminDashboard", allCourses);
        }
    }
}