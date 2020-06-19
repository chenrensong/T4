using Coding4Fun.VisualStudio.Utilities.Internal;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class RemoteSettingsValidator : IRemoteSettingsValidator
	{
		internal static readonly string CyclesDetectedMessage = "Cycles detected in Scopes";

		private ICycleDetection cycleDetection;

		private IScopesStorageHandler scopesStorageHandler;

		public RemoteSettingsValidator(ICycleDetection cycleDetection, IScopesStorageHandler scopesStorageHandler)
		{
			CodeContract.RequiresArgumentNotNull<ICycleDetection>(cycleDetection, "cycleDetection");
			CodeContract.RequiresArgumentNotNull<IScopesStorageHandler>(scopesStorageHandler, "scopesStorageHandler");
			this.cycleDetection = cycleDetection;
			this.scopesStorageHandler = scopesStorageHandler;
		}

		public void ValidateDeserialized(DeserializedRemoteSettings remoteSettings)
		{
			ValidateScopes(remoteSettings.Scopes);
		}

		public void ValidateStored()
		{
			List<Scope> list = new List<Scope>();
			foreach (string allScope in scopesStorageHandler.GetAllScopes())
			{
				string scope = scopesStorageHandler.GetScope(allScope);
				list.Add(new Scope
				{
					Name = allScope,
					ScopeString = scope
				});
			}
			ValidateScopes(list);
		}

		private void ValidateScopes(IEnumerable<Scope> scopes)
		{
			if (cycleDetection.HasCycles(scopes))
			{
				throw new RemoteSettingsValidationException(CyclesDetectedMessage);
			}
		}
	}
}
