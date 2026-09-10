namespace YesChef.Ingredients
{
    /// <summary>Preparation an ingredient needs before it can be delivered to an order.</summary>
    public enum IngredientProcess
    {
        /// <summary>Ready straight out of the refrigerator (cheese).</summary>
        None,

        /// <summary>Must be chopped on a table (vegetables).</summary>
        Chop,

        /// <summary>Must be cooked on a stove (meat).</summary>
        Cook
    }
}
