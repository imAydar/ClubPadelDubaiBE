﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubPadel.Models
{
    public class Location : EntityBase
    {
        public string Map { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Guid EventId { get; set; }
        public Event Event { get; set; }
    }
}
