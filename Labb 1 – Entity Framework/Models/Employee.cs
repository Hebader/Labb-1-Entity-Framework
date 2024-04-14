using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Labb_1___Entity_Framework.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        [DisplayName("Employee Name")]
        public string EmployeeName { get; set; }

        public string? Department { get; set; }
      
        public virtual ICollection<LeaveApplication>? Appointments { get; set; } 
      
    }
}