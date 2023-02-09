using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* License: (unlicense)
 * Author: PatchworkCoding
 * Description: An easy way to create weighted tables for randomized selection that can be easily edited in the unity inspector
*/

namespace PatchworkCoding.WeightedArray
{
    [System.Serializable]
    public class WeightedArray<T>
    {
        [SerializeField]
        T[] m_values = null;
        //[Range(0, 100)]
        [SerializeField]
        int[] m_weights = null;
        [SerializeField]
        string[] m_weightLabel = null;
        [SerializeField]
        int m_length = 0;
        [SerializeField]
        bool m_hasBeenValidated = false;
        [SerializeField]
        bool m_isWeightSumFulfilled = false;
        [SerializeField]
        bool m_doAutoBalance = false;
        int[] m_previousWeightValues = null;


        /// <summary>
        /// Updates weight values as they are changed in the editor 
        /// </summary>
        public void Validate()
        {
            m_hasBeenValidated = true;

            if (m_doAutoBalance)
            {
                AutoBalance();
                GenerateWeightLabels();
                SetPreviousValues();
                return;
            }

            SetLength();
            RebalanceWeights();
            GenerateWeightLabels();
            SetPreviousValues();
        }

        //Manage the length of all arrays
        private void SetLength()
        {
            if (m_length > 100)
            {
                m_length = 100;
            }
            else if (m_length <= 0)
            {
                m_length = 0;
            }

            if (m_length == 0 || (m_values != null && m_values.Length == m_length))
            {
                return;
            }

            m_values = ResizeArray(m_values, m_length, true);
            m_weights = ResizeArray(m_weights, m_length);
            m_weightLabel = ResizeArray(m_weightLabel, m_length);

            if (m_previousWeightValues == null || m_previousWeightValues.Length != m_weights.Length)
            {
                m_previousWeightValues = new int[m_weights.Length];
                SetPreviousValues();
            }

        }

        //Manages the weight values of the array
        private void RebalanceWeights()
        {
            //Sets previous values if they aren't already set
            if (m_previousWeightValues == null || m_previousWeightValues.Length != m_weights.Length)
            {
                m_previousWeightValues = new int[m_weights.Length];
                SetPreviousValues();
                return;
            }


            int _changedIndex = 0;
            int _weightSum = 0;
            //Checks to see if any of the weight values have changed
            for (int i = 0; i < m_previousWeightValues.Length; i++)
            {
                if (m_previousWeightValues[i] != m_weights[i])
                {
                    if (Mathf.Sign(m_weights[i] - m_previousWeightValues[i]) > 0)
                    {
                        _changedIndex = i;
                    }
                }
                else
                {
                    _weightSum += m_weights[i];
                }
            }

            if (_weightSum + m_weights[_changedIndex] < 100)
            {
                m_isWeightSumFulfilled = false;
            }
            else
            {
                m_isWeightSumFulfilled = true;
            }


            int _valuesToRedistro = _weightSum + m_weights[_changedIndex] - 100;
            int _iterationSinceLastChange = 0;

            //If a value has changed and it the weight sum is greater than 100...
            if (_weightSum > 0 && _weightSum + m_weights[_changedIndex] > 100)
            {
                //Redistribute the weights until the sum is equal to 100
                int _index = _changedIndex;
                while (_valuesToRedistro > 0 &&
                    _iterationSinceLastChange < m_weights.Length - 1)
                {
                    _index++;
                    if (_index == _changedIndex)
                    {
                        _index++;
                    }

                    if (_index >= m_weights.Length)
                    {
                        _index = 0;
                    }

                    if (m_weights[_index] != 0)
                    {
                        m_weights[_index] -= 1;
                        _valuesToRedistro--;
                        _iterationSinceLastChange = 0;
                    }
                    else
                    {
                        _iterationSinceLastChange++;
                    }
                }

                if (_iterationSinceLastChange >= m_weights.Length - 1)
                {
                    m_weights[_changedIndex] = 100;
                }
            }

            //Clamp all weight values to be between 0 - 100
            int _weightTotal = 0;
            for (int i = 0; i < m_weights.Length; i++)
            {
                if (_weightTotal == 100)
                {
                    m_weights[i] = 0;
                }
                else if (_weightTotal + m_weights[i] > 100)
                {
                    m_weights[i] = m_weights[i] - ((m_weights[i] + _weightTotal) - 100);

                    _weightTotal = 100;
                }
                else
                {
                    _weightTotal += m_weights[i];
                }
            }
        }

        //sets the values of m_previousWeightValues equal to the values of m_weights
        //This is so the array can be deep copied. = operator only shallow copies arrays
        private void SetPreviousValues()
        {
            for (int i = 0; i < m_weights.Length; i++)
            {
                m_previousWeightValues[i] = m_weights[i];
            }
        }

        //Generates the text that will be displayed beside weight values in the editor
        private void GenerateWeightLabels()
        {
            int _weightLabelTotal = 1;
            for (int i = 0; i < m_weights.Length; i++)
            {
                if (m_weights[i] == 0)
                {
                    m_weightLabel[i] = "not included";
                }
                else
                {
                    m_weightLabel[i] = _weightLabelTotal + " - " + (_weightLabelTotal + m_weights[i] - 1);
                }
                _weightLabelTotal += m_weights[i];
            }
        }

        //Resizes an array by copying values from one array into another array of the appropite size
        private T1[] ResizeArray<T1>(T1[] _arrayToResize, int _length, bool _copyLastValue = false)
        {
            T1[] _resizedArray = new T1[_length];

            if (_arrayToResize == null || (_length == 0 || _arrayToResize.Length == 0))
            {
                return new T1[_length];
            }


            for (int i = 0; i < _length; i++)
            {
                if (i < _arrayToResize.Length)
                {
                    _resizedArray[i] = _arrayToResize[i];
                }
                else if (_copyLastValue)
                {
                    _resizedArray[i] = _arrayToResize[_arrayToResize.Length - 1];
                }
                else
                {
                    break;
                }
            }
            return _resizedArray;
        }

        /// <summary>
        /// Gets a random element from the array with consideration to its weight
        /// </summary>
        public T GetElement()
        {
            int _rnd = Random.Range(0, 100);
            int _curThreshold = 0;

            for (int i = 0; i < m_weights.Length; i++)
            {
                _curThreshold += m_weights[i];

                if (_rnd < _curThreshold)
                {
                    return m_values[i];
                }
            }

            Debug.LogError("Something went wrong in array bounds");
            return m_values[0];
        }

        /// <summary>
        /// Gets an element at "i" index of the array
        /// </summary>
        public T GetElementAtIndex(int i)
        {
            return m_values[i];
        }

        //Sets the sum of all weight values to be equal to 100 by evenly distrubeting the difference
        //for example sum of all weight values is 88 than the remaining 22 points will be evenly distributed among all entries
        private void AutoBalance()
        {
            int _total = 0;
            for (int i = 0; i < m_weights.Length; i++)
            {
                _total += m_weights[i];
            }

            int _redistro = 100 - _total;
            int _index = 0;
            while (_redistro > 0)
            {
                m_weights[_index]++;

                _index++;
                if (_index >= m_weights.Length)
                {
                    _index = 0;
                }

                _redistro--;
            }

            m_doAutoBalance = false;
            m_isWeightSumFulfilled = true;
        }

        /// <summary>
        /// The length of the array
        /// </summary>
        public int Length
        {
            get { return m_length; }
        }
    }

#if UNITY_EDITOR
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
#endif
}
