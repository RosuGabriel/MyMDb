﻿using MyMDb.Models.Base;

namespace MyMDb.Models
{
    public class Review : BaseEntity
    {
        public double? Rating { get; set; }
        public string? Comment { get; set; }

        public string? UserId { get; set; }
        public virtual AppUser? User { get; set; }

        public Guid? MediaId { get; set; }
        public virtual Media? Media { get;}
    }
}