﻿using System;

namespace iHentai.Services.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal sealed class StringValueAttribute : Attribute, IValueAttribute
    {
        public StringValueAttribute(string key)
        {
            Key = key;
        }

        public string Key { get; }
        public string Separator { get; set; } = "=";

        public string GetValue(object instance)
        {
            return instance + "";
        }

        public string ToString(object instance)
        {
            return $"{Key}{Separator}{GetValue(instance)}";
        }
    }
}