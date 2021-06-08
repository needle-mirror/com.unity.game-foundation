using System;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    [CustomPropertyDrawer(typeof(Property))]
    class GameFoundationPropertyDrawer : PropertyDrawer
    {
        ResourcesAssetDrawer m_ResourcesAssetDrawer;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            const float valueWidthRatio = 0.6f;
            const float margin = 5;

            var nonLabelWidth = position.width - EditorGUIUtility.labelWidth;
            var originalWidth = position.width;
            var propertyType = property.FindPropertyRelative($"<{nameof(Property.type)}>k__BackingField");
            var builtInValue = property.FindPropertyRelative(nameof(Property.m_ValueType))
                .FindPropertyRelative(nameof(CoalescedValueType.longValue));
            var stringValue = property.FindPropertyRelative(nameof(Property.m_StringValue));

            //Value is drawn first to have the label functionalities.
            //Example: hold and drag on label to change numeric values.
            position.width = EditorGUIUtility.labelWidth + nonLabelWidth * valueWidthRatio;
            switch (propertyType.intValue)
            {
                case (int)PropertyType.Long:
                {
                    builtInValue.longValue = EditorGUI.LongField(position, label, builtInValue.longValue);

                    break;
                }

                case (int)PropertyType.Double:
                {
                    var doubleValue = BitConverter.Int64BitsToDouble(builtInValue.longValue);
                    doubleValue = EditorGUI.DoubleField(position, label, doubleValue);
                    builtInValue.longValue = BitConverter.DoubleToInt64Bits(doubleValue);

                    break;
                }

                case (int)PropertyType.Bool:
                {
                    builtInValue.boolValue = EditorGUI.Toggle(position, label, builtInValue.boolValue);

                    break;
                }

                case (int)PropertyType.String:
                {
                    stringValue.stringValue = EditorGUI.TextField(position, label, stringValue.stringValue);

                    break;
                }

                case (int)PropertyType.Addressables:
                {
                    stringValue.stringValue = EditorGUI.TextField(position, label, stringValue.stringValue);

                    break;
                }

                case (int)PropertyType.ResourcesAsset:
                {
                    if (m_ResourcesAssetDrawer == null)
                    {
                        m_ResourcesAssetDrawer = new ResourcesAssetDrawer();
                    }

                    stringValue.stringValue = m_ResourcesAssetDrawer.Draw(position, label, stringValue.stringValue, property.serializedObject.targetObject);

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            //Type
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                position.x += position.width + margin;
                position.width = originalWidth - margin - position.width;
                propertyType.enumValueIndex = EditorGUI.Popup(
                    position,
                    propertyType.enumValueIndex, propertyType.enumDisplayNames);

                //Reset string value when type is changed
                if (changeScope.changed
                    && propertyType.intValue != (int)PropertyType.String
                    && propertyType.intValue != (int)PropertyType.Addressables
                    && propertyType.intValue != (int)PropertyType.ResourcesAsset)
                {
                    stringValue.stringValue = null;
                }
            }
        }
    }
}
