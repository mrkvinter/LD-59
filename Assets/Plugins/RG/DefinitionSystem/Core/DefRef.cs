using System;
using UnityEngine;

namespace RG.DefinitionSystem.Core
{
    [Serializable]
    public struct DefRef<T> : IEquatable<DefRef<T>> where T : Definition
    {
        [SerializeField] private string id;

        public string Id => id;
        public Type Type => typeof(T);
        public bool IsEmpty => string.IsNullOrEmpty(id);

        public DefRef(string id)
        {
            this.id = id;
        }

        public bool Equals(DefRef<T> other)
        {
            return id == other.id;
        }

        public override bool Equals(object obj)
        {
            return obj is DefRef<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (id != null ? id.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return $"[{Type.Name}] '{id}'";
        }

        public static implicit operator DefRef<T>(string id) => new(id);

        public static bool operator ==(DefRef<T> left, DefRef<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DefRef<T> left, DefRef<T> right)
        {
            return !left.Equals(right);
        }
    }
}