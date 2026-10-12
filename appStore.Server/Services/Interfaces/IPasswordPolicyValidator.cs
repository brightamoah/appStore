using appStore.Server.Services.Security;

namespace appStore.Server.Services.Interfaces
{
	public interface IPasswordPolicyValidator
	{
		void Validate(
			string password,
			PasswordPolicyDefinition policy 
		);
	}
}