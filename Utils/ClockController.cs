using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static R2API.DamageAPI;

namespace Thalassophobia.Utils
{
    class ClockController : MonoBehaviour
    {
        public float time = 1f;
        public ModdedDamageType damageType;
        public DamageColorIndex index;
        public CharacterBody owner;
        public List<Stack<float>> pendingDamage = new List<Stack<float>>();

        private void FixedUpdate()
        {
            this.time -= Time.fixedDeltaTime;
            if (this.time <= 0)
            {
                time = 1.0f;
                float totalDamage = 0;
                for (int i = pendingDamage.Count - 1; i >= 0; i--)
                {
                    Stack<float> damageStack = pendingDamage[i];
                    totalDamage += damageStack.Pop();
                    if (damageStack.Count == 0)
                    {
                        pendingDamage.Remove(damageStack);
                    }
                }

                if (totalDamage > 0)
                {
                    DamageInfo damageInfo = new DamageInfo()
                    {
                        attacker = null,
                        crit = false,
                        damage = totalDamage,
                        force = Vector3.zero,
                        inflictor = null,
                        position = owner.corePosition,
                        procCoefficient = 0f,
                        damageColorIndex = DamageColorIndex.Fragile,
                    };
                    DamageAPI.AddModdedDamageType(damageInfo, damageType);
                    owner.healthComponent.TakeDamage(damageInfo);
                }
            }
        }
    }
}
