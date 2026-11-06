using ECommerce.FakePaymentProvider.DTOs;
using ECommerce.FakePaymentProvider.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.FakePaymentProvider.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(
    IPaymentSessionService paymentSessionService) : ControllerBase
{
    [HttpPost]
    public ActionResult<CreatePaymentSessionResultDto> CreatePayment(
        [FromBody] CreatePaymentSessionDto dto)
    {
        var result = paymentSessionService.CreateSession(dto);

        if (result is null)
        {
            return Conflict(new
            {
                Error =
                    "PaymentId already exists with another amount or currency."
            });
        }

        return Created(result.PaymentUrl, result);
    }
}