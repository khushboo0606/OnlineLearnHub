using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearnHub.Data;
using OnlineLearnHub.Models;

namespace OnlineLearnHub.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(
            ApplicationDbContext context, 
            UserManager<User> userManager,
            ILogger<CoursesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Courses
        public async Task<IActionResult> Index(string searchString)
        {
    try
    {
        var courses = _context.Courses.Include(c => c.Instructor).AsQueryable();

        if (!String.IsNullOrEmpty(searchString))
        {
            courses = courses.Where(s => (s.Title != null && s.Title.Contains(searchString))
                                   || (s.Description != null && s.Description.Contains(searchString)));
        }

        var coursesList = await courses.ToListAsync();

        // If user is a student, check enrollment status for each course
        if (User.IsInRole("Student"))
        {
            var userId = _userManager.GetUserId(User);
            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == userId)
                .Select(e => e.CourseId)
                .ToListAsync();

            foreach (var course in coursesList)
            {
                ViewData[$"Enrolled_{course.Id}"] = enrollments.Contains(course.Id);
            }
        }

        return View(coursesList);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving courses list");
        TempData["ErrorMessage"] = "An error occurred while retrieving courses.";
        return View(new List<Course>());
    }
}

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var course = await _context.Courses
                    .Include(c => c.Instructor)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (course == null)
                {
                    return NotFound();
                }

                var currentUser = await _userManager.GetUserAsync(User);
                ViewBag.IsEnrolled = false;

                if (currentUser != null && await _userManager.IsInRoleAsync(currentUser, "Student"))
                {
                    ViewBag.IsEnrolled = await _context.Enrollments
                        .AnyAsync(e => e.StudentId == currentUser.Id && e.CourseId == id);
                }

                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course details for ID: {CourseId}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving course details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Courses/Create
        [Authorize(Roles = "Admin,Instructor")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Create([Bind("Title,Description")] Course course)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser != null)
                    {
                        course.InstructorId = currentUser.Id;
                        course.CreatedAt = DateTime.Now;
                        course.UpdatedAt = DateTime.Now;
                        
                        _context.Add(course);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Course created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "User not found.");
                    }
                }
                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the course.");
                return View(course);
            }
        }

        // GET: Courses/Edit/5
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            // Only allow instructors to edit their own courses (admins can edit any)
            if (!User.IsInRole("Admin") && course.InstructorId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var existingCourse = await _context.Courses.FindAsync(id);
                    if (existingCourse == null)
                    {
                        return NotFound();
                    }

                    // Only allow instructors to edit their own courses (admins can edit any)
                    if (!User.IsInRole("Admin") && existingCourse.InstructorId != _userManager.GetUserId(User))
                    {
                        return Forbid();
                    }

                    existingCourse.Title = course.Title;
                    existingCourse.Description = course.Description;
                    existingCourse.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Course updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                return View(course);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!CourseExists(course.Id))
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, "Concurrency error updating course {CourseId}", id);
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the course.");
                    return View(course);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course {CourseId}", id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the course.");
                return View(course);
            }
        }

        // GET: Courses/Delete/5
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var course = await _context.Courses
                    .Include(c => c.Instructor)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (course == null)
                {
                    return NotFound();
                }

                // Only allow instructors to delete their own courses (admins can delete any)
                if (!User.IsInRole("Admin") && course.InstructorId != _userManager.GetUserId(User))
                {
                    return Forbid();
                }

                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course for deletion {CourseId}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving the course.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course != null)
                {
                    // Only allow instructors to delete their own courses (admins can delete any)
                    if (!User.IsInRole("Admin") && course.InstructorId != _userManager.GetUserId(User))
                    {
                        return Forbid();
                    }

                    _context.Courses.Remove(course);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Course deleted successfully.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course {CourseId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the course.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Courses/Enroll/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Enroll(int id)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                {
                    TempData["ErrorMessage"] = "Course not found.";
                    return RedirectToAction(nameof(Index));
                }

                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var existingEnrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.StudentId == userId && e.CourseId == id);

                if (existingEnrollment != null)
                {
                    TempData["ErrorMessage"] = "You are already enrolled in this course.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var enrollment = new Enrollment
                {
                    StudentId = userId,
                    CourseId = id,
                    EnrollmentDate = DateTime.Now,
                    Status = EnrollmentStatus.Enrolled
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "You have successfully enrolled in the course.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enrolling in course {CourseId}", id);
                TempData["ErrorMessage"] = "An error occurred while enrolling in the course.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: Courses/MyEnrollments
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyEnrollments()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return NotFound();
                }

                var enrolledCourses = await _context.Enrollments
                    .Where(e => e.StudentId == userId)
                    .Include(e => e.Course)
                    .ThenInclude(c => c!.Instructor)
                    .Select(e => e.Course)
                    .ToListAsync();

                return View(enrolledCourses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving enrolled courses");
                TempData["ErrorMessage"] = "An error occurred while retrieving your enrolled courses.";
                return View(new List<Course>());
            }
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}