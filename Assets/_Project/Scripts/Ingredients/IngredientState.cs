namespace YesChef.Ingredients
{
    /// <summary>
    /// Preparation state of a live ingredient. Kept deliberately binary: an ingredient is
    /// either as it came out of the refrigerator, or it has been through its station.
    /// </summary>
    public enum IngredientState
    {
        Raw,
        Prepared
    }
}
