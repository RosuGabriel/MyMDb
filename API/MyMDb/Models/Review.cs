﻿using MyMDb.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace MyMDb.Models
{
    public class Review : BaseEntity
    {
        [Required]
        [Range(1, 10, ErrorMessage = "Rating must be between 1 and 10.")]
        public double? Rating { get; set; }
        public string? Comment { get; set; }
        [Required]
        public string? UserId { get; set; }
        public virtual UserProfile? UserProfile { get; set; }
        [Required]
        public Guid? MediaId { get; set; }
        public virtual Media? Media { get;}
    }
}
