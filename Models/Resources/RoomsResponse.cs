﻿using System;
namespace testWebAPI.Models.Resources
{
    public class RoomsResponse : PagedCollection<Room>
    {
        public Link Openings { get; set; }
    }
}
