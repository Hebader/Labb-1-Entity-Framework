using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Labb_1___Entity_Framework.Models
{
    public class LeaveApplication
    {
        public int LeaveApplicationId { get; set; }

        [ForeignKey("Employee")]
        [Display(Name = "Employee")]
        public int FkEmployeeId { get; set; }
        public Employee? Employee { get; set; }

        [Display(Name = "Leave Type")]
        public int FkLeaveTypeId { get; set; }
        [ForeignKey("FkLeaveTypeId")]
        public LeaveType? LeaveType { get; set; }

        [Required]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        
    }
}
