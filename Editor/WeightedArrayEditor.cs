using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(WeightedArray<>))]
public class WeightedArrayEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        if (!property.FindPropertyRelative("m_hasBeenValidated").boolValue)
        {
            EditorGUI.PrefixLabel(position, label);
            EditorGUI.indentLevel++;

            GUIStyle _style = new GUIStyle();
            _style.richText = true;

            EditorGUI.LabelField(new Rect(position.x, position.y + 20, position.width, position.height),
                "<color=red>Weighted Array must call \"Validate\" method from the \"OnValidate\" Method in your " +
                "script</color>", _style);

            EditorGUI.EndProperty();
            return;
        }

        property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, 100, 20),
            property.isExpanded, label);

        if (!property.isExpanded)
        {
            return;
        }

        EditorGUI.indentLevel++;

        int _weightedArrayLength = property.FindPropertyRelative("m_length").intValue;
        float _xPos = (position.x);

        property.FindPropertyRelative("m_length").intValue =
            EditorGUI.IntField(new Rect(_xPos, position.y + 20,
            position.width, 20), "Length", property.FindPropertyRelative("m_length").intValue);

        if (property.FindPropertyRelative("m_values") == null)
        {
            return;
        }

        if (_weightedArrayLength <= 0 || _weightedArrayLength != property.FindPropertyRelative("m_values").arraySize)
        {
            return;
        }

        if (!property.FindPropertyRelative("m_values").GetArrayElementAtIndex(0).hasVisibleChildren)
        {
            SingleVarWeightedArrayElements(position, property, _weightedArrayLength);
        }
        else
        {
            MultiVarWeightedArrayElements(position, property, _weightedArrayLength);
        }

        if (!property.FindPropertyRelative("m_isWeightSumFulfilled").boolValue)
        {
            float _yPos = position.y + GetPropertyHeight(property, label) - 20;
            Vector2 _labelPos = new Vector2(15, _yPos);

            Vector2 _labelSize = new Vector2(position.width * 0.5f, 20);
            EditorGUI.LabelField(new Rect(_labelPos, _labelSize),
                new GUIContent("Weights must equal 100"));

            EditorGUI.PropertyField(new Rect(position.width * 0.5f, _yPos, position.width * 0.5f, 20),
                    property.FindPropertyRelative("m_doAutoBalance"),
                    new GUIContent("Click to Auto Balance ->"));
        }

        EditorGUI.EndProperty();
    }

    //Draws variables that can if on 1 line
    public void SingleVarWeightedArrayElements(Rect position, SerializedProperty property, int _weightedArrayLength)
    {
        float _width = position.width;
        float _xPos = (position.x);

        for (int i = 0; i < _weightedArrayLength; i++)
        {
            float _yPos = position.y + (20 * i) + 40;

            Vector2 _valuePos = new Vector2(_xPos, _yPos);
            Vector2 _valueSize = new Vector2(_width * 0.25f, 20);
            EditorGUI.PropertyField(new Rect(_valuePos, _valueSize),
                property.FindPropertyRelative("m_values").GetArrayElementAtIndex(i), GUIContent.none);

            Vector2 _weightPos = new Vector2(_xPos + (_width * 0.25f), _yPos);
            Vector2 _weightSize = new Vector2((_width * 0.5f), 20);
            EditorGUI.IntSlider(new Rect(_weightPos, _weightSize),
                property.FindPropertyRelative("m_weights").GetArrayElementAtIndex(i),
                0, 100, GUIContent.none);

            Vector2 _labelPos = new Vector2(_xPos + (_width * 0.75f), _yPos);
            Vector2 _labelSize = new Vector2(_width * 0.25f, 20);
            EditorGUI.LabelField(new Rect(_labelPos, _labelSize),
                property.FindPropertyRelative("m_weightLabel").GetArrayElementAtIndex(i).stringValue);

        }
    }

    //Draws Structs, Vectors, Rect, and other custom serialized classes with
    //Multiple children variables
    public void MultiVarWeightedArrayElements(Rect position, SerializedProperty property, int _weightedArrayLength)
    {
        float _width = position.width;
        float _xPos = (position.x);

        float _yPos = position.y + 45;
        for (int i = 0; i < _weightedArrayLength; i++)
        {
            Vector2 _valuePos = new Vector2(_xPos, _yPos);
            Vector2 _valueSize = new Vector2(_width, 20);
            EditorGUI.PropertyField(new Rect(_valuePos, _valueSize),
                property.FindPropertyRelative("m_values").GetArrayElementAtIndex(i), true);

            _yPos += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_values").GetArrayElementAtIndex(i));

            Vector2 _weightPos = new Vector2(_xPos, _yPos);
            Vector2 _weightSize = new Vector2((_width * 0.75f), 20);
            EditorGUI.IntSlider(new Rect(_weightPos, _weightSize),
                property.FindPropertyRelative("m_weights").GetArrayElementAtIndex(i),
                0, 100, new GUIContent("Weight " + i));

            Vector2 _labelPos = new Vector2(_xPos + (_width * 0.75f), _yPos);
            Vector2 _labelSize = new Vector2(_width * 0.25f, 20);
            EditorGUI.LabelField(new Rect(_labelPos, _labelSize),
                property.FindPropertyRelative("m_weightLabel").GetArrayElementAtIndex(i).stringValue);

            _yPos += 30;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float _propertyHeight = 0;

        if (property.FindPropertyRelative("m_hasBeenValidated").boolValue && property.isExpanded)
        {
            if (property.FindPropertyRelative("m_length").intValue == 0)
            {
                return 40;
            }

            if (property.FindPropertyRelative("m_values") != null)
            {
                if (property.FindPropertyRelative("m_values").arraySize > 0 &&
                    property.FindPropertyRelative("m_values").GetArrayElementAtIndex(0).hasVisibleChildren)
                {
                    _propertyHeight = 40;
                    //Debug.Log("sup!");
                    for (int i = 0; i < property.FindPropertyRelative("m_values").arraySize; i++)
                    {
                        _propertyHeight += EditorGUI.GetPropertyHeight(
                            property.FindPropertyRelative("m_values").GetArrayElementAtIndex(i));

                        _propertyHeight += 30;
                    }

                    if (!property.FindPropertyRelative("m_values").GetArrayElementAtIndex(0).isExpanded)
                    {
                        _propertyHeight += 5;
                    }
                }

                else
                {
                    _propertyHeight = 20 * (property.FindPropertyRelative("m_length").intValue) + 40;
                }
            }

            if (!property.FindPropertyRelative("m_isWeightSumFulfilled").boolValue)
            {
                _propertyHeight += 25;
            }
        }
        else
        {
            _propertyHeight = 20;
        }

        return _propertyHeight;
    }
}
