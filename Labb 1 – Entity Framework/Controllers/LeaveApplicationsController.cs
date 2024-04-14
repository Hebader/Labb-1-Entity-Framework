using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Labb_1___Entity_Framework.Data;
using Labb_1___Entity_Framework.Models;
using System.Globalization;

namespace Labb_1___Entity_Framework.Controllers
{
    public class LeaveApplicationsController : Controller
    {
        private readonly LeaveManagementDbContext _context;

        public LeaveApplicationsController(LeaveManagementDbContext context)
        {
            _context = context;
        }

        // GET: LeaveApplications
        // Inkludera anställdadata när du hämtar ledighetsansökningarna från databasen i Index()-metoden

        public async Task<IActionResult> Index()
        {
           

            // Hämta ledighetsansökningar från databasen inklusive relaterade data
            var leaveApplications = await _context.LeaveApplications
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .ToListAsync();

            return View(leaveApplications);
        }


        [HttpGet]
        public async Task<IActionResult> MonthlyLeaveApplications(int? month)
        {
            // Vald månad, om ingen månad anges, använd den aktuella månaden
            var selectedMonth = month.HasValue && month > 0 && month <= 12 ? month.Value : DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;

            // Hämta alla ansökningar som skapats under den valda månaden och året
            var monthlyApplications = await _context.LeaveApplications
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Where(l => l.CreatedAt.Month == selectedMonth && l.CreatedAt.Year == currentYear)
                .ToListAsync();

            // Skapa en dictionary för att lagra antalet dagar ledighet per person
            var leaveDaysPerEmployee = new Dictionary<string, int>();

            // Loopa igenom varje ansökan och räkna antalet lediga dagar per person
            foreach (var application in monthlyApplications)
            {
                var employeeName = application.Employee.EmployeeName;
                var leaveDays = (application.EndDate - application.StartDate).Days + 1; // Antal lediga dagar

                // Lägg till eller uppdatera antalet lediga dagar för varje person
                if (leaveDaysPerEmployee.ContainsKey(employeeName))
                {
                    leaveDaysPerEmployee[employeeName] += leaveDays;
                }
                else
                {
                    leaveDaysPerEmployee.Add(employeeName, leaveDays);
                }
            }

            // Skicka data till vyn för att visa resultatet
            ViewData["Month"] = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(selectedMonth); // Konvertera månadsnummer till namn
            ViewData["Year"] = currentYear;
            ViewData["LeaveDaysPerEmployee"] = leaveDaysPerEmployee;

            return View(monthlyApplications);
        }






        // GET-metod för att visa ledighetsansökningar för en specifik anställd
        public async Task<IActionResult> EmployeeLeaveApplications(string employeeName)
        {
            // Hämta alla anställda från databasen
            var employees = await _context.Employees.ToListAsync();

            // Skapa en lista över anställdas namn
            var employeeNames = employees.Select(e => new { EmployeeId = e.EmployeeId, EmployeeName = e.EmployeeName }).ToList();

            // Tilldela listan med anställdas namn till ViewBag.EmployeeNames
            ViewBag.EmployeeNames = employeeNames;

            // Om en specifik anställd har valts
            if (!string.IsNullOrEmpty(employeeName))
            {
                // Hämta den valda anställdan från databasen
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeName == employeeName);

                // Om den valda anställdan inte hittas, visa ett meddelande eller hantera fallet på annat sätt
                if (employee == null)
                {
                    // Exempel: visa ett felmeddelande
                    ModelState.AddModelError(string.Empty, "Selected employee not found.");
                    return View(new List<LeaveApplication>());
                }

                // Hämta ledighetsansökningarna för den valda anställda från databasen
                var employeeApplications = await _context.LeaveApplications
                    .Include(l => l.Employee)
                    .Include(l => l.LeaveType)
                    .Where(l => l.FkEmployeeId == employee.EmployeeId)
                    .ToListAsync();

                // Skicka ledighetsansökningarna till vyn
                return View(employeeApplications);
            }

            // Om ingen anställd har valts, returnera en tom lista till vyn
            return View(new List<LeaveApplication>());
        }

        // POST-metod för att visa ledighetsansökningar för en specifik anställd
        [HttpPost]
        public async Task<IActionResult> EmployeeLeaveApplications(int employeeid)
        {
            // Hämta alla anställda från databasen
            var employees = await _context.Employees.ToListAsync();

            // Skapa en lista över anställdas namn
            var employeeNames = employees.Select(e => new { EmployeeId = e.EmployeeId, EmployeeName = e.EmployeeName }).ToList();

            // Tilldela listan med anställdas namn till ViewBag.EmployeeNames
            ViewBag.EmployeeNames = employeeNames;

            // Hämta ledighetsansökningarna för den valda anställda från databasen
            var leaveApplications = await _context.LeaveApplications
                .Where(l => l.FkEmployeeId == employeeid)
                .ToListAsync();

            return View(leaveApplications);
        }



        // GET: LeaveApplications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveApplication = await _context.LeaveApplications
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(m => m.LeaveApplicationId == id);
            if (leaveApplication == null)
            {
                return NotFound();
            }

            return View(leaveApplication);
        }



        public IActionResult Create()
        {
            // Hämta befintliga ledighetstyper från databasen
            var leaveTypes = _context.LeaveTypes.ToList();

            // Om det inte finns några ledighetstyper i databasen, lägg till dem
            if (leaveTypes.Count == 0)
            {
                // Skapa och lägg till ledighetstyper i databasen
                _context.LeaveTypes.AddRange(new List<LeaveType>
        {
            new LeaveType { LeaveTypeName = "Sick Leave" },
            new LeaveType { LeaveTypeName = "Vacation" },
        
            // Lägg till fler ledighetstyper här vid behov
        });

                // Spara ändringarna i databasen
                _context.SaveChanges();

                // Hämta ledighetstyper från databasen igen
                leaveTypes = _context.LeaveTypes.ToList();
            }

            // Konvertera LeaveType-objekten till SelectListItem-objekt
            var leaveTypeItems = leaveTypes.Select(lt => new SelectListItem
            {
                Value = lt.LeaveTypeId.ToString(),
                Text = lt.LeaveTypeName
            }).ToList();

            // Lägg till listan med SelectListItem till ViewBag.LeaveTypes
            ViewBag.LeaveTypes = leaveTypeItems;
            ViewData["FkEmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeName");
            ViewData["FkLeaveTypeId"] = new SelectList(leaveTypes, "LeaveTypeId", "LeaveTypeName");
            return View();
        }

        // POST: LeaveApplications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: LeaveApplications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LeaveApplicationId,FkEmployeeId,FkLeaveTypeId,StartDate,EndDate,CreatedAt")] LeaveApplication leaveApplication)
        {
            if (ModelState.IsValid)
            {
                // Sätt CreatedAt till dagens datum och tid
                leaveApplication.CreatedAt = DateTime.Now;

                _context.Add(leaveApplication);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FkEmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeName", leaveApplication.FkEmployeeId);
            ViewData["FkLeaveTypeId"] = new SelectList(_context.LeaveTypes, "LeaveTypeId", "LeaveTypeId", leaveApplication.FkLeaveTypeId);
            return View(leaveApplication);
        }
        public async Task<IActionResult> SeedData()
        {
            try
            {
                // Populate Employees
                var employees = new List<Employee>
        {
            new Employee { EmployeeName = "Anna N" },
            new Employee { EmployeeName = "Lars P" }
            // Add more employees here if needed
        };
                _context.Employees.AddRange(employees);

   

                // Save changes to persist employees and leave types
                await _context.SaveChangesAsync();

                // Populate LeaveApplications
                var leaveApplications = new List<LeaveApplication>
        {
            new LeaveApplication { FkEmployeeId = 1, FkLeaveTypeId = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(3), CreatedAt = DateTime.Now },
            new LeaveApplication { FkEmployeeId = 2, FkLeaveTypeId = 2, StartDate = DateTime.UtcNow.AddDays(5), EndDate = DateTime.UtcNow.AddDays(10), CreatedAt = DateTime.Now }
            // Add more leave applications here if needed
        };
                _context.LeaveApplications.AddRange(leaveApplications);

                // Save changes to persist leave applications
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately (logging, error page, etc.)
                ModelState.AddModelError("", "An error occurred while seeding data.");
                return View(); // or return error view
            }
        }


        // GET: LeaveApplications1/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveApplication = await _context.LeaveApplications.FindAsync(id);
            if (leaveApplication == null)
            {
                return NotFound();
            }
            ViewData["FkEmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeName", leaveApplication.FkEmployeeId);
            ViewData["FkLeaveTypeId"] = new SelectList(_context.LeaveTypes, "LeaveTypeId", "LeaveTypeId", leaveApplication.FkLeaveTypeId);
            return View(leaveApplication);
        }

        // POST: LeaveApplications1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LeaveApplicationId,FkEmployeeId,FkLeaveTypeId,StartDate,EndDate,CreatedAt")] LeaveApplication leaveApplication)
        {
            if (id != leaveApplication.LeaveApplicationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(leaveApplication);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeaveApplicationExists(leaveApplication.LeaveApplicationId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FkEmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeName", leaveApplication.FkEmployeeId);
            ViewData["FkLeaveTypeId"] = new SelectList(_context.LeaveTypes, "LeaveTypeId", "LeaveTypeId", leaveApplication.FkLeaveTypeId);
            return View(leaveApplication);
        }

        // GET: LeaveApplications1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveApplication = await _context.LeaveApplications
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(m => m.LeaveApplicationId == id);
            if (leaveApplication == null)
            {
                return NotFound();
            }

            return View(leaveApplication);
        }

        // POST: LeaveApplications1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var leaveApplication = await _context.LeaveApplications.FindAsync(id);
            if (leaveApplication != null)
            {
                _context.LeaveApplications.Remove(leaveApplication);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LeaveApplicationExists(int id)
        {
            return _context.LeaveApplications.Any(e => e.LeaveApplicationId == id);
        }

        

    }
}

