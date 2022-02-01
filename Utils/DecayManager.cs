using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Mod.Utils
{
    class DecayManager : MonoBehaviour
    {
        public float dotDuration = 1;

        public float interval = 1;

        public Transform auraEffectTransform;

        private float timer;

        private float radius = 10;

        private CameraTargetParams.AimRequest aimRequest;

        private CameraTargetParams cameraTargetParams;


        private void Awake()
        {

        }

        private void OnDestroy()
        {
            if (this.auraEffectTransform)
            {
                UnityEngine.Object.Destroy(this.auraEffectTransform.gameObject);
                this.auraEffectTransform = null;
            }
            CameraTargetParams.AimRequest aimRequest = this.aimRequest;
            if (aimRequest == null)
            {
                return;
            }
            aimRequest.Dispose();
        }

        private void FixedUpdate()
        {
            CharacterBody attachedBody = this.gameObject.GetComponent<CharacterBody>();
            TeamIndex team = attachedBody.teamComponent.teamIndex;
            this.timer -= Time.fixedDeltaTime;
            if (this.timer <= 0f)
            {
                float damageMultiplier = 1f;
                this.timer = this.interval;
                Collider[] array = Physics.OverlapSphere(base.transform.position, this.radius, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Collide);
                GameObject[] array2 = new GameObject[array.Length];
                int count = 0;
                for (int i = 0; i < array.Length; i++)
                {
                    CharacterBody characterBody = Util.HurtBoxColliderToBody(array[i]);
                    GameObject gameObject = characterBody ? characterBody.gameObject : null;
                    if (gameObject && Array.IndexOf<GameObject>(array2, gameObject, 0, count) == -1 && team != characterBody.teamComponent.teamIndex)
                    {
                        DotController.InflictDot(gameObject, this.gameObject, DotController.DotIndex.Helfire, this.dotDuration, damageMultiplier);
                        array2[count++] = gameObject;
                    }
                }
            }
        }

        // Token: 0x06001101 RID: 4353 RVA: 0x000478C8 File Offset: 0x00045AC8
        private void LateUpdate()
        {
            
            if (this.gameObject)
            {
                CharacterBody attachedBody = this.gameObject.GetComponent<CharacterBody>();
                if (attachedBody)
                {
                    if (this.auraEffectTransform)
                    {
                        this.auraEffectTransform.position = attachedBody.corePosition;
                        this.auraEffectTransform.localScale = new Vector3(this.radius, this.radius, this.radius);
                        if (!this.cameraTargetParams)
                        {
                            this.cameraTargetParams = attachedBody.GetComponent<CameraTargetParams>();
                            return;
                        }
                        if (this.aimRequest == null)
                        {
                            this.aimRequest = this.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
                        }
                    }
                }
            }
            
        }
    }
}
