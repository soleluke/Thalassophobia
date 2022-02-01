using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Facepunch.Steamworks;
using JetBrains.Annotations;
using Rewired;
using RoR2;
using RoR2.Networking;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Mod.Utils
{
	internal class GeneralHelpers
	{
		public static void CleanseBody(CharacterBody characterBody, bool removeDebuffs, bool removeBuffs, bool removeDots, bool removeStun, bool removeNearbyProjectiles)
		{
			if (removeDebuffs)
			{
				BuffIndex buffIndex = (BuffIndex)0;
				BuffIndex buffCount = (BuffIndex)BuffCatalog.buffCount;
				while (buffIndex < buffCount)
				{
					BuffDef buffDef = BuffCatalog.GetBuffDef(buffIndex);
					if ((buffDef.isDebuff && removeDebuffs) || (!buffDef.isDebuff && removeBuffs))
					{
						characterBody.ClearTimedBuffs(buffIndex);
					}
					buffIndex++;
				}
			}
			if (removeDots)
			{
				DotController.RemoveAllDots(characterBody.gameObject);
			}
			if (removeStun)
			{
				SetStateOnHurt component = characterBody.GetComponent<SetStateOnHurt>();
				if (component)
				{
					component.Cleanse();
				}
			}
			if (removeNearbyProjectiles)
			{
				float num = 6f;
				float num2 = num * num;
				TeamIndex teamIndex = characterBody.teamComponent.teamIndex;
				List<ProjectileController> instancesList = InstanceTracker.GetInstancesList<ProjectileController>();
				List<ProjectileController> list = new List<ProjectileController>();
				int i = 0;
				int count = instancesList.Count;
				while (i < count)
				{
					ProjectileController projectileController = instancesList[i];
					if (projectileController.teamFilter.teamIndex != teamIndex && (projectileController.transform.position - characterBody.corePosition).sqrMagnitude < num2)
					{
						list.Add(projectileController);
					}
					i++;
				}
				int j = 0;
				int count2 = list.Count;
				while (j < count2)
				{
					ProjectileController projectileController2 = list[j];
					if (projectileController2)
					{
						UnityEngine.Object.Destroy(projectileController2.gameObject);
					}
					j++;
				}
			}
		}
	}
}
