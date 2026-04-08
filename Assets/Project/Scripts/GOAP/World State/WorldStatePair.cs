using System;
using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Represents a fact about the world state as a key-value pair.
    /// Serializable so it can be displayed in the Unity inspector.
    /// </summary>
    [Serializable]
    public sealed class WorldStatePair
    {
        // -----Nested types-----
        public enum ValueType
        {
            Bool = 0,
            Int = 1,
            Float = 2,
        }

        // -----Serialized properties-----
        [SerializeField] private string _key;
        [SerializeField] private ValueType _valueType;
        [SerializeField] private bool _boolValue;
        [SerializeField] private int _intValue;
        [SerializeField] private float _floatValue;

        // -----Public properties-----
        public string Key => _key;

        // -----Public methods-----
        public object GetValue() => _valueType switch
        {
            ValueType.Bool   => _boolValue,
            ValueType.Int    => _intValue,
            ValueType.Float  => _floatValue,
            _                => throw new InvalidOperationException($"[WorldStatePair] has unhandled Value type '{_valueType} on Key '{_key}''"),

        };

        public bool ValueEquals(object other)
        {
            return _valueType switch
            {
                ValueType.Bool      => other is bool b && b == _boolValue,
                ValueType.Int       => other is int i && i == _intValue,
                ValueType.Float     => other is float f && Mathf.Approximately(f, _floatValue),
                _                   => false,
            };
        }

        // -----Constructors-----

        // For creating Boolean-value WorldStatePairs
        public WorldStatePair(string key, bool value)
        {
            _key = key;
            _valueType = ValueType.Bool;
            _boolValue = value;
        }    
        
        // For creating Integer-value WorldStatePairs
        public WorldStatePair(string key, int value)
        {
            _key = key;
            _valueType = ValueType.Int;
            _intValue = value;
        }    
        
        // For creating Float-value WorldStatePairs
        public WorldStatePair(string key, float value)
        {
            _key = key;
            _valueType = ValueType.Float;
            _floatValue = value;
        }

        // -----Diagnostics-----
        public override string ToString() => $"[{_key} = {GetValue()}]";
    }
}

