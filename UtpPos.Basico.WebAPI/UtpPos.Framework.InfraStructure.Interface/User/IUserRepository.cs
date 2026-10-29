using DomainUser = UtpPos.Framework.Domain.Entity.User.User;

namespace UtpPos.Framework.InfraStructure.Interface.User
{
    public interface IUserRepository
    {
        Task<IEnumerable<DomainUser>> ListAsync(
            int? businessId,
            int? documentTypeId,
            string? status,
            string? documentNumber,
            string? name,
            string? email);

        Task<DomainUser?> GetByIdAsync(int userId);
        Task<int> CreateAsync(DomainUser user);
        Task<int> UpdateAsync(DomainUser user, bool clearBusinessId, bool clearPhone);
        Task<int> DeactivateAsync(int userId);
    }
}
