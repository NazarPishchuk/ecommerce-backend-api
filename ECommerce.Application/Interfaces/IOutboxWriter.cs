namespace ECommerce.Application.Interfaces;

public interface IOutboxWriter
{
    void Add<T>(T message);
}
