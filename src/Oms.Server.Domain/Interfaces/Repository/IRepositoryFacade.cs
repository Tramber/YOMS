namespace Oms.Server.Domain.Interfaces.Repository
{
    public interface IRepositoryFacade
    {
        IUserRepository Users { get; }
        IOrderRepository Orders { get; }
        IInstrumentRepository Instruments { get; }
    }
}
