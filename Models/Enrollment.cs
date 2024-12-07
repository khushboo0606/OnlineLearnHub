using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineLearnHub.Models
{
    public class Enrollment
    {
        public string ?StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public EnrollmentStatus Status { get; set; }

        // Navigation properties
        public virtual User ?Student { get; set; }
        public virtual Course ?Course { get; set; }
    }

    public enum EnrollmentStatus
    {
        Enrolled = 0,
        Completed = 1,
        Dropped = 2
    }
}