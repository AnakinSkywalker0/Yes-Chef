using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using YesChef.Characters;
using YesChef.Ingredients;

namespace YesChef.Tests
{
    /// <summary>
    /// Builds the minimal scene objects the station rules need - a chef, ingredient
    /// definitions, live ingredients - and tears them all down again after each test.
    /// </summary>
    public abstract class KitchenTestBase
    {
        private readonly List<Object> _created = new();

        protected IngredientSO Vegetable { get; private set; }
        protected IngredientSO Cheese { get; private set; }
        protected IngredientSO Meat { get; private set; }
        protected PlayerController Player { get; private set; }

        [SetUp]
        public void BaseSetUp()
        {
            Vegetable = CreateDefinition("Vegetable", 20, IngredientProcess.Chop);
            Cheese = CreateDefinition("Cheese", 10, IngredientProcess.None);
            Meat = CreateDefinition("Meat", 30, IngredientProcess.Cook);
            Player = CreateComponent<PlayerController>("Player");
        }

        [TearDown]
        public void BaseTearDown()
        {
            for (int i = _created.Count - 1; i >= 0; i--)
            {
                Object target = _created[i];
                if (target == null)
                {
                    continue;
                }

                if (target is Component component)
                {
                    target = component.gameObject;
                }

                Object.DestroyImmediate(target);
            }

            _created.Clear();
        }

        protected T CreateComponent<T>(string name) where T : Component
        {
            var go = new GameObject(name);
            T component = go.AddComponent<T>();
            _created.Add(go);
            return component;
        }

        protected Ingredient CreateIngredient(IngredientSO definition, IIngredientHolder holder, bool prepared = false)
        {
            Ingredient ingredient = CreateComponent<Ingredient>(definition.DisplayName);
            ingredient.Initialise(definition);

            if (prepared)
            {
                ingredient.MarkPrepared();
            }

            ingredient.SetHolder(holder);
            return ingredient;
        }

        protected IngredientSO CreateDefinition(string displayName, int scoreValue, IngredientProcess process)
        {
            var definition = ScriptableObject.CreateInstance<IngredientSO>();
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("_displayName").stringValue = displayName;
            serialized.FindProperty("_scoreValue").intValue = scoreValue;
            serialized.FindProperty("_requiredProcess").enumValueIndex = (int)process;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            _created.Add(definition);
            return definition;
        }

        protected static void SetReference(Object target, string propertyPath, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyPath).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        protected static void SetReferences(Object target, string propertyPath, params Object[] values)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty list = serialized.FindProperty(propertyPath);
            list.arraySize = values.Length;

            for (int i = 0; i < values.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
