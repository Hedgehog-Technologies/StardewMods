namespace HedgeTech.Common.Interfaces
{
	public interface IContentPatcherApi
	{
		/*********
		** Accessors
		*********/
		/// <summary>Whether the conditions API is initialized and ready for use.</summary>
		/// <remarks>Due to the Content Patcher lifecycle, the conditions API becomes available roughly two ticks after the <see cref="IGameLoopEvents.GameLaunched"/> event.</remarks>
		bool IsConditionsApiReady { get; }
	}
}
