using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Thalassophobia.Utils
{
    class SimpleTimer : MonoBehaviour
    {
        public float time = 0f;

        public delegate void Hook_OnTimerEnd();
        public Hook_OnTimerEnd OnTimerEnd = () => {};

        private void FixedUpdate()
        {
            this.time -= Time.fixedDeltaTime;
            if (this.time <= 0) {
                if (Plugin.DEBUG) {
                    Log.LogInfo("Timer End");
                }
                this.OnTimerEnd();
                Destroy(this);
            }
        }
    }
}
