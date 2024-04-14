using System.ComponentModel.DataAnnotations;

namespace Labb_1___Entity_Framework.Models
{
    public class LeaveType
    {

        public int LeaveTypeId { get; set; }

        [Required]
        [Display(Name = "Leave Type")]
        public string LeaveTypeName { get; set; }
        public virtual ICollection<LeaveApplication>? Appointments { get; set; }
    }
}
