using ClubPadel.DL;

namespace ClubPadel.Services
{
    public class SecurityService
    {
        private UserRepository _userRepository { get; set; }
        private UserRepository _roleRepository { get; set; }

        public SecurityService(UserRepository repository)
        {
            _roleRepository = repository;
        }

        public async Task CheckAccess(Guid participantId, Guid userId)
        {
            if (participantId == userId)
            {
                return;
            }
            var role = _roleRepository.GetById(userId);
        }
    }
}
