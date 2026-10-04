using UnityEditor;
using UnityEngine;

namespace Lab3.EditorTools
{
    /// <summary>
    /// Запис приватних [SerializeField]-полів зібраних скриптом компонентів.
    /// Без цього посилання в інспекторі лишилися б порожніми після генерації сцени.
    /// </summary>
    public sealed class Lab3Serialized
    {
        private readonly SerializedObject serialized;

        private Lab3Serialized(Object target)
        {
            serialized = new SerializedObject(target);
        }

        public static Lab3Serialized For(Object target)
        {
            return new Lab3Serialized(target);
        }

        public Lab3Serialized Reference(string fieldName, Object value)
        {
            Find(fieldName).objectReferenceValue = value;
            return this;
        }

        public Lab3Serialized Text(string fieldName, string value)
        {
            Find(fieldName).stringValue = value;
            return this;
        }

        public Lab3Serialized Flag(string fieldName, bool value)
        {
            Find(fieldName).boolValue = value;
            return this;
        }

        public Lab3Serialized Number(string fieldName, int value)
        {
            Find(fieldName).intValue = value;
            return this;
        }

        public Lab3Serialized Number(string fieldName, float value)
        {
            Find(fieldName).floatValue = value;
            return this;
        }

        public Lab3Serialized References(string fieldName, params Object[] values)
        {
            SerializedProperty property = Find(fieldName);
            property.arraySize = values.Length;

            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            return this;
        }

        public void Apply()
        {
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private SerializedProperty Find(string fieldName)
        {
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property == null)
            {
                throw new System.ArgumentException(
                    $"Lab3: поле '{fieldName}' не знайдено в {serialized.targetObject.GetType().Name}.", nameof(fieldName));
            }

            return property;
        }
    }
}
