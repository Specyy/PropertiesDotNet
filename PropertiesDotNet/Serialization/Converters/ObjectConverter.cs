﻿using System;
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
        public BindingFlags MemberFlags
        {
            get => _memberFlags;
            set
            {
                if (value != _memberFlags)
                    ClearCache();

                _memberFlags = value;
            }
        }

        private BindingFlags _memberFlags;

        /// <summary>
        /// Whether to allow search for fields as members or only properties.
        /// </summary>
        public bool AllowFields
        {
            get => _allowFields;
            set
            {
                if (value != _allowFields)
                    ClearCache();

                _allowFields = value;
            }
        }

        private bool _allowFields;

        private readonly Dictionary<Type, Dictionary<string, PropertiesMember>> _memberCache;

        /// <summary>
        /// Creates a new instance of the default object converter.
        /// </summary>
        public ObjectConverter()
        {
            _memberCache = new Dictionary<Type, Dictionary<string, PropertiesMember>>();
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
                var member = GetMember(type, node.Name);
                MemberInfo memberInfo = member?.Info ?? throw new PropertiesException($"Could not find member \"{node.Name}\" within type: {type.FullName}");

                if (node is PropertiesPrimitive primitive)
                {
                    member.SetValue(serializer.ValueProvider, target, serializer.DeserializePrimitive(member.Type, primitive.Value));
                }
                else if (node is PropertiesObject obj)
                {
                    member.SetValue(serializer.ValueProvider, target, serializer.DeserializeObject(member.Type, obj));
                }
                else throw new PropertiesException($"Cannot deserialize tree node of type \"{node.GetType().FullName}\"!");
            }

            return target;
        }

        /// <inheritdoc/>
        public void Serialize(PropertiesSerializer serializer, Type type, object? value, PropertiesObject tree)
        {
            var members = GetMembers(type);

            if (members is null)
                return;

            var entryValues = members.Values;
            foreach (var member in entryValues)
            {
                PropertiesTreeNode node;

                if (serializer.IsPrimitive(member.Type))
                {
                    // TODO: Handle null members (skip, error, or write as primitive null)
                    string pKey = serializer.SerializePrimitive(typeof(string), member.Name)!;
                    string? pValue = serializer.SerializePrimitive(member.Type, member.GetValue(serializer.ValueProvider, value));
                    node = tree.AddPrimitive(pKey, pValue);
                }
                else
                {
                    node = new PropertiesObject(serializer.SerializePrimitive(typeof(string), member.Name));
                    // TODO: Handle null members (skip, error, or write as primitive null)
                    serializer.SerializeObject(member.Type, member.GetValue(serializer.ValueProvider, value), (PropertiesObject)node);
                    tree.Add(node);
                }

                if (member.Comments != null)
                    node.Comments = member.Comments;
            }
        }

        /// <summary>
        /// Clears the cached members for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type members to clear.</param>
        public void ClearCache(Type type)
        {
            _memberCache?.Remove(type);
        }

        /// <summary>
        /// Clears all the  cached members.
        /// </summary>
        public void ClearCache()
        {
            _memberCache?.Clear();
        }

        private PropertiesMember? GetMember(Type type, string name)
        {
            var members = GetMembers(type);

            if (members is null)
                return null;

            return members.TryGetValue(name, out var member) ? member : null;
        }

        private Dictionary<string, PropertiesMember>? GetMembers(Type type)
        {
            if (_memberCache.TryGetValue(type, out var members))
                return members;

            ReadMembers(type);

            if (_memberCache.TryGetValue(type, out Dictionary<string, PropertiesMember> newMembers))
                return newMembers;

            return null;
        }

        private void ReadMembers(Type type)
        {
            foreach (var prop in type.GetProperties(MemberFlags))
            {
                PropertiesMember? member = ReadMember(prop);

                if (member is null)
                    continue;

                UpdateCache(type, member);
            }

            if (AllowFields)
            {
                foreach (var field in type.GetFields(MemberFlags))
                {
                    PropertiesMember? member = ReadMember(field);

                    if (member is null)
                        continue;

                    UpdateCache(type, member);
                }
            }
        }

        private void UpdateCache(Type cacheType, PropertiesMember member)
        {
            if (_memberCache.TryGetValue(cacheType, out var existingMembers))
            {
                existingMembers.Add(member.Name, member);
            }
            else
            {
                _memberCache[cacheType] = new Dictionary<string, PropertiesMember>()
                {
                    { member.Name, member }
                };
            }
        }

        private PropertiesMember? ReadMember(PropertyInfo prop)
        {
            PropertiesMember member;

            var memberAtt = Utils.TypeExtensions.GetCustomAttribute<PropertiesMemberAttribute>(prop);
            if (memberAtt is null)
            {
                if (!prop.CanRead || !prop.CanWrite)
                    return null;

                member = new PropertiesMember(prop.Name, prop.PropertyType, null, prop);
            }
            else
            {
                if (!memberAtt.Serialize)
                    return null;

                member = new PropertiesMember(string.IsNullOrEmpty(memberAtt.Name) ? prop.Name : memberAtt.Name,
                   memberAtt.SerializeAs ?? prop.PropertyType, null, prop);
            }

            ReadComments(member);
            return member;
        }

        private PropertiesMember? ReadMember(FieldInfo field)
        {
            PropertiesMember member;

            var memberAtt = Utils.TypeExtensions.GetCustomAttribute<PropertiesMemberAttribute>(field);
            if (memberAtt is null)
            {
                member = new PropertiesMember(field.Name, field.FieldType, null, field);
            }
            else
            {
                if (!memberAtt.Serialize)
                    return null;

                member = new PropertiesMember(string.IsNullOrEmpty(memberAtt.Name) ? field.Name : memberAtt.Name,
                   memberAtt.SerializeAs ?? field.FieldType, null, field);
            }

            ReadComments(member);
            return member;
        }

        private void ReadComments(PropertiesMember member)
        {
            var commentAtts = Utils.TypeExtensions.GetCustomAttributes<PropertiesCommentAttribute>(member.Info);
            if (commentAtts != null)
            {
                var comments = new List<string>();

                foreach (var commentAtt in commentAtts)
                    comments.Add(commentAtt.Comment);

                member.Comments = comments;
            }
        }

        private class PropertiesMember
        {
            public string Name { get; }
            public Type Type { get; set; }

            public List<string>? Comments { get; set; }
            public MemberInfo Info { get; }

            public PropertiesMember(string name, Type serializeAs, List<string>? comments, MemberInfo info)
            {
                Name = name;
                Type = serializeAs;
                Comments = comments;
                Info = info;
            }

            public object? GetValue(IValueProvider valueProvider, object? target)
            {
                if (Info is PropertyInfo prop)
                {
                    return valueProvider.GetValue(target, prop);
                }
                else if (Info is FieldInfo field)
                {
                    return valueProvider.GetValue(target, field);
                }
                else throw new ArgumentException($"Invalid member: \"{Info.GetType().FullName}\"!");
            }

            public void SetValue(IValueProvider valueProvider, object? target, object? value)
            {
                if (Info is PropertyInfo prop)
                {
                    valueProvider.SetValue(target, prop, value);
                }
                else if (Info is FieldInfo field)
                {
                    valueProvider.SetValue(target, field, value);
                }
                else throw new ArgumentException($"Invalid member: \"{Info.GetType().FullName}\"!");
            }
        }
    }
}