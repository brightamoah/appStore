using System.ComponentModel.DataAnnotations;

namespace appStore.Server.Utils
{
	public class GeneralUtilities
	{
		public static string ValidateAndNormalizeName( string? name )
		{
			if (string.IsNullOrWhiteSpace( name ))
			{
				throw new ValidationException( "Name cannot be null or whitespace." );
			}
			if (name.Length > 100)
			{
				throw new ValidationException( "Name cannot exceed 100 characters." );
			}

			return name.Trim();
		}
	}
}