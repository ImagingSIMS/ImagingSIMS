using ImagingSIMS.Common;
using ImagingSIMS.Common.Registry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ImagingSIMS.Data.Spectra
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class SpectrumTypeFileDescriptionAttribute : System.Attribute
    {
        readonly string _description;

        public SpectrumTypeFileDescriptionAttribute(string description)
        {
            _description = description;
        }

        public string Description
        {
            get { return _description; }
        }

    }

    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class SpectrumTypeFileExtensionAttribute : Attribute
    {
        readonly string[] _extensions;

        public SpectrumTypeFileExtensionAttribute(string extension)
        {
            _extensions = new string[] { extension };
        }
        public SpectrumTypeFileExtensionAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        public string[] Extensions
        {
            get { return _extensions; }
        }
    }

    public static class SpectrumFileExtensions
    {
        private static Dictionary<string, SpectrumType> _extensionMap;

        static SpectrumFileExtensions()
        {
            InitializeExtensionMap();
        }

        private static void InitializeExtensionMap()
        {
            _extensionMap = new Dictionary<string, SpectrumType>();

            var biotofExtensions = EnumEx.GetAttributeOfType<SpectrumTypeFileExtensionAttribute>(SpectrumType.BioToF).Extensions;
            var j105Extensions = EnumEx.GetAttributeOfType<SpectrumTypeFileExtensionAttribute>(SpectrumType.J105).Extensions;
            var camecaNanoSIMSExtensions = EnumEx.GetAttributeOfType<SpectrumTypeFileExtensionAttribute>(SpectrumType.CamecaNanoSIMS).Extensions;
            var cameca1280Extensions = EnumEx.GetAttributeOfType<SpectrumTypeFileExtensionAttribute>(SpectrumType.Cameca1280).Extensions;

            foreach (var extension in biotofExtensions)
            {
                _extensionMap.Add(extension.ToLower(), SpectrumType.BioToF);
            }
            foreach (var extension in j105Extensions)
            {
                _extensionMap.Add(extension.ToLower(), SpectrumType.J105);
            }
            foreach (var extension in camecaNanoSIMSExtensions)
            {
                _extensionMap.Add(extension.ToLower(), SpectrumType.CamecaNanoSIMS);
            }
            foreach (var extension in cameca1280Extensions)
            {
                _extensionMap.Add(extension.ToLower(), SpectrumType.Cameca1280);
            }
        }
        public static SpectrumType GetTypeForFileExtension(string extension)
        {
            var toMatch = extension.ToLower();
            if (!toMatch.StartsWith("."))
            {
                toMatch = $".{toMatch}";
            }

            if (_extensionMap.ContainsKey(toMatch))
                return _extensionMap[toMatch];

            else return SpectrumType.None;
        }
        public static string GetFilterForDefaultProgram(DefaultProgram defaultProgram)
        {
            StringBuilder filter = new StringBuilder();
            switch (defaultProgram)
            {
                case DefaultProgram.BioToF:
                    filter.Append(GetFileFilter(SpectrumType.BioToF) + "|");
                    filter.Append(GetFileFilter(SpectrumType.J105) + "|");
                    filter.Append(GetFileFilter(SpectrumType.CamecaNanoSIMS) + "|");
                    filter.Append(GetFileFilter(SpectrumType.Cameca1280));
                    break;
                case DefaultProgram.J105:
                    filter.Append(GetFileFilter(SpectrumType.J105) + "|");
                    filter.Append(GetFileFilter(SpectrumType.BioToF) + "|");
                    filter.Append(GetFileFilter(SpectrumType.CamecaNanoSIMS) + "|");
                    filter.Append(GetFileFilter(SpectrumType.Cameca1280));
                    break;
                case DefaultProgram.Cameca1280:
                    filter.Append(GetFileFilter(SpectrumType.Cameca1280) + "|");
                    filter.Append(GetFileFilter(SpectrumType.CamecaNanoSIMS) + "|");
                    filter.Append(GetFileFilter(SpectrumType.BioToF) + "|");
                    filter.Append(GetFileFilter(SpectrumType.J105));
                    break;
                case DefaultProgram.CamecaNanoSIMS:
                    filter.Append(GetFileFilter(SpectrumType.CamecaNanoSIMS) + "|");
                    filter.Append(GetFileFilter(SpectrumType.Cameca1280) + "|");
                    filter.Append(GetFileFilter(SpectrumType.BioToF) + "|");
                    filter.Append(GetFileFilter(SpectrumType.J105));
                    break;
            }

            return filter.ToString();
        }

        private static string GetFileFilter(SpectrumType type)
        {
            return $"{GetFileDescription(type)} ({GetFileExtension(type)})|{GetFileExtensionFilter(type)}";
        }
        private static string GetFileDescription(SpectrumType type)
        {
            var descriptionAttribute = EnumEx.GetAttributeOfType<SpectrumTypeFileDescriptionAttribute>(type);
            return descriptionAttribute.Description;
        }
        private static string GetFileExtension(SpectrumType type)
        {
            var fileExtensionAttribute = EnumEx.GetAttributeOfType<SpectrumTypeFileExtensionAttribute>(type);
            StringBuilder sb = new StringBuilder();
            foreach (var extension in fileExtensionAttribute.Extensions)
            {
                sb.Append($"{extension}, ");
            }
            sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }
        private static string GetFileExtensionFilter(SpectrumType type)
        {
            var fileExtensionAttribute = EnumEx.GetAttributeOfType<SpectrumTypeFileExtensionAttribute>(type);
            StringBuilder sb = new StringBuilder();
            foreach (var extension in fileExtensionAttribute.Extensions)
            {
                sb.Append($"*{extension};");
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }
    }

    public enum SpectrumType
    {
        [SpectrumTypeFileDescription("Ionoptika compressed V2 files")]
        [SpectrumTypeFileExtension(new string[] { ".zip", ".IonoptikaIA2DspectrV2" })]
        J105,

        [Description("Bio-ToF")]
        [SpectrumTypeFileDescription("Bio-ToF Spectra Files")]
        [SpectrumTypeFileExtension(new string[] { ".xyt", ".dat" })]
        BioToF,

        Generic,

        None,

        [Description("CAMECA 1280")]
        [SpectrumTypeFileDescription("Cameca 1280 Spectra Files")]
        [SpectrumTypeFileExtension(".imp")]
        Cameca1280,

        [Description("CAMECA NanoSIMS")]
        [SpectrumTypeFileDescription("Cameca NanoSIMS Spectra Files")]
        [SpectrumTypeFileExtension(".im")]
        CamecaNanoSIMS
    }
}
