using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation.Components
{
    internal struct DetailedDropdownItem
    {
        const string k_DisplayNameWithCountFormat = "{0} - [on {1} \u2044 {2} items]";
        const string k_DisplayNameWithPropertyTypeFormat = "{0} / {1}";
        const string k_DisplayNameWithCountAndPropertyTypeFormat = "{0} / {1} - [on {2} \u2044 {3} items]";

        readonly PropertyType m_PropertyType;
        int m_NumberOfOccurrences;
        int m_NumberOfPossibleOccurrences;

        public readonly string propertyKey;

        public DetailedDropdownItem(string propertyKey, PropertyType propertyType, int numberOfPossibleOccurrences = 1)
        {
            this.propertyKey = propertyKey;
            m_NumberOfPossibleOccurrences = numberOfPossibleOccurrences;
            m_PropertyType = propertyType;
            m_NumberOfOccurrences = 1;
        }

        public void IncrementOccurrences()
        {
            m_NumberOfOccurrences++;
        }

        public void SetNumberOfPossibleOccurrences(int value)
        {
            m_NumberOfPossibleOccurrences = value;
        }

        public string GetDisplayName(bool useNestedFormat = false)
        {
            if (m_NumberOfOccurrences < m_NumberOfPossibleOccurrences)
            {
                return GetDisplayNameWithCount(useNestedFormat);
            }

            return GetDisplayNameWithoutCount(useNestedFormat);
        }

        private string GetDisplayNameWithCount(bool useNestedFormat = false)
        {
            if (useNestedFormat)
            {
                return string.Format(k_DisplayNameWithCountAndPropertyTypeFormat, m_PropertyType, propertyKey,
                    m_NumberOfOccurrences, m_NumberOfPossibleOccurrences);
            }

            return string.Format(k_DisplayNameWithCountFormat, propertyKey, m_NumberOfOccurrences,
                m_NumberOfPossibleOccurrences);
        }

        private string GetDisplayNameWithoutCount(bool useNestedFormat = false)
        {
            if (useNestedFormat)
            {
                return string.Format(k_DisplayNameWithPropertyTypeFormat, m_PropertyType, propertyKey);
            }

            return propertyKey;
        }
    }
}
