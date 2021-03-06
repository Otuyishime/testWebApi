﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using testWebAPI.DBs;
using testWebAPI.Infrastructure;
using testWebAPI.Models;
using testWebAPI.Models.Entities;
using testWebAPI.Models.Forms;
using testWebAPI.Models.Resources;
using testWebAPI.Models.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace testWebAPI.Controllers
{
    [Route("api/[controller]")]
    public class RoomsController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IOpeningService _openingService;
        private readonly IDateLogicService _dateLogicService;
        private readonly IBookingService _bookingService;
        private readonly PagingOptions _defaultPagingOptions;

        public RoomsController(
            IRoomService roomService,
            IOpeningService openingService,
            IDateLogicService dateLogicService,
            IBookingService bookingService,
            IOptions<PagingOptions> defaultPagingOptions)
        {
            _roomService = roomService;
            _openingService = openingService;
            _dateLogicService = dateLogicService;
            _bookingService = bookingService;
            _defaultPagingOptions = defaultPagingOptions.Value;
        }

        // GET: api/rooms
        [HttpGet(Name = nameof(GetRoomsAsync))]
        public async Task<IActionResult> GetRoomsAsync(
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Room, RoomEntity> sortOptions,
            [FromQuery] SearchOptions<Room, RoomEntity> searchOptions,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var rooms = await _roomService.GetRoomsAsync(pagingOptions, sortOptions, searchOptions, cancellationToken);

            var collection = PagedCollection<Room>.Create<RoomsResponse>(
                Link.ToCollection(nameof(GetRoomsAsync)),
                rooms.Items.ToArray(),
                rooms.TotalSize,
                pagingOptions);
            collection.Openings = Link.ToCollection(nameof(GetAllRoomOpeningsAsync));
            collection.RoomsQuery = FormMetadata.FromResource<Room>(
                Link.ToForm(
                    nameof(GetRoomsAsync), null, 
                    Link.GetMethod, Form.QueryRelation
                )
            );

            return Ok(collection);
        }

        // GET /rooms/openings
        [HttpGet("openings", Name = nameof(GetAllRoomOpeningsAsync))]
        // client-cached and cached on server for 30 seconds
        // Add hints as to which queries cause the data to change
        [ResponseCache(Duration = 30, VaryByQueryKeys = new[] { "offset", "limit", "orderBy", "search" })]
        public async Task<IActionResult> GetAllRoomOpeningsAsync(
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Opening, OpeningEntity> sortOptions,
            [FromQuery] SearchOptions<Opening, OpeningEntity> searchOptions,
            CancellationToken cancellationToken
        )
        {
            if(!ModelState.IsValid){
                return BadRequest(new ApiError(ModelState));
            }

            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var openings = await _openingService.GetOpeningsAsync(
                pagingOptions, 
                sortOptions, 
                searchOptions,
                cancellationToken
            );

            var collection = PagedCollection<Opening>.Create<OpeningsResponse>(
                Link.ToCollection(nameof(GetAllRoomOpeningsAsync)),
                openings.Items.ToArray(),
                openings.TotalSize,
                pagingOptions);

            collection.OpeningsQuery = FormMetadata.FromResource<Opening>(
                Link.ToForm(
                    nameof(GetAllRoomOpeningsAsync),
                    null,
                    Link.GetMethod,
                    Form.QueryRelation));

            return Ok(collection);
        }

        // GET api/rooms/{roomId}
        [HttpGet("{roomId}", Name = nameof(GetRoomByIdAsync))]
        public async Task<IActionResult> GetRoomByIdAsync(Guid roomId, CancellationToken cancellationToken)
        {
            var room = await _roomService.GetRoomAsync(roomId, cancellationToken);
            return room == null ? NotFound() : (IActionResult)Ok(room);
        }

        // TODO authentication!
        // POST /rooms/{roomId}/bookings
        [HttpPost("{roomId}/bookings", Name = nameof(CreateBookingForRoomAsync))]
        public async Task<IActionResult> CreateBookingForRoomAsync(
            Guid roomId,
            [FromBody] BookingForm bookingForm,
            CancellationToken cancellationToken
        )
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            var room = await _roomService.GetRoomAsync(roomId, cancellationToken);
            if (room == null) return NotFound();

            var minimumStay = _dateLogicService.GetMinimumStay();
            bool tooShort = (bookingForm.EndAt.Value - bookingForm.StartAt.Value) < minimumStay;

            if (tooShort) return BadRequest(
                new ApiError($"The minimum booking duration is {minimumStay.TotalHours}.")
            );

            var conflictedSlots = await _openingService.GetConflictingSlots(
                roomId, bookingForm.StartAt.Value, bookingForm.EndAt.Value, cancellationToken
            );

            if (conflictedSlots.Any()) return BadRequest(
                new ApiError("This time conflicts with an existing booking.")
            );

            // Get the user ID (TODO)
            var userId = Guid.NewGuid();

            var bookingId = await _bookingService.CreateBookingAsync(
                userId, roomId, bookingForm.StartAt.Value, bookingForm.EndAt.Value, cancellationToken
            );

            return Created(
                Url.Link(nameof(BookingsController.GetBookingByIdAsync),
                new { bookingId }),
                null
            );
        }
    }
}
