using UnityEditor;
using UnityEngine;

/* License: (unlicense)
 * Author: PatchworkCoding
 * Description: An easy way to create weighted tables for randomized selection that can be easily edited in the unity inspector
*/

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
