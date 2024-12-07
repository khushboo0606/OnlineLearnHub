using System;

namespace OnlineLearnHub.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string ?Title { get; set; }
        public string ?Description { get; set; }
        public string ? InstructorId { get; set; }
        public User ?Instructor { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}