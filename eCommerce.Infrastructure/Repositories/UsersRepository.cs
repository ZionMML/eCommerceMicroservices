using eCommerce.Core.DTO;
using eCommerce.Core.Entities;
using eCommerce.Core.RepositoryContracts;

namespace eCommerce.Infrastructure.Repositories
{
    internal class UsersRepository : IUsersRepository
    {
        public async Task<ApplicationUser?> AddUser(ApplicationUser user)
        {
           // Generate a new unique User ID for the user
           user.UserID = Guid.NewGuid();

            return user;
        }

        public async Task<ApplicationUser?> GetUserByEmailAndPassword(string? email, string? password)
        {
            return new ApplicationUser() {
                UserID = Guid.NewGuid(),
                Email = email, 
                Password = password,
                PersonName = "Person name",
                Gender = GenderOptions.Male.ToString()
            };
        }
    }
}
