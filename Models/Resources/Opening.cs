﻿using System;
using testWebAPI.Infrastructure.Attributes;
using testWebAPI.Models.Entities;
using testWebAPI.Models.Forms;

namespace testWebAPI.Models.Resources
{
    public class Opening
    {
        [Sortable(EntityProperty = nameof(OpeningEntity.RoomId))]
        public Link Room { get; set; }

        [Sortable(Default = true)]
        [SearchableDateTime]
        public DateTimeOffset StartAt { get; set; }

        [Sortable]
        [SearchableDateTime]
        public DateTimeOffset EndAt { get; set; }

        [Sortable]
        [SearchableDecimal]
        public decimal Rate { get; set; }

        public Form Book { get; set; }
    }
}
