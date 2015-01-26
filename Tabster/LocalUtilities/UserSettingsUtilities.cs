﻿#region

using System.Collections.Generic;
using Tabster.Core.Searching;
using Tabster.Properties;
using Tabster.Utilities.Plugins;

#endregion

namespace Tabster.LocalUtilities
{
    internal class UserSettingsUtilities
    {
        public static ITablatureSearchEngine[] GetEnabledSearchEngines()
        {
            var engines = new List<ITablatureSearchEngine>();

            foreach (var plugin in Program.pluginController)
            {
                foreach (var engine in plugin.GetClassInstances<ITablatureSearchEngine>())
                {
                    var id = GetSearchEngineIdentifier(plugin, engine);

                    if (!Settings.Default.DisabledPlugins.Contains(id))
                    {
                        engines.Add(engine);
                    }
                }
            }

            return engines.ToArray();
        }

        public static string GetSearchEngineIdentifier(TabsterPluginHost plugin, ITablatureSearchEngine engine)
        {
            return string.Format("{0}:{1}", plugin.GUID, engine.Name);
        }
    }
}