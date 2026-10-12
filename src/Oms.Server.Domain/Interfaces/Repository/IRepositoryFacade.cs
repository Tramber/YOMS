namespace Oms.Server.Domain.Interfaces.Repository
{
    /// <summary>Entry point to every repository.</summary>
    public interface IRepositoryFacade
    {
        IUserRepository Users { get; }
        IOrderRepository Orders { get; }
        ISecurityRepository Securities { get; }
        IFundRepository Funds { get; }
    }
}
