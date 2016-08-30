using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using NLog;

namespace RopeSnake.Core.Validation
{
    public static class Validator
    {
        private class AttributeBundle
        {
            public ValidateAttribute Validate { get; private set; }
            public ValidateRuleBaseAttribute[] InstanceRules { get; private set; }
            public ValidateRuleBaseAttribute[] CollectionRules { get; private set; }
            public ValidateRuleBaseAttribute[] DictionaryKeyRules { get; private set; }
            public ValidateRuleBaseAttribute[] DictionaryValueRules { get; private set; }

            public AttributeBundle(IEnumerable<ValidateBaseAttribute> attributes)
            {
                Validate = attributes.FirstOrDefault(a => a.GetType() == typeof(ValidateAttribute)) as ValidateAttribute;
                var rules = attributes.OfType<ValidateRuleBaseAttribute>();
                InstanceRules = rules.Where(a => a.Flags.HasFlag(ValidateFlags.Instance)).ToArray();
                CollectionRules = rules.Where(a => a.Flags.HasFlag(ValidateFlags.Collection)).ToArray();
                DictionaryKeyRules = rules.Where(a => a.Flags.HasFlag(ValidateFlags.DictionaryValues)).ToArray();
                DictionaryValueRules = rules.Where(a => a.Flags.HasFlag(ValidateFlags.DictionaryKeys)).ToArray();
            }
        }

        private static readonly Type _typeOfCollection = typeof(ICollection);
        private static readonly Type _typeOfDictionary = typeof(IDictionary);

        private static Dictionary<Type, ValidateAttribute> _typeAttributes
            = new Dictionary<Type, ValidateAttribute>();

        private static Dictionary<Type, PropertyInfo[]> _typePropertyInfo
            = new Dictionary<Type, PropertyInfo[]>();

        private static Dictionary<PropertyInfo, AttributeBundle> _propertyAttributes
            = new Dictionary<PropertyInfo, AttributeBundle>();

        private static ValidateAttribute GetTypeAttribute(Type type)
        {
            ValidateAttribute attribute;
            if (_typeAttributes.TryGetValue(type, out attribute))
                return attribute;

            attribute = type.GetCustomAttribute<ValidateAttribute>(true);
            if (attribute != null && attribute.Flags != ValidateFlags.None)
            {
                throw new Exception("Type may not be decorated with a ValidateAttribute using flags other than None");
            }

            _typeAttributes.Add(type, attribute);
            return attribute;
        }

        private static PropertyInfo[] GetTypePropertyInfo(Type type)
        {
            PropertyInfo[] properties;
            if (_typePropertyInfo.TryGetValue(type, out properties))
                return properties;

            properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            _typePropertyInfo.Add(type, properties);
            return properties;
        }

        private static AttributeBundle GetPropertyAttributes(PropertyInfo property)
        {
            AttributeBundle bundle;
            if (_propertyAttributes.TryGetValue(property, out bundle))
                return bundle;

            var attributes = property.GetCustomAttributes<ValidateBaseAttribute>();
            bundle = null;
            if (attributes.Any())
            {
                bundle = new AttributeBundle(attributes);
            }
            _propertyAttributes.Add(property, bundle);
            return bundle;
        }

        public static bool Object(object value, LazyString path, Logger log)
        {
            bool success = true;

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var type = value.GetType();

            if (GetTypeAttribute(type) == null)
                return success;

            var properties = GetTypePropertyInfo(type);
            foreach (var property in properties)
            {
                success &= ValidateProperty(value, path, property, log);
            }

            return success;
        }

        private static bool ValidateProperty(object owner, LazyString ownerPath, PropertyInfo property, Logger log)
        {
            var value = property.GetValue(owner);
            var attributes = GetPropertyAttributes(property);

            if (attributes != null)
            {
                return ValidateObjectWithAttributes(value, ownerPath.Append($".{property.Name}"), attributes, log);
            }
            else
            {
                return true;
            }
        }

        private static bool ValidateObjectWithAttributes(object value, LazyString path, AttributeBundle attributes, Logger log)
        {
            bool success = true;

            // Get all rule attributes for various flags
            var instanceRules = attributes.InstanceRules;
            var collectionRules = attributes.CollectionRules;
            var dictionaryKeyRules = attributes.DictionaryKeyRules;
            var dictionaryValueRules = attributes.DictionaryValueRules;

            // For the instance, run all instance attributes
            foreach (var instanceRule in instanceRules)
            {
                success &= instanceRule.Validate(value, path, log);
            }

            // Check if it's a collection/dictionary
            ICollection collection = null;
            IDictionary dictionary = null;

            if (value != null)
            {
                var type = value.GetType();

                if (_typeOfCollection.IsAssignableFrom(type))
                {
                    // If it's a collection, run all collection rules
                    collection = value as ICollection;
                    int index = 0;
                    foreach (object element in collection)
                    {
                        var elementPath = path.Append(() => GetCollectionIndexer(element, index));
                        foreach (var collectionRule in collectionRules)
                        {
                            success &= collectionRule.Validate(element, elementPath, log);
                        }
                        index++;
                    }
                }

                if (_typeOfDictionary.IsAssignableFrom(type))
                {
                    // If it's a dictionary, run all key and value rules
                    dictionary = value as IDictionary;
                    foreach (var key in dictionary.Keys)
                    {
                        var keyPath = path.Append(() => GetDictionaryKeyIndexer(key));
                        foreach (var keyRule in dictionaryKeyRules)
                        {
                            success &= keyRule.Validate(key, keyPath, log);
                        }

                        var element = dictionary[key];
                        var valuePath = path.Append(() => GetDictionaryValueIndexer(element, key));
                        foreach (var valueRule in dictionaryValueRules)
                        {
                            success &= valueRule.Validate(element, valuePath, log);
                        }
                    }
                }
            }

            // Recursively validate the instance
            var validateAttribute = attributes.Validate;
            if (validateAttribute != null)
            {
                if (validateAttribute.Flags.HasFlag(ValidateFlags.Instance))
                {
                    success &= Validator.Object(value, path, log);
                }

                if (collection != null && validateAttribute.Flags.HasFlag(ValidateFlags.Collection))
                {
                    int index = 0;
                    foreach (object element in collection)
                    {
                        if (element != null)
                        {
                            var elementPath = path.Append(() => GetCollectionIndexer(element, index));
                            success &= Validator.Object(element, elementPath, log);
                        }
                        index++;
                    }
                }

                if (dictionary != null)
                {
                    bool doKeys = validateAttribute.Flags.HasFlag(ValidateFlags.DictionaryKeys);
                    bool doValues = validateAttribute.Flags.HasFlag(ValidateFlags.DictionaryValues);

                    if (doKeys || doValues)
                    {
                        foreach (var key in dictionary.Keys)
                        {
                            if (doKeys)
                            {
                                var keyPath = path.Append(() => GetDictionaryKeyIndexer(key));
                                success &= Validator.Object(key, keyPath, log);
                            }

                            if (doValues)
                            {
                                var element = dictionary[key];
                                if (element != null)
                                {
                                    var elementPath = path.Append(() => GetDictionaryValueIndexer(element, key));
                                    success &= Validator.Object(element, elementPath, log);
                                }
                            }
                        }
                    }
                }
            }

            return success;
        }

        private static string GetCollectionIndexer(object value, int index)
        {
            var nameHint = value as INameHint;
            if (nameHint != null)
            {
                return $"[{index} \"{nameHint.NameHint}\"]";
            }
            return $"[{index}]";
        }

        private static string GetDictionaryKeyIndexer(object key)
        {
            return $".Keys[{key}]";
        }

        private static string GetDictionaryValueIndexer(object value, object key)
        {
            return $"[{key}]";
        }
    }
}