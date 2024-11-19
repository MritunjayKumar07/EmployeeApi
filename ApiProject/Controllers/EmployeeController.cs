using ApiProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ApiProject.Models.EmployeeDetails;

namespace ApiProject.Controllers
{
	[Route("api/[controller]")]
	[ApiController]

	public class EmployeeController : ControllerBase
	{
		#region Injected Dependency Injection
		//Field that represents the database context class
		private readonly ProjectsContext _projectsContext;

		//Injected into the controller using Dependency Injection (DI) in the constructor
		public EmployeeController(ProjectsContext projectsContext)
		{
			_projectsContext = projectsContext;
		}
		#endregion

		#region Get All Employees
		[HttpGet]
		public async Task<ActionResult<IEnumerable<EmployeeDetails>>> GET()
		{
			try
			{
				// Fetch all employees with their addresses
				var employees = await _projectsContext.Employees
					.Include(e => e.Addresses)
					.Select(e => new EmployeeDetails
					{
						Eid = e.Eid,
						Name = e.Name,
						Position = e.Position,
						Salary = e.Salary ?? 0,
						Addresses = e.Addresses.Select(a => new EmployeeDetails.AddressDetails
						{
							Aid = a.Aid,
							Street = a.Street,
							City = a.City,
							State = a.State,
							PinCode = a.PinCode
						}).ToList()
					}).ToListAsync();

				return Ok(employees);
			}
			catch(Exception ex)
			{
				return StatusCode(500, new { message = "An error occurred while fetching the employees.", details = ex.Message });
			}
			
		}
		#endregion

		#region Search by Eid & Name
		[HttpGet("search")]
		public async Task<ActionResult<IEnumerable<EmployeeDetails>>> GET(string? name, int? id)
		{
			try
			{
				var query = _projectsContext.Employees.Include(e => e.Addresses).AsQueryable();

				// Apply filters (search by name or ID)
				if (id.HasValue)
				{
					query = query.Where(e => e.Eid == id.Value);
				}
				if (!string.IsNullOrEmpty(name))
				{
					query = query.Where(e => e.Name.Contains(name));
				}

				var searchedEmployees = await query.Select(e => new EmployeeDetails
				{
					Eid = e.Eid,
					Name = e.Name,
					Position = e.Position,
					Salary = e.Salary ?? 0,
					Addresses = e.Addresses.Select(a => new EmployeeDetails.AddressDetails
					{
						Aid = a.Aid,
						Street = a.Street,
						City = a.City,
						State = a.State,
						PinCode = a.PinCode
					}).ToList()
				}).ToListAsync();

				return Ok(searchedEmployees);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "An error occurred while searching the employees.", details = ex.Message });
			}
		}
		#endregion

		#region Add new Employee
		[HttpPost]
		public async Task<IActionResult> Post(EmployeeDetails employee)
		{
			try
			{
				// Check if the employee already exists
				var existingEmployee = await _projectsContext.Employees
					.FirstOrDefaultAsync(e => e.Name == employee.Name && e.Position == employee.Position);

				if (existingEmployee != null)
				{
					return BadRequest(new { message = "Employee already exists." });
				}

				// Create a new employee record
				var newEmployee = new Employee
				{
					Name = employee.Name,
					Position = employee.Position,
					Salary = employee.Salary
				};

				// Add the employee
				_projectsContext.Employees.Add(newEmployee);

				// Add the addresses associated with the new employee
				if (employee.Addresses != null)
				{
					foreach (var item in employee.Addresses)
					{
						var newAddress = new Address
						{
							Street = item.Street,
							City = item.City,
							State = item.State,
							PinCode = item.PinCode
						};

						// Add address in new employee
						newEmployee.Addresses.Add(newAddress);
					}
				}

				// Save the new employee with addresses in a database
				await _projectsContext.SaveChangesAsync();

				// Return the newly created employee data
				return CreatedAtAction(nameof(GET), new { id = newEmployee.Eid }, new EmployeeDetails
				{
					Eid = newEmployee.Eid,
					Name = newEmployee.Name,
					Position = newEmployee.Position,
					Salary = newEmployee.Salary ?? 0,
					Addresses = newEmployee.Addresses.Select(a => new EmployeeDetails.AddressDetails
					{
						Aid = a.Aid,
						Street = a.Street,
						City = a.City,
						State = a.State,
						PinCode = a.PinCode
					}).ToList()
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "An error occurred while creating the employee.", details = ex.Message });
			}
		}
		#endregion

		#region Update Employee
		[HttpPut("update/{id}")]
		public async Task<IActionResult> Put(int id, EmployeeDetails employeeDetails)
		{
			try
			{
				//Check exists or not
				var existingEmp = await _projectsContext.Employees.Include(e => e.Addresses).FirstOrDefaultAsync(e => e.Eid == id);
				if (existingEmp == null)
				{
					return NotFound(new { message = "Requested employee not fount" });
				}

				//Update employee:
				existingEmp.Name = employeeDetails.Name;
				existingEmp.Position = employeeDetails.Position;
				existingEmp.Salary = employeeDetails.Salary;

				//Update Employee address
				if (employeeDetails.Addresses != null)
				{
					foreach (var item in employeeDetails.Addresses)
					{
						//Check address exist or not
						var existingAddress = existingEmp.Addresses.FirstOrDefault(a => a.Aid == item.Aid);
						//Update address
						if (existingAddress != null)
						{
							existingAddress.Street = item.Street;
							existingAddress.City = item.City;
							existingAddress.State = item.State;
							existingAddress.PinCode = item.PinCode;
						}
						else
						{
							//Add new address if not found
							existingEmp.Addresses.Add(new Address
							{
								Street = item.Street,
								City = item.City,
								State = item.State,
								PinCode = item.PinCode
							});
						}
					}
				}

				//Save the changes data
				await _projectsContext.SaveChangesAsync();

				//Return the updated employee data
				return Ok(new EmployeeDetails
				{
					Eid = employeeDetails.Eid,
					Name = employeeDetails.Name,
					Position = employeeDetails.Position,
					Salary = employeeDetails.Salary ?? 0,
					Addresses = existingEmp.Addresses.Select(a => new EmployeeDetails.AddressDetails
					{
						Aid = a.Aid,
						Street = a.Street,
						City = a.City,
						State = a.State,
						PinCode = a.PinCode
					}).ToList()
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "An error occurred while updating the employee.", details = ex.Message });
			}
		}
		#endregion

		#region Delete Employee
		[HttpDelete("employee/{id}")]
		public async Task<IActionResult> DeleteEmployee(int id)
		{
			try
			{
				var employee = await _projectsContext.Employees
				.Include(e => e.Addresses)
				.FirstOrDefaultAsync(e => e.Eid == id);

				if (employee == null)
				{
					return NotFound(new { message = "Employee not found." });
				}

				// Delete the employee with addresses
				_projectsContext.Employees.Remove(employee);

				// Save the changes to the database
				await _projectsContext.SaveChangesAsync();

				return Ok(new { message = "Employee and all related addresses deleted successfully." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "An error occurred while deleting the Employee.", details = ex.Message });
			}

		}
		#endregion

		#region Add New Address
		[HttpPost("address/{id}")]
		public async Task<IActionResult> Post(int id, EmployeeDetails.AddressDetails addressDetails)
		{
			try
			{
				// Check if the employee exists
				var existingEmployee = await _projectsContext.Employees
					.Include(e => e.Addresses)
					.FirstOrDefaultAsync(e => e.Eid == id);

				if (existingEmployee == null)
				{
					return NotFound(new { message = "Employee not found." });
				}

				// Create a new address record
				var newAddress = new Address
				{
					Street = addressDetails.Street,
					City = addressDetails.City,
					State = addressDetails.State,
					PinCode = addressDetails.PinCode
				};

				// Add the new address in a regarded employee
				existingEmployee.Addresses.Add(newAddress);

				// Save the database
				await _projectsContext.SaveChangesAsync();

				// Return the updated employee data with a new address
				return Ok(new EmployeeDetails
				{
					Eid = existingEmployee.Eid,
					Name = existingEmployee.Name,
					Position = existingEmployee.Position,
					Salary = existingEmployee.Salary ?? 0,
					Addresses = existingEmployee.Addresses.Select(a => new EmployeeDetails.AddressDetails
					{
						Aid = a.Aid,
						Street = a.Street,
						City = a.City,
						State = a.State,
						PinCode = a.PinCode
					}).ToList()
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "An error occurred while creating the address.", details = ex.Message });
			}

		}
		#endregion

		#region Delete Address
		[HttpDelete("address/{employeeId}/{addressId}")]
		public async Task<IActionResult> Delete(int employeeId, int addressId)
		{
			try
			{
				// Find the employee by ID
				var existingEmployee = await _projectsContext.Employees
					.Include(e => e.Addresses)
					.FirstOrDefaultAsync(e => e.Eid == employeeId);

				if (existingEmployee == null)
				{
					return NotFound(new { message = "Employee not found." });
				}

				// Find the address by ID
				var addressToDelete = existingEmployee.Addresses
					.FirstOrDefault(a => a.Aid == addressId);

				if (addressToDelete == null)
				{
					return NotFound(new { message = "Address not found." });
				}

				// Remove the address
				existingEmployee.Addresses.Remove(addressToDelete);

				// Save the the database
				await _projectsContext.SaveChangesAsync();

				// Return the updated employee data after a address deletion
				return Ok(new EmployeeDetails
				{
					Eid = existingEmployee.Eid,
					Name = existingEmployee.Name,
					Position = existingEmployee.Position,
					Salary = existingEmployee.Salary ?? 0,
					Addresses = existingEmployee.Addresses.Select(a => new EmployeeDetails.AddressDetails
					{
						Aid = a.Aid,
						Street = a.Street,
						City = a.City,
						State = a.State,
						PinCode = a.PinCode
					}).ToList()
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "An error occurred while deleting the address.", details = ex.Message });
			}
		}
		#endregion

	}
}
