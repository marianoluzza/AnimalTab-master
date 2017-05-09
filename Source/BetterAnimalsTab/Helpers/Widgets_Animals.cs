﻿// // Karel Kroeze
// // Widgets_Animals.cs
// // 2016-06-27

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using static BetterAnimalsTab.Resources;

namespace Fluffy
{
	public static class Widgets_Animals
	{
		public static bool IsPregnant( this Pawn pawn )
		{
			Hediff_Pregnant dump;
			return pawn.Pregnant( out dump );
		}
		
		public static bool IsReproductive( this Pawn pawn )
		{
			bool reproductive = pawn.ageTracker.CurLifeStage.reproductive;

			if ( reproductive && BirdsAndBeesActive )
				return pawn.health.capacities.CapableOf( PawnCapacityDef_Reproductive );

			return reproductive;
		}

		private static bool _birdsAndBeesChecked;
		private static bool _birdsAndBeesActive;
		public static PawnCapacityDef PawnCapacityDef_Reproductive;
		public static bool BirdsAndBeesActive
		{
			get
			{
				if ( !_birdsAndBeesChecked )
				{
					PawnCapacityDef_Reproductive = DefDatabase<PawnCapacityDef>.GetNamedSilentFail( "Reproduction" );
					_birdsAndBeesActive = PawnCapacityDef_Reproductive != null;
					_birdsAndBeesChecked = true;

					Log.Message( "AnimalTab :: BirdsAndBees detected, adding fertility capacityDef to fertility filter." );
				}
				return _birdsAndBeesActive;
			}
		}

		public static bool Pregnant( this Pawn pawn, out Hediff_Pregnant hediff )
		{
			// get hediff
			var _hediff = pawn.health.hediffSet.GetFirstHediffOfDef( HediffDefOf.Pregnant );

			// if pregnant, and pregnancy is far enough advanced to be visible
			if ( _hediff?.Visible ?? false )
			{
				hediff = _hediff as Hediff_Pregnant;
				return true;
			}

			// not (visibly) pregnant.
			hediff = null;
			return false;
		}

		public static IEnumerable<Pawn> AnimalsOfColony => Find.VisibleMap.mapPawns.SpawnedPawnsInFaction( Faction.OfPlayer ).Where( p => p.RaceProps.Animal );
		public static IEnumerable<Pawn> ObedientAnimalsOfColony => AnimalsOfColony.Where( p => p.training.IsCompleted( TrainableDefOf.Obedience ) );

		public static List<TrainableDef> Trainables
		{
			get { return TrainableUtility.TrainableDefsInListOrder; }
		}

		public static void SlaughterAnimal( Pawn pawn )
		{
			Find.VisibleMap.designationManager.AddDesignation( new Designation( pawn, DesignationDefOf.Slaughter ) );
		}

		public static void UnSlaughterAnimal( Pawn pawn )
		{
			Find.VisibleMap.designationManager.DesignationOn( pawn, DesignationDefOf.Slaughter ).Delete();
		}

		public static void SlaughterAllAnimals( List<Pawn> pawns )
		{
			if ( pawns.All( p => Find.VisibleMap.designationManager.DesignationOn( p, DesignationDefOf.Slaughter ) != null ) )
			{
				SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
				foreach ( Pawn t in pawns )
				{
					UnSlaughterAnimal( t );
				}
			}
			else
			{
				SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
				foreach ( Pawn t in pawns )
				{
					if ( Find.VisibleMap.designationManager.DesignationOn( t, DesignationDefOf.Slaughter ) == null )
						SlaughterAnimal( t );
				}
			}
		}

		public static void HuntAnimal(Pawn pawn)
		{
			Find.VisibleMap.designationManager.AddDesignation(new Designation(pawn, DesignationDefOf.Hunt));
		}

		public static void UnHuntAnimal(Pawn pawn)
		{
			Find.VisibleMap.designationManager.DesignationOn(pawn, DesignationDefOf.Hunt).Delete();
		}

		public static void HuntAllAnimals(List<Pawn> pawns)
		{
			if (pawns.All(p => Find.VisibleMap.designationManager.DesignationOn(p, DesignationDefOf.Hunt) != null))
			{
				SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
				foreach (Pawn t in pawns)
				{
					UnHuntAnimal(t);
				}
			}
			else
			{
				SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
				foreach (Pawn t in pawns)
				{
					if (Find.VisibleMap.designationManager.DesignationOn(t, DesignationDefOf.Hunt) == null)
						HuntAnimal(t);
				}
			}
		}

		public static void TameAnimal(Pawn pawn)
		{
			Find.VisibleMap.designationManager.AddDesignation(new Designation(pawn, DesignationDefOf.Tame));
		}

		public static void UnTameAnimal(Pawn pawn)
		{
			Find.VisibleMap.designationManager.DesignationOn(pawn, DesignationDefOf.Tame).Delete();
		}

		public static void TameAllAnimals(List<Pawn> pawns)
		{
			if (pawns.All(p => Find.VisibleMap.designationManager.DesignationOn(p, DesignationDefOf.Tame) != null))
			{
				SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
				foreach (Pawn t in pawns)
				{
					UnTameAnimal(t);
				}
			}
			else
			{
				SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
				foreach (Pawn t in pawns)
				{
					if (Find.VisibleMap.designationManager.DesignationOn(t, DesignationDefOf.Tame) == null)
						TameAnimal(t);
				}
			}
		}

		public static void DoTrainingRow( ref float curX, Pawn pawn )
		{
			List<TrainableDef> trainableDefs = TrainableUtility.TrainableDefsInListOrder;
			var iconSize = 16f;
			float widthOffset = ( widthPerTrainable - iconSize ) / 2;
			float heightOffset = ( rowHeight - iconSize ) / 2;

			foreach ( TrainableDef t in trainableDefs )
			{
				var bg = new Rect( curX, 0, widthPerTrainable, rowHeight );
				var icon = new Rect( curX + widthOffset, heightOffset, iconSize, iconSize );
				curX += widthPerTrainable;
				bool vis;
				if (pawn.training != null)
				{
					AcceptanceReport report = pawn.training.CanAssignToTrain(t, out vis);
					TooltipHandler.TipRegion(bg, GetTrainingTip(pawn, t, report));
					if (vis)
					{
						DrawTrainingButton(icon, pawn, t, report);
						if (report.Accepted && !pawn.training.IsCompleted(t))
						{
							if (Widgets.ButtonInvisible(bg))
							{
								ToggleTraining(t, pawn, report);
							}
							if (Mouse.IsOver(icon))
							{
								GUI.DrawTexture(icon, TexUI.HighlightTex);
							}
						}
					}
				}
			}
		}

		public static void ToggleTraining( TrainableDef td, Pawn pawn, AcceptanceReport ar )
		{
			bool train = !pawn.training.GetWanted( td );
			if ( ar.Accepted )
				SetWantedRecursive( td, pawn, train );
		}


		// Get method named "GetSteps", which is internal(non-public) and not static(instance)
		public static MethodInfo getSteps = typeof( Pawn_TrainingTracker ).GetMethod( "GetSteps",
																					BindingFlags.NonPublic |
																					BindingFlags.Instance );

		public static void DrawTrainingButton( Rect rect, Pawn pawn, TrainableDef td, AcceptanceReport ar )
		{
			if ( ar.Accepted )
			{
				if ( pawn.training.IsCompleted( td ) )
				{
					GUI.DrawTexture( rect, CheckWhite );
				}
				else if ( pawn.training.GetWanted( td ) )
				{
					GUI.DrawTexture( rect, Widgets.CheckboxOnTex );

					if ( getSteps == null )
					{
#if DEBUG
						Log.Error( "GetSteps is null!" );
#endif
						return;
					}

					// Call "GetSteps" from instance pawn.training, parameter is td.
					object curSteps = getSteps.Invoke( pawn.training, new object[] { td } );
					int steps = td.steps;
					// Return value of Invoke(...) is Object; thus casting into int type.
					float barHeight = rect.height / steps * (int)curSteps;
					var bar = new Rect( rect.xMax - 5f, rect.yMax - barHeight, 3f, barHeight );
					GUI.DrawTexture( bar, BarBg );
				}
			}
		}

		public static string GetTrainingTip( Pawn pawn, TrainableDef td, AcceptanceReport ar )
		{
			var label = new StringBuilder();
			label.AppendLine( td.LabelCap );
			if ( !ar.Accepted )
			{
				label.AppendLine( ar.Reason );
			}
			else
			{
				if ( pawn.training.IsCompleted( td ) )
				{
					label.Append( "Fluffy.TrainingCompleted".Translate() );
				}
				else
				{
					label.AppendLine( !pawn.training.GetWanted( td )
										  ? "Fluffy.NotTraining".Translate()
										  : "Fluffy.CurrentlyTraining".Translate() );

					MethodInfo getSteps = typeof( Pawn_TrainingTracker ).GetMethod( "GetSteps",
																					BindingFlags.NonPublic |
																					BindingFlags.Instance );
					if ( getSteps == null )
					{
#if DEBUG
						Log.Error( "GetSteps is null!" );
#endif
						return label.ToString();
					}

					object curSteps = getSteps.Invoke( pawn.training, new object[] { td } );
					int steps = td.steps;
					label.Append( "Fluffy.StepsCompleted".Translate( curSteps, steps ) );
				}
			}
			return label.ToString();
		}

		public static void ToggleAllTraining( TrainableDef td, List<Pawn> pawns )
		{
			var visible = new bool[pawns.Count];
			var canAssign = new AcceptanceReport[pawns.Count];
			bool[] assigned = pawns.Select( p => p.training.GetWanted( td ) ).ToArray();
			bool[] trained = pawns.Select( p => p.training.IsCompleted( td ) ).ToArray();
			var all = true;

			for ( var i = 0; i < pawns.Count; i++ )
			{
				canAssign[i] = pawns[i].training.CanAssignToTrain( td, out visible[i] );
				if ( !assigned[i] && !trained[i] && canAssign[i].Accepted )
					all = false;
			}

			for ( var i = 0; i < pawns.Count; i++ )
			{
				if ( all && assigned[i] )
				{
					SetWantedRecursive( td, pawns[i], false );
				}
				else if ( !assigned[i] && canAssign[i].Accepted && !trained[i] )
				{
					SetWantedRecursive( td, pawns[i], true );
				}
			}
		}

		private static void SetWantedRecursive( TrainableDef td, Pawn pawn, bool checkOn )
		{
			pawn.training.SetWanted( td, checkOn );
			if ( checkOn )
			{
				SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
				if ( td.prerequisites != null )
				{
					foreach ( TrainableDef trainable in td.prerequisites )
					{
						SetWantedRecursive( trainable, pawn, true );
					}
				}
			}
			else
			{
				SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
				IEnumerable<TrainableDef> enumerable = from t in DefDatabase<TrainableDef>.AllDefsListForReading
													   where t.prerequisites != null && t.prerequisites.Contains( td )
													   select t;
				foreach ( TrainableDef current in enumerable )
				{
					SetWantedRecursive( current, pawn, false );
				}
			}
		}

		public static FloatMenuOption MassAssignMaster_FloatMenuOption( Pawn colonist )
		{
			var animals = ObedientAnimalsOfColony;
			if ( colonist == null )
			{
				return new FloatMenuOption( "Fluffy.MassAssignMasterNone".Translate(),
											delegate
											{ MassAssignMaster( null, animals ); } );
			}

			// get number of animals this pawn could be the master of.
			var skill = colonist.skills.GetSkill( SkillDefOf.Animals ).Level;
			var eligibleAnimals = animals.Where( p => Mathf.RoundToInt( p.GetStatValue( StatDefOf.MinimumHandlingSkill ) ) < skill );
			Action action = delegate
			{ MassAssignMaster( colonist, eligibleAnimals ); };

			return new FloatMenuOption( "Fluffy.MassAssignMaster".Translate( colonist.NameStringShort, skill, eligibleAnimals.Count(), animals.Count() ),
				eligibleAnimals.Any() ? action : null );
		}

		private static void MassAssignMaster( Pawn pawn, IEnumerable<Pawn> animals )
		{
			foreach ( Pawn animal in animals )
				animal.playerSettings.master = pawn;
		}

		public static void MassAssignMasterBonded()
		{
			// assign bonded animals to their bond-master, if not bonded, or bonded has low skill, do not touch.
			foreach ( Pawn animal in ObedientAnimalsOfColony )
			{
				// get bond
				var bond = animal.relations.GetFirstDirectRelationPawn( PawnRelationDefOf.Bond, p => p.Faction == Faction.OfPlayer );
				if ( bond == null || bond.skills.GetSkill( SkillDefOf.Animals ).Level < animal.GetStatValue( StatDefOf.MinimumHandlingSkill ) )
					continue;
				animal.playerSettings.master = bond;
			}
		}
	}
}
