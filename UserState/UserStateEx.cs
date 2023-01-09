using System.Reflection;
using Utilities.Reflection;

namespace Discord.UserState
{
	public static class UserStateEx
	{
		private static Dictionary<string, Type> statesTypes = LoadTypes();

		private static Dictionary<string, Type> LoadTypes()
		{
			Assembly[]? assemblies = AppDomain.CurrentDomain.GetAssemblies();

			Dictionary<string, Type> dict = new();

			if (assemblies is null) return dict;

			for (int i = 0; i < assemblies.Length; i++)
			{
				Type[]? types = assemblies[i]?.GetTypes();

				if (types is null) continue;

				for (int i1 = 0; i1 < types.Length; i1++)
				{
					Type? type = types[i1];

					if (type is null || !type.Inherits(typeof(IUserState)) || type.FullName is null)
						continue;

					dict[type.FullName] = type;
				}
			}

			return dict;
		}
	}
}
