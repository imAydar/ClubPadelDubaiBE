using ClubPadel.Models;
using ClubPadel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClubPadel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController: ControllerBase
    {
        private readonly EventService _eventService;

        public EventsController(EventService eventService)
        {
            _eventService = eventService;
            //new TelegramBotService().SendEventMessage().GetAwaiter().GetResult();
        }

        //[Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Event param)
        {
            return Ok(await _eventService.Create(param));
        }

       //[Authorize(Roles = "admin")]
        //[HttpPut]
        //public IActionResult Update(Event param)
        //{
        //    return Ok(_eventService.Update(param));
        //}

        //[Authorize(Roles = "admin")]
        //[HttpDelete]
        //public IActionResult Delete(Event param)
        //{
        //    return Ok(_eventService.Delete(param));
        //}

        [HttpGet]
        public IActionResult GetAll()
        {
            Console.WriteLine("Trying to log get all events");
            return Ok(_eventService.GetAllEvents());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var eventItem = _eventService.GetEventById(id);
            if (eventItem == null) return NotFound();

            return Ok(eventItem);
        }

        [HttpPost("{id}/participants")]
        public async Task<IActionResult> AddParticipant(Guid id, [FromBody] Participant participant)
        {
            await _eventService.AddParticipant(id, participant);
            return Ok();
        }

        [HttpPost("{id}/participants/confirm")]
        public async Task<IActionResult> ConfirmParticipant(Guid id, [FromBody] Participant participant)
        {
            await _eventService.ConfirmParticipant(id, participant.Id, participant.Confirmed, id);
            return Ok();
        }

        [HttpDelete("{id}/participants")]
        public async Task<IActionResult> RemoveParticipant(Guid id, [FromBody] Guid participanId)
        {
            await _eventService.RemoveParticipant(id, participanId);
            return Ok();
        }
    }
}
