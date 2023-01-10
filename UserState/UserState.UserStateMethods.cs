using Discord.Utils.Emotes;
using System.Reflection;

namespace Discord.UserState
{
	public partial class CoreUserState
	{
		protected class UserStateMethods
		{
			public enum Result
			{
				NotFound,
				Success,
				Failed
			}

			private const BindingFlags flags =
					BindingFlags.Public | BindingFlags.NonPublic |
					BindingFlags.InvokeMethod |
					BindingFlags.Instance | BindingFlags.Static;

			private interface IMethodsContainer
			{
				bool TryAdd(MethodInfo method, out string? emote);

				bool TryGet(string key, out MethodInfo method);
			}

			private abstract class MethodsContainer<TAttribute> : IMethodsContainer
				where TAttribute : StateOptionAttribute
			{
				private readonly Type stateType;
				private readonly Type attributeType = typeof(TAttribute);

				private readonly Dictionary<string, MethodInfo> methods = new();

				public MethodsContainer(Type stateType)
				{
					this.stateType = stateType;
				}

				public bool TryAdd(MethodInfo method, out string? emote)
				{
					emote = null;

					if (!TryGetAttribute(method, out TAttribute attribute)) return false;

					if (!ValidateMethod(method, attribute)) return false;

					bool add = !string.IsNullOrEmpty(attribute.emote);

					if (add)
					{
						emote = attribute.emote;
						AddMethod(method, emote);
					}

					if (attribute.aliases is null) return add;

					for (int k = 0; k < attribute.aliases.Length; k++)
					{
						string? key = attribute.aliases[k];

						if (string.IsNullOrEmpty(key)) continue;

						AddMethod(method, key);

						add = true;
					}

					return add;
				}

				public bool TryGet(string key, out MethodInfo method)
					=> methods.TryGetValue(key, out method!);

				private void AddMethod(MethodInfo method, string key)
				{
					if (methods.ContainsKey(key))
					{
						throw new Exception($"Duplicate method for key {key}. " +
							$"{stateType.FullName}.{method.Name}");
					}

					methods[key] = method;
				}

				private bool TryGetAttribute(MethodInfo method, out TAttribute attribute)
				{
#pragma warning disable CS8601 // Possible null reference assignment.
					attribute = Attribute.GetCustomAttribute(method, attributeType) as TAttribute;
#pragma warning restore CS8601 // Possible null reference assignment.

					return attribute != null;
				}

				protected abstract bool ValidateMethod(MethodInfo method, TAttribute attribute);
			}

			private class UserStateVerificationMethods : MethodsContainer<StateOptionVerificationAttribute>
			{
				public UserStateVerificationMethods(Type stateType) : base(stateType) { }

				protected override bool ValidateMethod(MethodInfo method, StateOptionVerificationAttribute attribute)
				{
					if (method.GetParameters().Length != 0) return false;

					if (method.ReturnType != typeof(bool) ||
						method.ReturnType != typeof(Task<bool>))
						return false;

					return true;
				}
			}

			private class UserStateInvocationMethods : MethodsContainer<StateOptionAttribute>
			{
				public UserStateInvocationMethods(Type stateType) : base(stateType) { }

				protected override bool ValidateMethod(MethodInfo method, StateOptionAttribute attribute)
				{
					Type returnType = method.ReturnType;

					bool isCompatibleReturnType =
						returnType is null ||
						returnType == typeof(void) ||
						returnType == typeof(Task);

					if (!isCompatibleReturnType) return false;

					return true;
				}
			}

			private static readonly Dictionary<Type, IMethodsContainer> invokationMethods = new();
			private static readonly Dictionary<Type, IMethodsContainer> verificationMethods = new();
			private static readonly Dictionary<Type, string[]> emotesValues = new();

			private static TContainer Get<TContainer>(Type stateType)
				where TContainer : IMethodsContainer
			{
				Type containerType = typeof(TContainer);

				Dictionary<Type, IMethodsContainer> dict =
					containerType == typeof(UserStateVerificationMethods) ? verificationMethods
					:
					containerType == typeof(UserStateInvocationMethods) ? invokationMethods
					: throw new ArgumentOutOfRangeException();

				if (dict.TryGetValue(stateType, out IMethodsContainer? container) && container is not null)
					return (TContainer)container;

				Load(stateType);

				return (TContainer)dict[stateType];
			}

			private static void Load(Type stateType)
			{
				IMethodsContainer verification = new UserStateVerificationMethods(stateType);
				verificationMethods[stateType] = verification;

				IMethodsContainer invokation = new UserStateInvocationMethods(stateType);
				invokationMethods[stateType] = invokation;

				List<string> emotesList = new();

				MethodInfo[] methods = stateType.GetMethods(flags);

				for (int i = 0; i < methods.Length; i++)
				{
					MethodInfo method = methods[i];

					verification.TryAdd(method, out _);

					if (!invokation.TryAdd(method, out string? emote) || emote is null) continue;

					emotesList.Add(emote);
				}

				emotesValues[stateType] = emotesList.ToArray();
			}

			public static async Task<Result> Verify(IUserState state, string key)
			{
				IMethodsContainer container = Get<UserStateVerificationMethods>(state.GetType());

				if (!container.TryGet(key, out MethodInfo method)) return Result.NotFound;

				object? result = method.Invoke(method.IsStatic ? null : state, null);

				return result switch
				{
					Task<bool> task => await task,

					bool success => success,

					_ => false
				} ? Result.Success : Result.Failed;
			}

			public static async Task<Result> Invoke(IUserState state, string key)
			{
				IMethodsContainer container = Get<UserStateInvocationMethods>(state.GetType());

				if (container is null || !container.TryGet(key, out MethodInfo method))
					return Result.NotFound;

				if (await Verify(state, key) == Result.Failed) return Result.Failed;

				object? result = method.Invoke(method.IsStatic ? null : state, null);

				if (result is Task task) await task;

				return Result.Success;
			}

			public static async Task<IEnumerable<IEmote>> GetEmotes(IUserState state)
			{
				Type type = state.GetType();

				Get<UserStateInvocationMethods>(type);

				if (!emotesValues.TryGetValue(type, out string[]? values) || 
					values is null || values.Length == 0)
					return Enumerable.Empty<IEmote>();

				List<IEmote> results = new();

				for (int i = 0; i < values.Length; i++) 
				{
					string? str = values[i];

					if (string.IsNullOrEmpty(str)) continue;

					if (await Verify(state, str) == Result.Failed) continue;

					if (Emote.TryParse(str, out Emote emote))
						results.Add(emote);
					else if (Emoji.TryParse(str, out Emoji emoji))
						results.Add(emoji);
				}

				return results;
			}
		}
	}
}
