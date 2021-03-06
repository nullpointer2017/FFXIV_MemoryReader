﻿using Advanced_Combat_Tracker;
using System.Collections.Generic;
using System.Windows.Forms;
using TamanegiMage.FFXIV_MemoryReader.Core;

namespace TamanegiMage.FFXIV_MemoryReader.Base
{
    public class MemoryPlugin : IActPluginV1
    {
        public PluginCore Core;
        public void DeInitPlugin()
        {
            if (Core != null)
            {
                Core.Dispose();
            }
        }

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            Core = new PluginCore();
            Core.Init(pluginScreenSpace, pluginStatusText);
        }

        public List<Model.CombatantV1> GetCombatantsV1() => Core.GetCombatantsV1();
        public Model.CameraInfoV1 GetCameraInfoV1() => Core.GetCameraInfoV1();
        public List<Model.HotbarRecastV1> GetHotbarRecastV1() => Core.GetHotbarRecastV1();

    }
}
