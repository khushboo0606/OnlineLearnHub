using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearnHub.Data;
using OnlineLearnHub.Models;

namespace OnlineLearnHub.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/EnrollmentsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetEnrollments()
        {
            try
            {
                var enrollments = await _context.Enrollments
                    .Include(e => e.Course)
                    .Include(e => e.Student)
                    .ToListAsync();

                return Ok(enrollments);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error retrieving enrollments");
            }
        }

        // GET: api/EnrollmentsApi/student/{studentId}/course/{courseId}
        [HttpGet("student/{studentId}/course/{courseId}")]
        public async Task<ActionResult<Enrollment>> GetEnrollment(string studentId, int courseId)
        {
            try
            {
                var enrollment = await _context.Enrollments
                    .Include(e => e.Course)
                    .Include(e => e.Student)
                    .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

                if (enrollment == null)
                {
                    return NotFound($"Enrollment not found for student {studentId} and course {courseId}");
                }

                return Ok(enrollment);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error retrieving enrollment");
            }
        }

        // GET: api/EnrollmentsApi/student/{studentId}
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetStudentEnrollments(string studentId)
        {
            try
            {
                var enrollments = await _context.Enrollments
                    .Include(e => e.Course)
                    .Include(e => e.Student)
                    .Where(e => e.StudentId == studentId)
                    .ToListAsync();

                if (!enrollments.Any())
                {
                    return NotFound($"No enrollments found for student {studentId}");
                }

                return Ok(enrollments);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error retrieving student enrollments");
            }
        }

        // GET: api/EnrollmentsApi/course/{courseId}
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetCourseEnrollments(int courseId)
        {
            try
            {
                var enrollments = await _context.Enrollments
                    .Include(e => e.Course)
                    .Include(e => e.Student)
                    .Where(e => e.CourseId == courseId)
                    .ToListAsync();

                if (!enrollments.Any())
                {
                    return NotFound($"No enrollments found for course {courseId}");
                }

                return Ok(enrollments);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error retrieving course enrollments");
            }
        }

        // POST: api/EnrollmentsApi
        [HttpPost]
        public async Task<ActionResult<Enrollment>> CreateEnrollment(Enrollment enrollment)
        {
            try
            {
                // Validate student exists
                var student = await _context.Users.FindAsync(enrollment.StudentId);
                if (student == null)
                {
                    return BadRequest($"Student with ID {enrollment.StudentId} not found");
                }

                // Validate course exists
                var course = await _context.Courses.FindAsync(enrollment.CourseId);
                if (course == null)
                {
                    return BadRequest($"Course with ID {enrollment.CourseId} not found");
                }

                // Check if already enrolled
                var exists = await _context.Enrollments
                    .AnyAsync(e => e.StudentId == enrollment.StudentId && e.CourseId == enrollment.CourseId);

                if (exists)
                {
                    return BadRequest("Student is already enrolled in this course");
                }

                // Set default values
                enrollment.EnrollmentDate = DateTime.UtcNow;
                enrollment.Status = EnrollmentStatus.Enrolled;

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetEnrollment),
                    new { studentId = enrollment.StudentId, courseId = enrollment.CourseId },
                    enrollment);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error creating enrollment");
            }
        }

        // PUT: api/EnrollmentsApi/student/{studentId}/course/{courseId}
        [HttpPut("student/{studentId}/course/{courseId}")]
        public async Task<IActionResult> UpdateEnrollment(string studentId, int courseId, Enrollment enrollment)
        {
            try
            {
                if (studentId != enrollment.StudentId || courseId != enrollment.CourseId)
                {
                    return BadRequest("Student ID and Course ID must match the URL parameters");
                }

                var existingEnrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

                if (existingEnrollment == null)
                {
                    return NotFound($"Enrollment not found for student {studentId} and course {courseId}");
                }

                existingEnrollment.Status = enrollment.Status;
                
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(studentId, courseId))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error updating enrollment");
            }
        }

        // DELETE: api/EnrollmentsApi/student/{studentId}/course/{courseId}
        [HttpDelete("student/{studentId}/course/{courseId}")]
        public async Task<IActionResult> DeleteEnrollment(string studentId, int courseId)
        {
            try
            {
                var enrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

                if (enrollment == null)
                {
                    return NotFound($"Enrollment not found for student {studentId} and course {courseId}");
                }

                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error deleting enrollment");
            }
        }

        private bool EnrollmentExists(string studentId, int courseId)
        {
            return _context.Enrollments.Any(e => e.StudentId == studentId && e.CourseId == courseId);
        }
    }
}