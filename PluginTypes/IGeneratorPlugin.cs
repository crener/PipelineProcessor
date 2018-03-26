﻿using System.Collections.Generic;

namespace PluginTypes
{
    public interface IGeneratorPlugin : IPlugin
    {
        List<byte[]> StaticData(string nodeValue);
        string DefaultValue { get; }
    }
}
