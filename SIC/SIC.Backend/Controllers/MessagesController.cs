using Microsoft.AspNetCore.Mvc;
using SIC.Backend.Helpers;
using SIC.Backend.UnitOfWork.Implemetations;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : GenericController<Message>
    {
        private readonly IMessageUnitOfWork _messageUnitOfWork;
        private readonly IInvitationUnitOfWork _invitationUnitOfWork;
        private readonly IGenericUnitOfWork<MessageKey> _unitOfWorkmk;

        public MessagesController(IGenericUnitOfWork<Message> unitOfWork, IGenericUnitOfWork<MessageKey> unitOfWorkmk, IMessageUnitOfWork messageUnitOfWork, IInvitationUnitOfWork invitationUnitOfWork) : base(unitOfWork)
        {
            _messageUnitOfWork = messageUnitOfWork;
            _invitationUnitOfWork = invitationUnitOfWork;
            _unitOfWorkmk = unitOfWorkmk;
        }

        //Generar un Endpoind para que obtenga el mensaje con la sustitucion de los tokens del mensaje y conservando la estructura del mismo
        [HttpGet("byCode/{code}/{codeInvitation}")]
        public async Task<IActionResult> GetInvitationMessagesAsync(string code, string codeInvitation)
        {
            var message = await _messageUnitOfWork.GetByCodeAsync(code);
            var keys = await _unitOfWorkmk.GetAsync();
            var invitation = await _invitationUnitOfWork.GetByCodeAsync(codeInvitation);
            //Logica para sustituir los tokens del mensaje
            if (message.Result != null && invitation.Result != null && keys.Result != null)
            {
                return Ok(MessageFormatter.FormatMessage(message.Result, invitation.Result, keys.Result.ToList()));
            }
            return NotFound();
        }

        [HttpGet("byCode/{code}")]
        public async Task<IActionResult> GetByCodeAsync(string code)
        {
            var response = await _messageUnitOfWork.GetByCodeAsync(code);
            if (response.Success)
            {
                return Ok(response.Result);
            }
            return NotFound();
        }

        [HttpGet("Keys")]
        public async Task<IActionResult> GetKeysAsync()
        {
            var response = await _messageUnitOfWork.GetKeysAsync();
            if (response.Success)
            {
                return Ok(response.Result);
            }
            return NotFound();
        }

        [HttpPost("full")]
        public async Task<IActionResult> PostFullAsync(Message message, string code)
        {
            var action = await _messageUnitOfWork.AddFullAsync(message, code);
            if (action.Success)
            {
                return Ok(action.Result);
            }
            return NotFound(action.Message);
        }

        [HttpPut("full")]
        public async Task<IActionResult> PutFullAsync(Message message, string code)
        {
            var action = await _messageUnitOfWork.UpdateFullAsync(message, code);
            if (action.Success)
            {
                return Ok(action.Result);
            }
            return NotFound(action.Message);
        }
    }
}