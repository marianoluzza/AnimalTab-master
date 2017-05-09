using RimWorld;
using UnityEngine;
using Verse;

namespace Fluffy
{
	public class Filter_TrainableIntelligence : Filter
	{
		#region Properties

		public override string Author => "XChronos";
		public override string Label => "intel";

		public override Texture2D[] Textures => new[]
												{
													ContentFinder<Texture2D>.Get( "UI/FilterStates/trainable" ),
													ContentFinder<Texture2D>.Get( "UI/FilterStates/not_trainable" ),
													ContentFinder<Texture2D>.Get( "UI/FilterStates/all_large" )
												};

		protected internal override FilterType State { get; set; } = FilterType.None;

		#endregion Properties

		#region Methods

		public override bool IsAllowed( Pawn p )
		{
			if (State == FilterType.None)
				return true;
			bool trainable = p.RaceProps.trainableIntelligence > TrainableIntelligence.None;
			if ( State == FilterType.True && trainable)
				return true;
			if ( State == FilterType.False && !trainable)
				return true;
			return false;
		}

		#endregion Methods
	}
}