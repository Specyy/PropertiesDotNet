using System;
using System.Collections.Generic;
using System.Reflection;

using PropertiesDotNet.Core;
using PropertiesDotNet.Serialization.ObjectProviders;
using PropertiesDotNet.Serialization.ValueProviders;
using PropertiesDotNet.Serialization.PropertiesTree;
using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Serialization.Converters
{
    /// <summary>
    /// An object serializer for composite types.
    /// </summary>
    public sealed class ObjectConverter : IPropertiesConverter
    {
        /// <summary>
        /// The bindings flags for properties and fields.
        /// </summary>
        public BindingFlags MemberFlags { get; set; }

        /// <summary>
        /// Whether to allow search for fields as members or only properties.
        /// </summary>
        public bool AllowFields { get; set; }

        /// <summary>
        /// Creates a new instance of the default object converter.
        /// </summary>
        public ObjectConverter()
        {
            AllowFields = true;
            MemberFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
        }

        /// <inheritdoc/>
        public bool Accepts(Type type)
        {
            return !type.IsEnum() && !type.IsArray && !type.IsAbstract() && !type.IsInterface();
        }

        /// <inheritdoc/>
        public object? Deserialize(PropertiesSerializer serializer, Type type, PropertiesObject tree)
        {
            object target = serializer.ObjectProvider.Construct(type);

            foreach (var node in tree)
            {
                MemberInfo member = ((MemberInfo)type.GetProperty(node.Name, MemberFlags) ??
                    (AllowFields ? type.GetField(node.Name, MemberFlags) : null)) ??
                    throw new PropertiesException($"Could not find member \"{node.Name}\" within type: {type.FullName}");

                if (node is PropertiesPrimitive primitive)
                {
                    SetMemberValue(serializer.ValueProvider, target, member, serializer.DeserializePrimitive(GetMemberType(member), primitive.Value));
                }
                else if (node is PropertiesObject obj)
                {
                    SetMemberValue(serializer.ValueProvider, target, member, serializer.DeserializeObject(GetMemberType(member), obj));
                }
                else throw new PropertiesException($"Cannot deserialize tree node of type \"{node.GetType().FullName}\"!");
            }

            return target;
        }

        private void SetMemberValue(IValueProvider valueProvider, object? target, MemberInfo member, object? value)
        {
            if (member is PropertyInfo prop)
            {
                valueProvider.SetValue(target, prop, value);
            }
            else if (member is FieldInfo field)
            {
                valueProvider.SetValue(target, field, value);
            }
            else throw new ArgumentException($"Invalid member: \"{member.GetType().FullName}\"!");
        }

        /// <inheritdoc/>
        public void Serialize(PropertiesSerializer serializer, Type type, object? value, PropertiesObject tree)
        {
            foreach (MemberInfo member in GetValidMembers(type, MemberFlags))
            {
                SerializeMember(serializer, member, value, tree);
            }
        }

        private void SerializeMember(PropertiesSerializer serializer, MemberInfo member, object? target, PropertiesObject tree)
        {
            var memberType = GetMemberType(member);
            if (serializer.IsPrimitive(memberType))
            {
                string? pValue = serializer.SerializePrimitive(memberType, GetMemberValue(serializer.ValueProvider, target, member));
                // TODO: Get attribute - add comments from attribute
                // TODO: Get attribute - get name from attribute
                // Memeber name already string, no conversion needed?
                tree.AddPrimitive(member.Name, pValue);
            }
            else
            {
                // TODO: Get attribute - get name from attribute
                // Memeber name already string, no conversion needed?
                PropertiesObject memberObj = new PropertiesObject(member.Name);
                serializer.SerializeObject(memberType, GetMemberValue(serializer.ValueProvider, target, member), memberObj);
                tree.Add(memberObj);
            }
        }

        private object? GetMemberValue(IValueProvider valueProvider, object? target, MemberInfo member)
        {
            if (member is PropertyInfo prop)
            {
                return valueProvider.GetValue(target, prop);
            }
            else if (member is FieldInfo field)
            {
                return valueProvider.GetValue(target, field);
            }
            else throw new ArgumentException($"Invalid member: \"{member.GetType().FullName}\"!");
        }

        private Type GetMemberType(MemberInfo member)
        {
            if (member is PropertyInfo prop)
            {
                return prop.PropertyType;
            }
            else if (member is FieldInfo field)
            {
                return field.FieldType;
            }
            else throw new ArgumentException($"Invalid member: \"{member.GetType().FullName}\"!");
        }

        private IEnumerable<MemberInfo> GetValidMembers(Type type, BindingFlags flags)
        {
            foreach (var prop in type.GetProperties(flags))
                yield return prop;

            if (AllowFields)
                foreach (var field in type.GetFields(flags))
                    yield return field;
        }
    }
}
