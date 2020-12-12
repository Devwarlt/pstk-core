using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;

namespace PSTk.Extensions.Utils
{
    /// <summary>
    /// Contains XML utilities.
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Get value from <paramref name="node"/> by <paramref name="attribute"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <exception cref="NotSupportedException"></exception>
        /// <returns></returns>
        public static T GetAttribute<T>(this XElement node, string attribute)
        {
            if (node.Attribute(attribute) == null)
                throw new ArgumentNullException($"Null value from attribute: {attribute}");

            var val = node.Attribute(attribute).Value;
            var t = typeof(T);
            if (t == typeof(string))
                return (T)Convert.ChangeType(val, t);
            else if (t == typeof(byte))
                return (T)Convert.ChangeType(Convert.ToByte(val), t);
            else if (t == typeof(int))
                return (T)Convert.ChangeType(val.GetInt(), t);
            else if (t == typeof(ushort))
                return (T)Convert.ChangeType(Convert.ToUInt16(val, 16), t);
            else if (t == typeof(uint))
                return (T)Convert.ChangeType(val.GetUint(), t);
            else if (t == typeof(long))
                return (T)Convert.ChangeType(long.Parse(val, CultureInfo.InvariantCulture), t);
            else if (t == typeof(double))
                return (T)Convert.ChangeType(double.Parse(val, CultureInfo.InvariantCulture), t);
            else if (t == typeof(float))
                return (T)Convert.ChangeType(float.Parse(val, CultureInfo.InvariantCulture), t);
            else if (t == typeof(bool))
                return (T)Convert.ChangeType(string.IsNullOrWhiteSpace(val) || bool.Parse(val), t);
            else
                throw new NotSupportedException($"Type of {t} is not supported by this method!");
        }

        /// <summary>
        /// Get value from <paramref name="node"/> by <paramref name="element"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="element"></param>
        /// <exception cref="NotSupportedException"></exception>
        /// <returns></returns>
        public static T GetValue<T>(this XElement node, string element)
        {
            if (node.Element(element) == null)
                throw new ArgumentNullException($"Null value from element: {element}");

            var val = node.Element(element).Value;
            var t = typeof(T);
            if (t == typeof(string))
                return (T)Convert.ChangeType(val, t);
            else if (t == typeof(byte))
                return (T)Convert.ChangeType(Convert.ToByte(val), t);
            else if (t == typeof(int))
                return (T)Convert.ChangeType(val.GetInt(), t);
            else if (t == typeof(ushort))
                return (T)Convert.ChangeType(Convert.ToUInt16(val, 16), t);
            else if (t == typeof(uint))
                return (T)Convert.ChangeType(val.GetUint(), t);
            else if (t == typeof(long))
                return (T)Convert.ChangeType(long.Parse(val, CultureInfo.InvariantCulture), t);
            else if (t == typeof(double))
                return (T)Convert.ChangeType(double.Parse(val, CultureInfo.InvariantCulture), t);
            else if (t == typeof(float))
                return (T)Convert.ChangeType(float.Parse(val, CultureInfo.InvariantCulture), t);
            else if (t == typeof(bool))
                return (T)Convert.ChangeType(string.IsNullOrWhiteSpace(val) || bool.Parse(val), t);
            else
                throw new NotSupportedException($"Type of {t} is not supported by this method!");
        }

        /// <summary>
        /// Return all <paramref name="node"/> from <paramref name="parent"/>.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> GetXmlsByNode(this XElement parent, string node)
        {
            foreach (var element in parent.XPathSelectElements($"//{node}"))
                yield return element;
        }

        /// <summary>
        /// Return all <paramref name="node"/> from <paramref name="parent"/> filtered by <paramref name="value"/>.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="node"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> GetXmlsByNode(this XElement parent, string node, string value)
        {
            foreach (var element in parent.XPathSelectElements($"//{node}"))
                if (element.Value.Equals(value))
                    yield return element.Parent;
        }

        /// <summary>
        /// Check if <paramref name="node"/> has <paramref name="attribute"/>.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static bool HasAttribute(this XElement node, string attribute) => node.Attribute(attribute) != null;

        /// <summary>
        /// Check if <paramref name="node"/> has <paramref name="element"/>.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool HasElement(this XElement node, string element) => node.Element(element) != null;

        private static int GetInt(this string x) => x.Contains("x") ? Convert.ToInt32(x, 16) : int.Parse(x);

        private static uint GetUint(this string x) => x.Contains("x") ? Convert.ToUInt32(x, 16) : uint.Parse(x);
    }
}
