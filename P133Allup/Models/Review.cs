﻿using System.ComponentModel.DataAnnotations;

namespace P133Allup.Models
{
	public class Review : BaseEntity
	{
		public int? ProductId { get; set; }

		public Product? Product { get; set; }

		public string? UserId { get; set; }

		public AppUser? AppUser { get; set; }

		[EmailAddress]
		public string? Email { get; set; }

		[StringLength(100)]
		public string? Name { get; set; }


		[StringLength(1000)]
		public string? Description { get; set; }

		[Range(0,5)]
		public int Start { get; set; }

	}
}
