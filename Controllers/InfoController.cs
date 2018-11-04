﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using testWebAPI.Models;
using testWebAPI.Models.Resources;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace testWebAPI.Controllers
{
    [Route("api/[controller]")]
    public class InfoController : Controller
    {
        private readonly HotelInfo _hotelInfo;

        public InfoController(IOptions<HotelInfo> hotelInfoAccessor)
        {
            _hotelInfo = hotelInfoAccessor.Value;
        }

        // GET: api/values
        [HttpGet(Name = nameof(GetInfo))]
        [ResponseCache(CacheProfileName = "Static")]
        public IActionResult GetInfo()
        {
            _hotelInfo.Self = Link.To(nameof(GetInfo));
            return Ok(_hotelInfo);
        }
    }
}
