namespace ApiProject.Models
{
	public class EmployeeDetails
	{
		// Employee Details
		public int Eid { get; set; }
		public string Name { get; set; }
		public string Position { get; set; }
		public decimal? Salary { get; set; }

		// Address Details
		public List<AddressDetails> Addresses { get; set; } = new List<AddressDetails>();
		public class AddressDetails
		{
			public int Aid { get; set; }
			public string Street { get; set; }
			public string City { get; set; }
			public string State { get; set; }
			public string PinCode { get; set; }
		}
	}
}
