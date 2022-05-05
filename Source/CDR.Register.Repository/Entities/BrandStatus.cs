using System;
using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Repository.Entities
{
	public class BrandStatus
	{
		[Key]
		public BrandStatusType BrandStatusId { get; set; }
		[MaxLength(25), Required]
		public string BrandStatusCode { get; set; }
	}

	public enum BrandStatusType
	{
		Unknown = 0,
		Active = 1,
		Inactive = 2,
		Removed = 3
	}
}