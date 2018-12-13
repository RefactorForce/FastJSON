using System;
using System.Collections.Generic;

namespace FastJSON
{
    public sealed class Parameters
    {
        /// <summary>
        /// Use the optimized fast Dataset Schema format (default = True)
        /// </summary>
        public bool UseOptimizedDatasetSchema { get; set; } = true;

        /// <summary>
        /// Use the fast GUID format (default = True)
        /// </summary>
        public bool UseFastGuid { get; set; } = true;
        
        /// <summary>
        /// Serialize null values to the output (default = True)
        /// </summary>
        public bool SerializeNullValues { get; set; } = true;
        
        /// <summary>
        /// Use the UTC date format (default = True)
        /// </summary>
        public bool UseUTCDateTime { get; set; } = true;
        
        /// <summary>
        /// Show the readonly properties of types in the output (default = False)
        /// </summary>
        public bool ShowReadOnlyProperties { get; set; } = false;
        
        /// <summary>
        /// Use the $types extension to optimise the output json (default = True)
        /// </summary>
        public bool UsingGlobalTypes { get; set; } = true;
        
        /// <summary>
        /// Anonymous types have read only properties 
        /// </summary>
        public bool EnableAnonymousTypes { get; set; } = false;
        
        /// <summary>
        /// Enable fastJSON extensions $types, $type, $map (default = True)
        /// </summary>
        public bool UseExtensions { get; set; } = true;
        
        /// <summary>
        /// Use escaped unicode i.e. \uXXXX format for non ASCII characters (default = True)
        /// </summary>
        public bool UseEscapedUnicode { get; set; } = true;
        
        /// <summary>
        /// Output string key dictionaries as "k"/"v" format (default = False) 
        /// </summary>
        public bool KVStyleStringDictionary { get; set; } = false;
        
        /// <summary>
        /// Output Enum values instead of names (default = False)
        /// </summary>
        public bool UseValuesOfEnums { get; set; } = false;
        
        /// <summary>
        /// Ignore attributes to check for (default : XmlIgnoreAttribute, NonSerialized)
        /// </summary>
        public List<Type> IgnoreAttributes { get; set; } = new List<Type> { typeof(System.Xml.Serialization.XmlIgnoreAttribute), typeof(NonSerializedAttribute) };
        
        /// <summary>
        /// If you have parametric and no default constructor for you classes (default = False)
        /// 
        /// IMPORTANT NOTE : If True then all initial values within the class will be ignored and will be not set
        /// </summary>
        public bool ParametricConstructorOverride { get; set; } = false;
        
        /// <summary>
        /// Serialize DateTime milliseconds i.e. yyyy-MM-dd HH:mm:ss.nnn (default = false)
        /// </summary>
        public bool DateTimeMilliseconds { get; set; } = false;
        
        /// <summary>
        /// Maximum depth for circular references in inline mode (default = 20)
        /// </summary>
        public byte SerializerMaxDepth { get; set; } = 20;
        
        /// <summary>
        /// Inline circular or already seen objects instead of replacement with $i (default = false) 
        /// </summary>
        public bool InlineCircularReferences { get; set; } = false;
        
        /// <summary>
        /// Save property/field names as lowercase (default = false)
        /// </summary>
        public bool SerializeToLowerCaseNames { get; set; } = false;
        
        /// <summary>
        /// Formatter indent spaces (default = 3)
        /// </summary>
        public byte FormatterIndentSpaces { get; set; } = 3;
        
        /// <summary>
        /// TESTING - allow non quoted keys in the json like javascript (default = false)
        /// </summary>
        public bool AllowNonQuotedKeys { get; set; } = false;

        public void FixValues()
        {
            if (UseExtensions == false) // disable conflicting params
            {
                UsingGlobalTypes = false;
                InlineCircularReferences = true;
            }
            if (EnableAnonymousTypes)
                ShowReadOnlyProperties = true;
        }

        internal Parameters MakeCopy() => new Parameters
        {
            AllowNonQuotedKeys = AllowNonQuotedKeys,
            DateTimeMilliseconds = DateTimeMilliseconds,
            EnableAnonymousTypes = EnableAnonymousTypes,
            FormatterIndentSpaces = FormatterIndentSpaces,
            IgnoreAttributes = new List<Type>(IgnoreAttributes),
            InlineCircularReferences = InlineCircularReferences,
            KVStyleStringDictionary = KVStyleStringDictionary,
            ParametricConstructorOverride = ParametricConstructorOverride,
            SerializeNullValues = SerializeNullValues,
            SerializerMaxDepth = SerializerMaxDepth,
            SerializeToLowerCaseNames = SerializeToLowerCaseNames,
            ShowReadOnlyProperties = ShowReadOnlyProperties,
            UseEscapedUnicode = UseEscapedUnicode,
            UseExtensions = UseExtensions,
            UseFastGuid = UseFastGuid,
            UseOptimizedDatasetSchema = UseOptimizedDatasetSchema,
            UseUTCDateTime = UseUTCDateTime,
            UseValuesOfEnums = UseValuesOfEnums,
            UsingGlobalTypes = UsingGlobalTypes
        };
    }
}