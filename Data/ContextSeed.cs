using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineLearnHub.Models;

namespace OnlineLearnHub.Data
{
    public static class ContextSeed
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            await CreateRoleIfNotExists(roleManager, UserRoles.Admin);
            await CreateRoleIfNotExists(roleManager, UserRoles.Instructor);
            await CreateRoleIfNotExists(roleManager, UserRoles.Student);
        }

        private static async Task CreateRoleIfNotExists(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                Console.WriteLine($"{roleName} role created successfully.");
            }
        }

        public static async Task SeedUsersAsync(UserManager<User> userManager)
        {
            var users = new List<(string Email, string Password, string Role, string FirstName, string LastName)>
            {
                ("admin@learnhub.com", "Admin@123456", UserRoles.Admin, "Admin", "User"),
                
                // Instructors
                ("david.wilson@learnhub.com", "Instructor@123456", UserRoles.Instructor, "David", "Wilson"),
                ("sarah.zhang@learnhub.com", "Instructor@123456", UserRoles.Instructor, "Sarah", "Zhang"),
                ("michael.brown@learnhub.com", "Instructor@123456", UserRoles.Instructor, "Michael", "Brown"),
                ("emily.patel@learnhub.com", "Instructor@123456", UserRoles.Instructor, "Emily", "Patel"),
                
                // Students
                ("student.one@learnhub.com", "Student@123456", UserRoles.Student, "John", "Smith"),
                ("student.two@learnhub.com", "Student@123456", UserRoles.Student, "Emma", "Johnson"),
                ("student.three@learnhub.com", "Student@123456", UserRoles.Student, "Lucas", "Garcia")
            };

            foreach (var (email, password, role, firstName, lastName) in users)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FirstName = firstName,
                        LastName = lastName
                    };

                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, role);
                        Console.WriteLine($"{role} user created successfully: {email}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create {role} user: {email}");
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"- {error.Description}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"{role} user already exists: {email}");
                }
            }
        }

        public static async Task SeedCoursesAsync(ApplicationDbContext context, UserManager<User> userManager)
        {
            try
            {
                var instructors = new Dictionary<string, string>();
                var instructorEmails = new[]
                {
                    "david.wilson@learnhub.com",
                    "sarah.zhang@learnhub.com",
                    "michael.brown@learnhub.com",
                    "emily.patel@learnhub.com"
                };

                foreach (var email in instructorEmails)
                {
                    var instructor = await userManager.FindByEmailAsync(email);
                    if (instructor != null)
                    {
                        instructors[email] = instructor.Id;
                    }
                }

                if (instructors.Any())
                {
                    var courses = new List<Course>
                    {
                        // Programming Fundamentals Track (David Wilson)
                        new Course
                        {
                            Title = "Programming Fundamentals with C#",
                            Description = "Master the basics of C# programming with hands-on projects and real-world applications. Perfect for beginners!",
                            InstructorId = instructors["david.wilson@learnhub.com"],
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Course
                        {
                            Title = "Object-Oriented Programming Mastery",
                            Description = "Deep dive into OOP concepts with practical examples in C# and Java. Learn inheritance, polymorphism, and encapsulation.",
                            InstructorId = instructors["david.wilson@learnhub.com"],
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },

                        // Web Development Track (Sarah Zhang)
                        new Course
                        {
                            Title = "Full-Stack Web Development Bootcamp",
                            Description = "Comprehensive course covering HTML5, CSS3, JavaScript, and modern web frameworks. Build responsive and dynamic websites.",
                            InstructorId = instructors["sarah.zhang@learnhub.com"],
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Course
                        {
                            Title = "Advanced React & Redux",
                            Description = "Master React.js, Redux, and modern state management. Create sophisticated single-page applications.",
                            InstructorId = instructors["sarah.zhang@learnhub.com"],
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },

                        // Data Science Track (Michael Brown)
                        new Course
                        {
                            Title = "Data Science Essentials",
                            Description = "Introduction to data analysis, visualization, and statistical methods using Python and R. Learn from real-world datasets.",
                            InstructorId = instructors["michael.brown@learnhub.com"],
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Course
                        {
                            Title = "Machine Learning Fundamentals",
                            Description = "Explore ML algorithms, neural networks, and practical applications using TensorFlow and scikit-learn.",
                            InstructorId = instructors["michael.brown@learnhub.com"],
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },

                        // Cloud & DevOps Track (Emily Patel)
                        new Course
                        {
                            Title = "Cloud Computing with AWS",
                            Description = "Learn AWS services, cloud architecture, and best practices for scalable cloud solutions.",
                            InstructorId = instructors["emily.patel@learnhub.com"],
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Course
                        {
                            Title = "DevOps Engineering Professional",
                            Description = "Master CI/CD pipelines, containerization with Docker, and Kubernetes orchestration.",
                            InstructorId = instructors["emily.patel@learnhub.com"],
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },

                        // Additional Specialized Courses
                        new Course
                        {
                            Title = "Mobile App Development with Flutter",
                            Description = "Create cross-platform mobile applications using Flutter and Dart. Build beautiful, native apps for iOS and Android.",
                            InstructorId = instructors["sarah.zhang@learnhub.com"],
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Course
                        {
                            Title = "Cybersecurity Fundamentals",
                            Description = "Learn essential cybersecurity concepts, threat detection, and security best practices for modern applications.",
                            InstructorId = instructors["emily.patel@learnhub.com"],
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Course
                        {
                            Title = "Artificial Intelligence Ethics",
                            Description = "Explore the ethical implications of AI, responsible AI development, and current challenges in the field.",
                            InstructorId = instructors["michael.brown@learnhub.com"],
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Course
                        {
                            Title = "Software Architecture Patterns",
                            Description = "Study modern software architecture patterns, microservices, and system design principles.",
                            InstructorId = instructors["david.wilson@learnhub.com"],
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    foreach (var course in courses)
                    {
                        var existingCourse = await context.Courses
                            .FirstOrDefaultAsync(c => c.Title == course.Title);
                        
                        if (existingCourse == null)
                        {
                            await context.Courses.AddAsync(course);
                            Console.WriteLine($"Added course: {course.Title}");
                        }
                        else
                        {
                            Console.WriteLine($"Course already exists: {course.Title}");
                        }
                    }

                    await context.SaveChangesAsync();
                    Console.WriteLine("Course seeding completed successfully.");
                }
                else
                {
                    Console.WriteLine("No instructors found. Please ensure instructors are created first.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding courses: {ex.Message}");
                throw;
            }}
             public static async Task SeedEnrollmentsAsync(ApplicationDbContext context, UserManager<User> userManager)
{
    try
    {
        // Check if enrollments already exist
        if (!context.Enrollments.Any())
        {
            // First verify the student exists
            var studentId = "2b562217-751a-4ce6-a14c-848aaa0b5293";
            var studentExists = await context.Users.AnyAsync(u => u.Id == studentId);
            if (!studentExists)
            {
                Console.WriteLine($"Student with ID {studentId} not found!");
                return;
            }

            // Get actual course IDs that exist in the database
            var availableCourses = await context.Courses
                .Select(c => new { c.Id, c.Title })
                .ToListAsync();

            Console.WriteLine($"Available courses: {availableCourses.Count}");
            foreach (var course in availableCourses)
            {
                Console.WriteLine($"Course ID: {course.Id}, Title: {course.Title}");
            }

            var enrollments = new List<Enrollment>();

            // Create enrollments only for existing courses
            foreach (var course in availableCourses)
            {
                // Check if enrollment already exists
                var exists = await context.Enrollments
                    .AnyAsync(e => e.StudentId == studentId && e.CourseId == course.Id);

                if (!exists)
                {
                    var enrollment = new Enrollment
                    {
                        StudentId = studentId,
                        CourseId = course.Id,
                        EnrollmentDate = DateTime.UtcNow,
                        Status = EnrollmentStatus.Enrolled
                    };

                    enrollments.Add(enrollment);
                    Console.WriteLine($"Preparing enrollment for course: {course.Title}");
                }
            }

            if (enrollments.Any())
            {
                await context.Enrollments.AddRangeAsync(enrollments);
                await context.SaveChangesAsync();
                Console.WriteLine($"Successfully added {enrollments.Count} enrollments");

                // Verify the enrollments
                var verifyEnrollments = await context.Enrollments
                    .Include(e => e.Course)
                    .Where(e => e.StudentId == studentId)
                    .ToListAsync();

                foreach (var enrollment in verifyEnrollments)
                {
                    Console.WriteLine($"Verified enrollment in: {enrollment.Course?.Title}");
                }
            }
            else
            {
                Console.WriteLine("No new enrollments to create");
            }
        }
        else
        {
            var existingCount = await context.Enrollments.CountAsync();
            Console.WriteLine($"Enrollments already exist. Count: {existingCount}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in SeedEnrollmentsAsync: {ex.Message}");
        Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
        throw;
    }
    
}}}