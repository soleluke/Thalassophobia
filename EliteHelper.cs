using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Thalassophobia.EliteEquipments;

namespace Thalassophobia
{
    public class EliteHelper
    {

        private BepInEx.Configuration.ConfigFile config;
        public List<EliteEquipmentBase> EliteEquipments = new List<EliteEquipmentBase>();

        public EliteHelper(BepInEx.Configuration.ConfigFile config)
        {
            this.config = config;
        }

        // Loads all elite types
        public void LoadAll()
        {
            var EliteEquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EliteEquipmentBase)));

            foreach (var eliteEquipmentType in EliteEquipmentTypes)
            {
                EliteEquipmentBase eliteEquipment = (EliteEquipmentBase)System.Activator.CreateInstance(eliteEquipmentType);
                if (ValidateEliteEquipment(eliteEquipment, EliteEquipments))
                {
                    eliteEquipment.Init(config);

                }
            }
        }

        /// <summary>
        /// A helper to easily set up and initialize an elite equipment from your elite equipment classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="eliteEquipment">A new instance of an EliteEquipmentBase class.</param>
        /// <param name="eliteEquipmentList">The list you would like to add this to if it passes the config check.</param>
        /// <returns></returns>
        public bool ValidateEliteEquipment(EliteEquipmentBase eliteEquipment, List<EliteEquipmentBase> eliteEquipmentList)
        {
            var enabled = config.Bind<bool>("Equipment: " + eliteEquipment.EliteEquipmentName, "Enable", true, "Should this elite equipment appear in runs? If disabled, the associated elite will not appear in runs either.").Value;

            if (enabled)
            {
                eliteEquipmentList.Add(eliteEquipment);
                return true;
            }
            return false;
        }
    }
}
