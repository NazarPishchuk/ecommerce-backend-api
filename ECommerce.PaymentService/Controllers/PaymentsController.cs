using ECommerce.PaymentService.DTOs;
using ECommerce.PaymentService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.PaymentService.Controllers;

[Route("api/payments")]
[ApiController]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> CreatePayment(
        [FromBody] CreatePaymentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await paymentService.CreatePaymentAsync(
            dto.OrderId,
            dto.Amount,
            dto.Currency,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetPayment),
            new { id = result.PaymentId },
            result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetPaymentDto>> GetPayment(
        Guid id,
        CancellationToken cancellationToken)
    {
        var payment = await paymentService.GetPaymentAsync(
            id,
            cancellationToken);

        if (payment is null)
            return NotFound();

        return Ok(payment);
    }
}
