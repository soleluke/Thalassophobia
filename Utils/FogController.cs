using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Thalassophobia.Utils
{
    class FogController : MonoBehaviour
    {
        public float time = 0f;
        public float tickPeriodSeconds;
        public float healthFractionPerSecond;
        public float healthFractionRampCoefficientPerSecond;
        public CharacterBody victim;
        public CharacterBody attacker;
        public BuffDef buff;
        private int stacks = 1;

        private void FixedUpdate()
        {
            this.time -= Time.fixedDeltaTime;
            if (!victim.HasBuff(buff))
            {
                Destroy(this);
            }
            else if (this.time <= 0)
            {
                DamageInfo damageInfo = new DamageInfo();
                damageInfo.attacker = attacker.gameObject;
                damageInfo.crit = false;
                damageInfo.damage = healthFractionPerSecond * (1f + (float)stacks * healthFractionRampCoefficientPerSecond * tickPeriodSeconds) * tickPeriodSeconds * victim.healthComponent.fullCombinedHealth; ;
                damageInfo.force = Vector3.zero;
                damageInfo.inflictor = base.gameObject;
                damageInfo.position = victim.corePosition;
                damageInfo.procCoefficient = 0f;
                damageInfo.damageColorIndex = DamageColorIndex.Void;
                damageInfo.damageType = DamageType.DoT;
                damageInfo.dotIndex = DotController.DotIndex.None;
                victim.healthComponent.TakeDamage(damageInfo);
                time = tickPeriodSeconds;
                stacks++;
            }
        }
    }
}
