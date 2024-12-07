using System;
using OnlineLearnHub.Models; // Ensure this is the correct namespace for EnrollmentStatus

namespace OnlineLearnHub.DTOs
{
    public class EnrollmentDto
    {
        public int Id { get; set; }
        public string ?StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public EnrollmentStatus Status { get; set; }
    }
}
